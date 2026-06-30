using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using Reactor.Utilities;
using DivaniMods.Assets;
using DivaniMods.Buttons.Neutral.NeutralKilling;
using DivaniMods.Modifiers;
using DivaniMods.Modifiers.Neutral.NeutralOutlier;
using DivaniMods.Networking.Crewmate.CrewmateKilling;
using DivaniMods.Networking.Neutral.NeutralKilling;
using DivaniMods.Options;
using DivaniMods.Roles.Crewmate.CrewmateAfterlife;
using DivaniMods.Roles.Neutral.NeutralKilling;
using DivaniMods.Utilities;
using TownOfUs.Extensions;
using TownOfUs.Modules;
using TownOfUs.Networking;
using TownOfUs.Roles;
using TownOfUs.Utilities;
using UnityEngine;

namespace DivaniMods.Modules.Watcher;

public static class WatcherLightSystem
{
    private enum Phase { Idle, Green, Red, EndGreen }

    private const string TimerId = "watcher_light";
    private const float FlashHold = 0.25f;
    private const float FlashAlpha = 0.4f;
    private const float MoveThreshold = 0.08f;
    private const float EndGreenDuration = 1f;
    private const float InstantKillDelay = 0.1f;
    private const float SoundVolume = 1f;

    private static bool _active;
    private static Phase _phase = Phase.Idle;
    private static byte _watcherId;

    private static float _phaseEndTime;
    private static float _redStartTime;
    private static float _redDuration;
    private static float _greenDuration;
    private static float _grace;
    private static int _loopsTotal;
    private static int _redLightsDone;

    private static bool _refCaptured;
    private static Vector2 _redRefPos;
    private static bool _localReported;
    private static bool _batchGunshotPlayed;
    private static bool _localInvisNotified;

    private static readonly HashSet<byte> Movers = new();
    private static readonly HashSet<byte> ManualKills = new();

    public static bool IsActive => _active;

    public static void RegisterManualKill(byte victimId)
    {
        ManualKills.Add(victimId);
    }

    public static bool ConsumeManualKill(byte victimId)
    {
        return ManualKills.Remove(victimId);
    }

    public static bool IsRedLightActive => _active && _phase == Phase.Red;

    public static bool IsGreenLightActive => _active && (_phase == Phase.Green || _phase == Phase.EndGreen);

    private static WatcherOptions Options => OptionGroupSingleton<WatcherOptions>.Instance;

    public static bool BlocksSabotage(PlayerControl player)
    {
        return Options.BlockSabotage.Value && IsActive && IsAffected(player);
    }

    public static bool BlocksKill(PlayerControl player)
    {
        return IsRedLightActive && IsAffected(player);
    }

    public static void Start(byte watcherId, float greenDuration, float redDuration, float grace, int loops)
    {
        Stop();

        _watcherId = watcherId;
        _greenDuration = greenDuration;
        _redDuration = redDuration;
        _grace = grace;
        _loopsTotal = Mathf.Max(1, loops);
        _redLightsDone = 0;
        _batchGunshotPlayed = false;
        _localInvisNotified = false;
        Movers.Clear();
        ManualKills.Clear();

        _active = true;
        EnterGreen(greenDuration);
        NotifyLocalExempt();
        CloseOpenEmergency();
    }

    private static void CloseOpenEmergency()
    {
        if (!Options.DisableEmergencyButton.Value)
        {
            return;
        }

        var emergency = Minigame.Instance == null ? null : Minigame.Instance.TryCast<EmergencyMinigame>();
        emergency?.Close();
    }

    private static void CloseOpenPlayerMenu()
    {
        var menu = Minigame.Instance == null ? null : Minigame.Instance.TryCast<CustomPlayerMenu>();
        menu?.ForceClose();
    }

    public static void Stop()
    {
        DivaniTimers.Remove(TimerId);
        RemoveLocalWatched();
        ClearFlash();

        _active = false;
        _phase = Phase.Idle;
        _refCaptured = false;
        _localReported = false;
        _localInvisNotified = false;
        Movers.Clear();
        ManualKills.Clear();
    }

    public static void RegisterMover(byte playerId)
    {
        if (!_active)
        {
            return;
        }

        if (Options.InstantKillOnMovement.Value)
        {
            Coroutines.Start(CoInstantKill(playerId));
        }
        else
        {
            Movers.Add(playerId);
        }
    }

    private static IEnumerator CoInstantKill(byte moverId)
    {
        yield return new WaitForSeconds(InstantKillDelay);

        if (MeetingHud.Instance || ExileController.Instance)
        {
            yield break;
        }

        var watcher = PlayerById(_watcherId);
        var victim = PlayerById(moverId);
        if (watcher == null || watcher.HasDied() || victim == null || victim.PlayerId == _watcherId || !IsAffected(victim))
        {
            yield break;
        }

        if (!AmongUsClient.Instance.AmHost)
        {
            yield break;
        }

        if (victim.HasDied())
        {
            WatcherRpc.RpcNeutralizeGhost(watcher, victim.PlayerId);
        }
        else
        {
            watcher.RpcSpecialMurder(
                victim,
                isIndirect: true,
                ignoreShield: false,
                teleportMurderer: false,
                showKillAnim: false,
                causeOfDeath: "Watcher");
        }
    }

    private static void EnterGreen(float duration)
    {
        _phase = Phase.Green;
        _phaseEndTime = Time.time + duration;
        Flash(WatcherRole.GreenLightColor, FlashHold);
        PlaySound(DivaniAssets.WatcherGoSound.LoadAsset());
        UpdateGreenTimer();
    }

    private static void EnterRed()
    {
        _phase = Phase.Red;
        _redStartTime = Time.time;
        _phaseEndTime = Time.time + _redDuration;
        _refCaptured = false;
        _localReported = false;
        Flash(WatcherRole.RedLightColor, _redDuration + 0.25f);
        PlaySound(DivaniAssets.WatcherStopSound.LoadAsset());
        AddLocalWatched();
        CloseOpenPlayerMenu();
    }

    private static void FinishRedLight()
    {
        DoHostKill();
        RemoveLocalWatched();
        Movers.Clear();
        _batchGunshotPlayed = false;
    }

    private static void EnterLoopGreen()
    {
        FinishRedLight();
        EnterGreen(_greenDuration);
    }

    private static void EnterEndGreen()
    {
        FinishRedLight();

        _phase = Phase.EndGreen;
        _phaseEndTime = Time.time + EndGreenDuration;
        Flash(WatcherRole.GreenLightColor, FlashHold);
        PlaySound(DivaniAssets.WatcherGoSound.LoadAsset());
        UpdateGreenTimer();
    }

    private static void Tick()
    {
        try
        {
            if (!_active)
            {
                return;
            }

            if (LobbyBehaviour.Instance || MeetingHud.Instance || ExileController.Instance)
            {
                Stop();
                return;
            }

            CheckInvisNotification();

            var watcher = PlayerById(_watcherId);
            if (watcher == null || watcher.HasDied())
            {
                Stop();
                return;
            }

            var now = Time.time;
            switch (_phase)
            {
                case Phase.Green:
                    UpdateGreenTimer();
                    if (now >= _phaseEndTime)
                    {
                        EnterRed();
                    }
                    break;

                case Phase.Red:
                    UpdateRedTimer(_phaseEndTime - now);
                    LockButtonsDuringRed();

                    if (!_refCaptured && now >= _redStartTime + _grace)
                    {
                        var me = PlayerControl.LocalPlayer;
                        if (me != null)
                        {
                            _redRefPos = me.GetTruePosition();
                        }
                        _refCaptured = true;
                    }

                    if (_refCaptured)
                    {
                        DetectMovement();
                    }

                    if (now >= _phaseEndTime)
                    {
                        _redLightsDone++;
                        if (_redLightsDone >= _loopsTotal)
                        {
                            EnterEndGreen();
                        }
                        else
                        {
                            EnterLoopGreen();
                        }
                    }
                    break;

                case Phase.EndGreen:
                    UpdateGreenTimer();
                    if (now >= _phaseEndTime)
                    {
                        ArmWatchCooldown();
                        Stop();
                    }
                    break;
            }
        }
        catch (Exception ex)
        {
            DivaniPlugin.Instance?.Log.LogWarning($"WatcherLightSystem.Tick: {ex.Message}");
            Stop();
        }
    }

    private static void DetectMovement()
    {
        if (_localReported)
        {
            return;
        }

        var me = PlayerControl.LocalPlayer;
        if (me == null)
        {
            return;
        }

        if (TimeLordRewindSystem.IsRewinding)
        {
            _redRefPos = me.GetTruePosition();
            return;
        }

        if (!IsAffected(me))
        {
            _redRefPos = me.GetTruePosition();
            return;
        }

        if (Vector2.Distance(me.GetTruePosition(), _redRefPos) <= MoveThreshold)
        {
            return;
        }

        _localReported = true;

        WatcherRpc.RpcReportMover(me, me.PlayerId);
    }

    public static void OnConfirmedRedLightKill(PlayerControl victim)
    {
        if (victim == null || !victim.HasDied())
        {
            return;
        }

        PlayKillFeedback(victim);
    }

    public static void NeutralizeGhostLocal(byte ghostId)
    {
        var ghost = PlayerById(ghostId);
        if (ghost == null)
        {
            return;
        }

        if (ghost.Data?.Role is VengefulSoulRole)
        {
            if (AmongUsClient.Instance.AmHost)
            {
                RetributionistRpc.RpcRevengeFailed(ghost);
            }
        }
        else if (ghost.Data?.Role is IGhostRole ghostRole)
        {
            ghostRole.Caught = true;
        }

        PlayKillFeedback(ghost);
    }

    private static void PlayKillFeedback(PlayerControl victim)
    {
        if (Options.InstantKillOnMovement.Value)
        {
            PlayGunshot();
        }
        else if (!_batchGunshotPlayed)
        {
            _batchGunshotPlayed = true;
            PlayGunshot();
        }

        if (victim.AmOwner)
        {
            MiraAPI.Utilities.Helpers.CreateAndShowNotification(
                $"<b>{WatcherRole.RedLightColor.ToTextColor()}Movement Detected!</color></b>",
                Color.white,
                new Vector3(0f, 1f, -20f),
                spr: DivaniAssets.WatcherRedLight.LoadAsset());
        }
    }

    private static void DoHostKill()
    {
        if (!AmongUsClient.Instance.AmHost)
        {
            return;
        }

        var watcher = PlayerById(_watcherId);
        if (watcher == null || watcher.HasDied())
        {
            return;
        }

        var targets = new List<PlayerControl>();
        foreach (var id in Movers)
        {
            var p = PlayerById(id);
            if (p == null || p.PlayerId == _watcherId || !IsAffected(p))
            {
                continue;
            }

            if (p.HasDied())
            {
                WatcherRpc.RpcNeutralizeGhost(watcher, p.PlayerId);
            }
            else
            {
                targets.Add(p);
            }
        }

        if (targets.Count > 0)
        {
            watcher.RpcSpecialMultiMurder(
                targets,
                isIndirect: true,
                ignoreShields: false,
                teleportMurderer: false,
                showKillAnim: false,
                causeOfDeath: "Watcher");
        }
    }

    private static bool IsAffected(PlayerControl player)
    {
        if (player == null || player.IsRole<WatcherRole>())
        {
            return false;
        }

        if (player.HasModifier<DuelModifier>())
        {
            return false;
        }

        if (IsInvisible(player))
        {
            return false;
        }

        if (!player.HasDied())
        {
            return true;
        }

        return Options.GhostwalkersMustFreeze.Value && player.Data?.Role is IGhostRole;
    }

    private const string SwoopModifierName = "Swooped";

    private static bool IsInvisible(PlayerControl player)
    {
        foreach (var mod in player.GetModifiers<BaseModifier>())
        {
            if (mod != null && mod.ModifierName == SwoopModifierName)
            {
                return true;
            }
        }

        return false;
    }

    private static void CheckInvisNotification()
    {
        var me = PlayerControl.LocalPlayer;
        if (me == null || me.HasDied() || me.IsRole<WatcherRole>() || !IsInvisible(me))
        {
            _localInvisNotified = false;
            return;
        }

        if (_localInvisNotified)
        {
            return;
        }

        _localInvisNotified = true;
        ShowExemptNotification("While invisible, you won't be affected.");
    }

    private static void NotifyLocalExempt()
    {
        var me = PlayerControl.LocalPlayer;
        if (me == null || me.IsRole<WatcherRole>())
        {
            return;
        }

        if (!me.HasDied() && me.HasModifier<DuelModifier>())
        {
            ShowExemptNotification("You are in a Duel and won't be affected.");
        }
        else if (me.HasDied() && !Options.GhostwalkersMustFreeze.Value && me.Data?.Role is IGhostRole)
        {
            ShowExemptNotification("Ghost Roles like you are not affected.");
        }
    }

    private static void ShowExemptNotification(string body)
    {
        MiraAPI.Utilities.Helpers.CreateAndShowNotification(
            $"<b>{WatcherRole.WatcherColor.ToTextColor()}Red Light, Green Light</color></b>\n" + body,
            Color.white,
            new Vector3(0f, 1f, -20f),
            spr: DivaniAssets.WatcherIcon.LoadAsset());
    }

    private static void LockButtonsDuringRed()
    {
        var me = PlayerControl.LocalPlayer;
        if (me == null || !me.IsRole<WatcherRole>())
        {
            return;
        }

        foreach (var button in CustomButtonManager.Buttons)
        {
            if (button is WatcherKillButton or WatcherWatchButton)
            {
                continue;
            }

            button.Button?.SetDisabled();
        }
    }

    private static void AddLocalWatched()
    {
        var me = PlayerControl.LocalPlayer;
        if (me != null && IsAffected(me) && !me.HasModifier<WatcherWatchedModifier>())
        {
            me.AddModifier<WatcherWatchedModifier>();
        }
    }

    private static void RemoveLocalWatched()
    {
        var me = PlayerControl.LocalPlayer;
        if (me != null && me.HasModifier<WatcherWatchedModifier>())
        {
            me.RemoveModifier<WatcherWatchedModifier>();
        }
    }

    private static void ArmWatchCooldown()
    {
        var me = PlayerControl.LocalPlayer;
        if (me == null || !me.IsRole<WatcherRole>())
        {
            return;
        }

        var button = CustomButtonSingleton<WatcherWatchButton>.Instance;
        button?.SetTimer(button.Cooldown);

        if (Options.LinkWatchKillCooldown.Value)
        {
            var kill = CustomButtonSingleton<WatcherKillButton>.Instance;
            if (kill != null)
            {
                kill.SetTimer(kill.Cooldown);
                me.SetKillTimer(kill.Cooldown);
            }
        }
    }

    private static void UpdateGreenTimer()
    {
        DivaniTimers.Set(
            TimerId,
            $"{WatcherRole.GreenLightColor.ToTextColor()}<b>GREEN LIGHT</b></color>",
            DivaniAssets.WatcherGreenLight.LoadAsset());
    }

    private static void UpdateRedTimer(float remaining)
    {
        var secs = Mathf.Max(0, Mathf.CeilToInt(remaining));
        DivaniTimers.Set(
            TimerId,
            $"{WatcherRole.RedLightColor.ToTextColor()}<b>RED LIGHT</b></color>  <color=#FFFFFF>{secs}s</color>",
            DivaniAssets.WatcherRedLight.LoadAsset());
    }

    private static void Flash(Color color, float hold)
    {
        Coroutines.Start(MiscUtils.CoFlash(color, hold, FlashAlpha));
    }

    private static void ClearFlash()
    {
        var rend = MiscUtils.FlashRenderer;
        if (rend != null)
        {
            rend.enabled = false;
            rend.gameObject.SetActive(false);
        }
    }

    private static void PlaySound(AudioClip? clip)
    {
        if (clip == null || SoundManager.Instance == null)
        {
            return;
        }

        SoundManager.Instance.PlaySound(clip, false, SoundVolume);
    }

    private static void PlayGunshot()
    {
        if (Options.GunshotSoundOnDeath.Value)
        {
            PlaySound(DivaniAssets.WatcherShootSound.LoadAsset());
        }
    }

    private static PlayerControl? PlayerById(byte id)
    {
        foreach (var p in PlayerControl.AllPlayerControls)
        {
            if (p != null && p.PlayerId == id)
            {
                return p;
            }
        }

        return null;
    }

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    public static class WatcherLightHudUpdate
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Tick();
        }
    }

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
    public static class WatcherLightGameEnd
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Stop();
        }
    }

    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.CoBegin))]
    public static class WatcherLightIntro
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Stop();
        }
    }
}
