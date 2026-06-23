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
using DivaniMods.Options;
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
    public static float Duration { get; private set; }
    private static float ArmingRemaining;
    public static float ArmingDuration { get; private set; }

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
        if (!IsArmed) return ArmingRemaining;
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
            Duration = duration;
            ArmingRemaining = Mathf.Clamp(armingDelay, 2f, 7f);
            ArmingDuration = ArmingRemaining;
            IsArmed = false;
            _explosionTriggered = false;
        }
        else
        {
            IsArmed = true;
            ArmingRemaining = 0f;
        }

        HolderId = targetId;
        ImmuneId = immuneId;
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
        Duration = 0f;
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
        var holderIdCapture = HolderId;
        var fragIdCapture = FragId;
        var shouldKill = IsActive && IsArmed;

        Clear(startGiveCooldown: false);

        if (shouldKill)
        {
            Coroutines.Start(CoExplodeInMeeting(holderIdCapture, fragIdCapture));
        }
    }

    private static IEnumerator CoExplodeInMeeting(byte holderIdCapture, byte fragIdCapture)
    {
        while (MeetingHud.Instance &&
               MeetingHud.Instance.state == MeetingHud.VoteStates.Animating)
        {
            yield return null;
        }

        if (!MeetingHud.Instance) yield break;

        yield return new WaitForSeconds(0.25f);

        if (!AmongUsClient.Instance.AmHost) yield break;

        var holder = PlayerById(holderIdCapture);
        if (holder == null || holder.Data == null || holder.Data.IsDead) yield break;

        var frag = PlayerById(fragIdCapture);
        var source = (frag != null && frag.Data != null && !frag.Data.IsDead) ? frag : holder;

        if (source.AmOwner)
        {
            MeetingMenu.Instances.Do(x => x.HideSingle(holder.PlayerId));
        }

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
            causeOfDeath: "Frag");
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
        var rewinding = TownOfUs.Modules.TimeLordRewindSystem.IsRewinding;

        if (!inMeeting && rewinding &&
            OptionGroupSingleton<FragOptions>.Instance.OnTimelordRewind.Value == (int)FragRewindBehavior.Rewind)
        {
            if (!IsArmed)
            {
                ArmingRemaining = Mathf.Min(ArmingDuration, ArmingRemaining + Time.deltaTime);
            }
            else
            {
                TimeRemaining = Mathf.Min(Duration, TimeRemaining + Time.deltaTime);
            }
        }
        else if (!inMeeting && !rewinding)
        {
            if (!IsArmed)
            {
                ArmingRemaining -= Time.deltaTime;
                if (ArmingRemaining <= 0f)
                {
                    IsArmed = true;
                    ArmingRemaining = 0f;
                    RefreshFragButtons();
                }
            }
            else if (TimeRemaining > 0f)
            {
                TimeRemaining -= Time.deltaTime;
            }
        }

        if (IsArmed && TimeRemaining <= 0f && !_explosionTriggered && !inMeeting && !rewinding)
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
        if (TryBlockExplosionIfBeforeMurderCancelled(frag, holder))
        {
            return;
        }

        _explosionTriggered = true;
        frag.RpcSpecialMurder(
            holder,
            isIndirect: true,
            teleportMurderer: false,
            causeOfDeath: "Frag");

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

    internal static void StopHeartbeat()
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
        FragBombState.StopHeartbeat();
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
