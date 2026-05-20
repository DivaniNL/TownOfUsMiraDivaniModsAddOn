using System.Collections;
using AmongUs.GameOptions;
using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using MiraAPI.Utilities;
using Reactor.Utilities;
using DivaniMods.Assets;
using DivaniMods.Buttons.Neutral.NeutralKilling;
using DivaniMods.Roles.Neutral.NeutralKilling;
using DivaniMods.Utilities;
using TownOfUs.Modules;
using TownOfUs.Networking;
using UnityEngine;

namespace DivaniMods.Patches;

public static class FragBombState
{
    public const string TimerId = "divani.frag_bomb";
    private const int TimerPriority = 20;

    private const byte NoPlayer = byte.MaxValue;

    public static bool IsActive { get; private set; }
    public static bool IsArmed { get; private set; }
    public static byte FragId { get; private set; } = NoPlayer;
    public static byte HolderId { get; private set; } = NoPlayer;
    public static byte ImmuneId { get; private set; } = NoPlayer;
    public static float TimeRemaining { get; private set; }
    private static float ArmingRemaining;

    private static bool _tickRunning;
    private static bool _explosionTriggered;

    private static int _beforeMurderShieldProbeDepth;

    private static AudioSource? _heartbeatSource;
    private static bool _heartbeatUnavailable;
    private static byte _heartbeatOwnerId = NoPlayer;
    private static Sprite? _cachedFragRoleIcon;

    public static bool IsHolder(byte playerId) => IsActive && HolderId == playerId;
    public static bool IsImmune(byte playerId) => IsActive && ImmuneId == playerId;

    public static float GetSecondsUntilExplosionForDisplay()
    {
        if (!IsActive) return 0f;
        if (!IsArmed) return ArmingRemaining + TimeRemaining;
        return TimeRemaining;
    }

    public static bool LocalFragShouldBlockKill()
    {
        var local = PlayerControl.LocalPlayer;
        return IsActive && local != null && local.Data?.Role is FragRole;
    }

    public static void PassBomb(PlayerControl sender, byte targetId, byte immuneId, float duration, float armingDelay)
    {
        var wasActive = IsActive;

        IsActive = true;
        if (!wasActive)
        {
            FragId = sender.PlayerId;
            TimeRemaining = duration;
            ArmingRemaining = Mathf.Clamp(armingDelay, 2f, 7f);
            IsArmed = false;
            _explosionTriggered = false;
        }
        else
        {
            // A pass between holders keeps the same countdown and is armed
            // instantly: button, heartbeat and HUD all appear immediately for
            // the new holder.
            IsArmed = true;
            ArmingRemaining = 0f;
        }

        HolderId = targetId;
        ImmuneId = immuneId;

        // Force the heartbeat off immediately so we never get a half-frame of
        // audio leaking from the previous holder or a stale source.
        StopHeartbeat();

        StartTickIfNeeded();
        RefreshFragButtons();
        SyncFragTimerRow();
    }

    public static void Clear(bool startGiveCooldown = false)
    {
        var wasActive = IsActive;
        IsActive = false;
        IsArmed = false;
        FragId = NoPlayer;
        HolderId = NoPlayer;
        ImmuneId = NoPlayer;
        TimeRemaining = 0f;
        ArmingRemaining = 0f;
        _explosionTriggered = false;

        StopHeartbeat();
        DivaniTimers.Remove(TimerId);

        if (startGiveCooldown && wasActive)
        {
            FragGiveBombButton.StartCooldown();
        }

        RefreshFragButtons();
    }

    public static void ClearForMeeting()
    {
        // Calling a meeting (or reporting a body) detonates the bomb on the current
        // holder. We capture the IDs, clear bomb state immediately so the countdown/HUD
        // stops, and schedule a coroutine that fires the murder via RpcCustomMurder
        // shortly after the meeting intro animation completes. The RPC syncs to every
        // client and TouMira's HandleMeetingMurder plays the kill animation in-cabin.
        var holderIdCapture = HolderId;
        var fragIdCapture = FragId;
        var shouldKill = IsActive;

        Clear(startGiveCooldown: false);

        if (shouldKill)
        {
            Coroutines.Start(CoExplodeInMeeting(holderIdCapture, fragIdCapture));
        }
    }

    private static IEnumerator CoExplodeInMeeting(byte holderIdCapture, byte fragIdCapture)
    {
        // Wait until the meeting cabin's intro animation finishes so the kill
        // plays as a proper in-meeting murder (Assassin-style cabin animation
        // is wired up by TouMira's HandleMeetingMurder via AfterMurderEvent).
        while (MeetingHud.Instance &&
               MeetingHud.Instance.state == MeetingHud.VoteStates.Animating)
        {
            yield return null;
        }

        if (!MeetingHud.Instance) yield break;

        yield return new WaitForSeconds(0.25f);

        // Only the host kicks off the RPC so the kill resolves exactly once.
        if (!AmongUsClient.Instance.AmHost) yield break;

        var holder = PlayerById(holderIdCapture);
        if (holder == null || holder.Data == null || holder.Data.IsDead) yield break;

        var frag = PlayerById(fragIdCapture);
        var source = (frag != null && frag.Data != null && !frag.Data.IsDead) ? frag : holder;

        // TouMira's HandleMeetingMurder hides the guess-crosshair button for the
        // dead player on every screen *except* the murderer's, because assassins
        // normally call HideSingle themselves inside their guess click handler.
        // Frag's bomb doesn't go through that flow, so do it explicitly on the
        // source's client so they can't still target the player they just bombed.
        if (source.AmOwner)
        {
            MeetingMenu.Instances.Do(x => x.HideSingle(holder.PlayerId));
        }

        // MeetingCheck.ForMeeting matches Assassin's guess kill — required so the
        // RPC isn't cancelled by MiraAPI's CustomMurder meeting filter. The cabin
        // death animation is driven by TouMira's HandleMeetingMurder, so we skip
        // showKillAnim/teleport here and just make sure the body is created for
        // post-meeting reports.
        source.RpcSpecialMurder(
            holder,
            MeetingCheck.ForMeeting,
            isIndirect: true,
            ignoreShield: false,
            didSucceed: true,
            resetKillTimer: false,
            createDeadBody: true,
            teleportMurderer: false,
            showKillAnim: false,
            playKillSound: true,
            causeOfDeath: "BomberBomb");
    }

    public static void DestroyHud() => DivaniTimers.Remove(TimerId);

    private static void StartTickIfNeeded()
    {
        if (_tickRunning) return;
        Coroutines.Start(TickCoroutine());
    }

    private static IEnumerator TickCoroutine()
    {
        _tickRunning = true;

        while (IsActive)
        {
            Tick();
            yield return null;
        }

        StopHeartbeat();
        _tickRunning = false;
    }

    private static void Tick()
    {
        var inMeeting = MeetingHud.Instance || ExileController.Instance;

        if (!inMeeting)
        {
            if (!IsArmed)
            {
                ArmingRemaining -= Time.deltaTime;
                if (ArmingRemaining <= 0f)
                {
                    IsArmed = true;
                    ArmingRemaining = 0f;
                    // Refresh now so the Pass button appears the same frame the
                    // heartbeat and HUD turn on. This is the synchronization
                    // step the user explicitly asked for.
                    RefreshFragButtons();
                }
            }
            else if (TimeRemaining > 0f)
            {
                TimeRemaining -= Time.deltaTime;
            }
        }

        if (IsArmed && TimeRemaining <= 0f && !_explosionTriggered && !inMeeting)
        {
            TriggerExplosion();
        }

        UpdateHeartbeat();
        SyncFragTimerRow();
    }

    private static void TriggerExplosion()
    {
        var localPlayer = PlayerControl.LocalPlayer;
        var frag = PlayerById(FragId);
        var holder = PlayerById(HolderId);

        if (localPlayer == null || localPlayer.PlayerId != FragId || frag == null || holder == null ||
            holder.Data == null || holder.Data.IsDead)
        {
            Clear(startGiveCooldown: true);
            return;
        }

        if (MeetingHud.Instance || ExileController.Instance)
        {
            Clear(startGiveCooldown: false);
            return;
        }

        // RpcSpecialMurder can skip the normal BeforeMurder pipeline. Run the
        // same cancelable event TownOfUs uses for melee kills so every shield
        // (BaseShieldModifier subclasses like Medic + Warden fortify, Cleric
        // barrier, first-dead shield, GA, etc.) applies its RPC, flash, and
        // cooldown reset without maintaining a fragile per-modifier list here.
        if (TryBlockExplosionIfBeforeMurderCancelled(frag, holder))
        {
            return;
        }

        _explosionTriggered = true;

        // RpcSpecialMurder routes through TouMira's CustomMurder so the kill
        // counts toward Frag's impostor stats and the death handler shows
        // "Bombed By <FragName>".
        frag.RpcSpecialMurder(
            holder,
            isIndirect: true,
            teleportMurderer: false,
            causeOfDeath: "BomberBomb");

        // OnAfterMurder will Clear() once the kill resolves on every client. If
        // the holder is already dead on this frame, don't leave UI stuck.
        if (PlayerById(HolderId)?.Data?.IsDead == true)
        {
            Clear(startGiveCooldown: true);
        }
    }

    private static bool TryBlockExplosionIfBeforeMurderCancelled(PlayerControl frag, PlayerControl holder)
    {
        if (holder.PlayerId == frag.PlayerId) return false;

        _beforeMurderShieldProbeDepth++;
        try
        {
            var evt = new BeforeMurderEvent(frag, holder);
            MiraEventManager.InvokeEvent(evt);
            if (!evt.IsCancelled) return false;

            Clear(startGiveCooldown: true);
            return true;
        }
        finally
        {
            _beforeMurderShieldProbeDepth--;
        }
    }

    // --------------------------------------------------------------------
    // Heartbeat
    // --------------------------------------------------------------------

    private static void UpdateHeartbeat()
    {
        var localPlayer = PlayerControl.LocalPlayer;
        var shouldPlay = IsActive && IsArmed && localPlayer != null &&
                        localPlayer.PlayerId == HolderId &&
                        localPlayer.Data != null && !localPlayer.Data.IsDead;

        if (!shouldPlay)
        {
            StopHeartbeat();
            return;
        }

        if (_heartbeatOwnerId == HolderId && _heartbeatSource != null && _heartbeatSource.isPlaying)
        {
            return;
        }

        StopHeartbeat();
        PlayHeartbeat();
        _heartbeatOwnerId = HolderId;
    }

    private static void PlayHeartbeat()
    {
        if (_heartbeatUnavailable || !SoundManager.Instance) return;
        if (!IsArmed) return;

        try
        {
            var clip = DivaniAssets.FragHeartbeat.LoadAsset();
            if (clip == null)
            {
                _heartbeatUnavailable = true;
                return;
            }

            // Keep the known-working PlaySound path, then force the returned
            // Unity source to loop. Some builds don't reliably start playback
            // when the loop flag is passed into SoundManager directly.
            _heartbeatSource = SoundManager.Instance.PlaySound(clip, false, 0.75f);
            if (_heartbeatSource != null)
            {
                _heartbeatSource.loop = true;
                if (!_heartbeatSource.isPlaying)
                {
                    _heartbeatSource.Play();
                }
            }
        }
        catch (System.Exception ex)
        {
            _heartbeatUnavailable = true;
            DivaniPlugin.Instance.Log.LogWarning($"Frag: heartbeat sfx unavailable: {ex.Message}");
        }
    }

    private static void StopHeartbeat()
    {
        if (_heartbeatSource != null)
        {
            _heartbeatSource.Stop();
            _heartbeatSource = null;
        }

        _heartbeatOwnerId = NoPlayer;
    }

    public static void PlayGivePassSoundLocal()
    {
        if (!SoundManager.Instance) return;

        try
        {
            var clip = DivaniAssets.FragGiveSound.LoadAsset();
            if (clip != null)
            {
                SoundManager.Instance.PlaySound(clip, false, 0.75f);
            }
        }
        catch (System.Exception ex)
        {
            DivaniPlugin.Instance.Log.LogWarning($"Frag: give/pass sfx unavailable: {ex.Message}");
        }
    }

    // --------------------------------------------------------------------
    // HUD: shared <see cref="DivaniTimers"/> row (same prefab as Thief / Sentinel
    // toasts), stacked under lockdown when both are active.
    // --------------------------------------------------------------------

    private static Sprite? GetFragRoleIcon()
    {
        if (_cachedFragRoleIcon != null) return _cachedFragRoleIcon;
        try
        {
            _cachedFragRoleIcon = DivaniAssets.FragIcon.LoadAsset();
        }
        catch
        {
            // non-fatal
        }

        return _cachedFragRoleIcon;
    }

    private static void SyncFragTimerRow()
    {
        var local = PlayerControl.LocalPlayer;
        var inMeeting = MeetingHud.Instance || ExileController.Instance;
        if (!IsActive || !IsArmed || inMeeting || local == null || local.Data == null || local.Data.IsDead ||
            !IsHolder(local.PlayerId))
        {
            DivaniTimers.Remove(TimerId);
            return;
        }

        DivaniTimers.Set(
            TimerId,
            "<b><color=#e8a87c>PASS THE FRAG!</color></b>",
            GetFragRoleIcon(),
            Mathf.Max(0f, TimeRemaining),
            useLocalTimeDelta: false,
            priority: TimerPriority);
    }

    // --------------------------------------------------------------------
    // Helpers / events
    // --------------------------------------------------------------------

    private static PlayerControl? PlayerById(byte id)
    {
        if (id == NoPlayer) return null;

        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player != null && player.PlayerId == id)
            {
                return player;
            }
        }

        return null;
    }

    private static void RefreshFragButtons()
    {
        var localPlayer = PlayerControl.LocalPlayer;
        if (localPlayer?.Data == null) return;

        var role = localPlayer.Data.Role;

        var passButton = FragBombButton.Instance;
        if (passButton?.Button != null)
        {
            passButton.Button.ToggleVisible(passButton.Enabled(role));
        }

        var giveButton = FragGiveBombButton.Instance;
        if (giveButton?.Button != null)
        {
            giveButton.Button.ToggleVisible(giveButton.Enabled(role));
        }
    }

    [RegisterEvent]
    public static void OnAfterMurder(AfterMurderEvent evt)
    {
        if (!IsActive || evt.Target == null) return;
        if (evt.Target.PlayerId != HolderId) return;

        // If the bomb wasn't the cause, the holder was murdered by something
        // else mid-bomb. Spec: in that case both Frag cooldowns restart.
        var killedExternally = !_explosionTriggered;

        Clear(startGiveCooldown: true);

        if (killedExternally)
        {
            var local = PlayerControl.LocalPlayer;
            if (local != null && local.Data?.Role is FragRole && local.Data.Role.CanUseKillButton)
            {
                local.SetKillTimer(GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown);
            }
        }
    }

    [RegisterEvent(-500)]
    public static void OnBeforeMurder(BeforeMurderEvent evt)
    {
        if (_beforeMurderShieldProbeDepth > 0) return;
        if (!IsActive) return;
        if (evt.Source == null || evt.Source.Data?.Role is not FragRole) return;

        if (_explosionTriggered && evt.Target != null && evt.Target.PlayerId == HolderId)
        {
            return;
        }

        evt.Cancel();
    }

    [RegisterEvent]
    public static void OnRoundStart(RoundStartEvent _)
    {
        FragGiveBombButton.StartCooldown();

        var local = PlayerControl.LocalPlayer;
        if (local != null && local.Data?.Role is FragRole && local.Data.Role.CanUseKillButton)
        {
            local.SetKillTimer(GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown);
        }
    }
}

[HarmonyPatch]
public static class FragBombPatches
{
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    [HarmonyPostfix]
    public static void HudManagerUpdatePostfix(HudManager __instance)
    {
        FragBombButton.UpdateVisuals();
        FragGiveBombButton.UpdateVisuals();

        if (FragBombState.LocalFragShouldBlockKill() && __instance.KillButton != null)
        {
            __instance.KillButton.SetDisabled();
        }
    }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
    [HarmonyPostfix]
    public static void MeetingStartPostfix()
    {
        FragBombState.ClearForMeeting();
    }

    // We deliberately do NOT patch Console.CanUse here. Several normal tasks
    // (trash chute, download upload, divert power, etc.) ship with an empty
    // TaskTypes array, so a "TaskTypes empty == emergency button" check would
    // also block those tasks. The two minigame prefixes below are enough to
    // stop the bomb holder from actually opening the emergency-meeting UI,
    // and they leave every task console fully usable.
    [HarmonyPatch(typeof(EmergencyMinigame), nameof(EmergencyMinigame.Begin))]
    [HarmonyPrefix]
    public static bool EmergencyMinigameBeginPrefix(EmergencyMinigame __instance)
    {
        var localPlayer = PlayerControl.LocalPlayer;
        if (localPlayer == null || !FragBombState.IsHolder(localPlayer.PlayerId)) return true;

        __instance.Close();
        return false;
    }

    [HarmonyPatch(typeof(Minigame), nameof(Minigame.Begin))]
    [HarmonyPrefix]
    public static bool MinigameBeginPrefix(Minigame __instance, PlayerTask task)
    {
        // Normal task minigames always arrive with a task. Emergency button
        // opens a non-task minigame, so this keeps the safety net narrow and
        // matches the existing LockdownPatch signature for this Harmony target.
        if (task != null) return true;

        var localPlayer = PlayerControl.LocalPlayer;
        if (localPlayer == null || !FragBombState.IsHolder(localPlayer.PlayerId)) return true;
        if (!__instance.GetType().Name.Contains("Emergency")) return true;

        __instance.Close();
        return false;
    }

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
    [HarmonyPostfix]
    public static void OnGameEndPostfix()
    {
        FragBombState.Clear();
        FragBombState.DestroyHud();
    }

    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.CoBegin))]
    [HarmonyPostfix]
    public static void OnIntroBeginPostfix()
    {
        FragBombState.Clear();
        FragBombState.DestroyHud();
        FragGiveBombButton.StartCooldown();
    }
}
