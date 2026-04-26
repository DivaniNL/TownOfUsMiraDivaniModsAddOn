using System.Collections;
using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using Reactor.Utilities;
using DivaniMods.Assets;
using DivaniMods.Modifiers;
using DivaniMods.Options;
using TownOfUs.Assets;
using UnityEngine;

namespace DivaniMods.Patches;

public static class BearTrapPatch
{
    private sealed record ActiveTrap(byte KillerId, byte VictimId, float EndsAt);

    private static readonly Dictionary<byte, ActiveTrap> ActiveTraps = new();
    private static bool _reportButtonHiddenByBearTrap;

    [RegisterEvent]
    public static void OnAfterMurder(AfterMurderEvent evt)
    {
        if (MeetingHud.Instance != null || evt.Source == null || evt.Target == null)
        {
            return;
        }

        if (!evt.Target.HasModifier<BearTrapModifier>())
        {
            return;
        }

        var duration = OptionGroupSingleton<BearTrapOptions>.Instance.FreezeDuration.Value;
        var trap = new ActiveTrap(evt.Source.PlayerId, evt.Target.PlayerId, Time.time + duration);
        ActiveTraps[evt.Source.PlayerId] = trap;

        DivaniPlugin.Instance.Log.LogInfo(
            $"Bear Trap: froze {evt.Source.Data?.PlayerName} for {duration:0} seconds after killing {evt.Target.Data?.PlayerName}.");

        if (evt.Source.AmOwner)
        {
            Helpers.CreateAndShowNotification(
                $"<b><color=#A5632D>A Bear Trap caught you for {duration:0} seconds</color></b>",
                Color.white,
                new Vector3(0f, 1f, -20f),
                spr: TouCrewAssets.TrapSprite.LoadAsset());
            PlayBearTrapSound();
            ExitVentIfNeeded(evt.Source);
            Coroutines.Start(CoFreezeKiller(evt.Source, trap));
        }
    }

    [RegisterEvent]
    public static void OnReportBody(ReportBodyEvent evt)
    {
        if (evt.Target == null)
        {
            return;
        }

        if (!ActiveTraps.TryGetValue(evt.Reporter.PlayerId, out var trap))
        {
            return;
        }

        if (Time.time >= trap.EndsAt)
        {
            ActiveTraps.Remove(evt.Reporter.PlayerId);
            return;
        }

        if (evt.Target.PlayerId == trap.VictimId)
        {
            DivaniPlugin.Instance.Log.LogInfo(
                $"Bear Trap: blocked {evt.Reporter.Data?.PlayerName} from self-reporting their trapped victim.");
            evt.Cancel();
        }
    }

    [RegisterEvent]
    public static void OnMeetingStart(StartMeetingEvent _)
    {
        ActiveTraps.Clear();
        _reportButtonHiddenByBearTrap = false;
    }

    public static void UpdateReportButtonVisibility()
    {
        if (!HudManager.InstanceExists || HudManager.Instance.ReportButton == null)
        {
            return;
        }

        var localPlayer = PlayerControl.LocalPlayer;
        if (localPlayer == null)
        {
            return;
        }

        if (ActiveTraps.TryGetValue(localPlayer.PlayerId, out var trap) &&
            Time.time < trap.EndsAt &&
            IsTrapVictimReportableNearby(localPlayer, trap.VictimId))
        {
            HudManager.Instance.ReportButton.ToggleVisible(false);
            _reportButtonHiddenByBearTrap = true;
            return;
        }

        if (_reportButtonHiddenByBearTrap)
        {
            HudManager.Instance.ReportButton.ToggleVisible(HasReportableBodyNearby(localPlayer));
            _reportButtonHiddenByBearTrap = false;
        }
    }

    private static bool IsTrapVictimReportableNearby(PlayerControl reporter, byte victimId)
    {
        foreach (var body in UnityEngine.Object.FindObjectsOfType<DeadBody>())
        {
            if (body == null || body.ParentId != victimId)
            {
                continue;
            }

            return IsBodyReportableBy(reporter, body);
        }

        return false;
    }

    private static bool HasReportableBodyNearby(PlayerControl reporter)
    {
        foreach (var body in UnityEngine.Object.FindObjectsOfType<DeadBody>())
        {
            if (body != null && IsBodyReportableBy(reporter, body))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsBodyReportableBy(PlayerControl reporter, DeadBody body)
    {
        if (reporter.Data == null || reporter.Data.IsDead || reporter.inVent || body.Reported)
        {
            return false;
        }

        var reporterPosition = reporter.GetTruePosition();
        var bodyPosition = body.TruePosition;
        if (Vector2.Distance(reporterPosition, bodyPosition) > reporter.MaxReportDistance)
        {
            return false;
        }

        return !PhysicsHelpers.AnythingBetween(
            reporterPosition,
            bodyPosition,
            Constants.ShipAndObjectsMask,
            false);
    }

    private static void PlayBearTrapSound()
    {
        if (SoundManager.Instance == null)
        {
            return;
        }

        try
        {
            var clip = DivaniAssets.BearTrapActivateSound.LoadAsset();
            if (clip != null)
            {
                SoundManager.Instance.PlaySound(clip, false, 1f);
            }
        }
        catch (System.Exception ex)
        {
            DivaniPlugin.Instance.Log.LogWarning($"Bear Trap: activation sfx failed: {ex.Message}");
        }
    }

    private static void ExitVentIfNeeded(PlayerControl killer)
    {
        if (!killer.inVent)
        {
            return;
        }

        if (Vent.currentVent != null)
        {
            killer.MyPhysics.RpcExitVent(Vent.currentVent.Id);
        }

        killer.MyPhysics.ExitAllVents();
    }

    private static IEnumerator CoFreezeKiller(PlayerControl killer, ActiveTrap trap)
    {
        if (killer == null)
        {
            yield break;
        }

        var killerId = killer.PlayerId;
        var originalMoveable = killer.moveable;
        var lockedPosition = killer.transform.position;

        while (killer != null &&
               ActiveTraps.TryGetValue(killerId, out var currentTrap) &&
               currentTrap == trap &&
               Time.time < trap.EndsAt &&
               MeetingHud.Instance == null)
        {
            killer.moveable = false;
            killer.transform.position = lockedPosition;
            killer.MyPhysics?.ResetMoveState();

            if (killer.MyPhysics?.body != null)
            {
                killer.MyPhysics.body.velocity = Vector2.zero;
            }

            UpdateReportButtonVisibility();

            yield return null;
        }

        if (ActiveTraps.TryGetValue(killerId, out var finalTrap) && finalTrap == trap)
        {
            ActiveTraps.Remove(killerId);
        }

        if (killer != null && MeetingHud.Instance == null)
        {
            killer.moveable = originalMoveable;
            UpdateReportButtonVisibility();
        }
    }
}

[HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
public static class BearTrapHudPatch
{
    [HarmonyPostfix]
    public static void HudManagerUpdatePostfix()
    {
        BearTrapPatch.UpdateReportButtonVisibility();
    }
}
