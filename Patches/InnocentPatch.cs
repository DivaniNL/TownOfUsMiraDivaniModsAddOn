using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using DivaniMods.Modifiers.Neutral.NeutralEvil;
using DivaniMods.Options;
using DivaniMods.Roles.Neutral.NeutralEvil;
using UnityEngine;

namespace DivaniMods.Patches;

[HarmonyPatch]
public static class InnocentPatch
{

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CoStartGame))]
    [HarmonyPostfix]
    public static void ResetOnGameStart()
    {
        StripAllInnocentTargetMarkers();
        InnocentRole.ClearAndReload();
    }

    [RegisterEvent(1000)]
    public static void OnBeforeMurderLate(BeforeMurderEvent evt)
    {
        if (!evt.IsCancelled || evt.Target == null || evt.Source == null)
        {
            return;
        }

        if (InnocentRole.ActiveInnocents.TryGetValue(evt.Target.PlayerId, out var innocent) &&
            innocent.PendingTauntKillerId == evt.Source.PlayerId)
        {
            innocent.PendingTauntKillerId = null;
        }
    }

    [RegisterEvent]
    public static void OnAfterMurder(AfterMurderEvent evt)
    {
        if (evt.Target == null || evt.Source == null ||
            !InnocentRole.ActiveInnocents.TryGetValue(evt.Target.PlayerId, out var innocent))
        {
            return;
        }

        if (innocent.PendingTauntKillerId != evt.Source.PlayerId)
        {
            return;
        }

        innocent.PendingTauntKillerId = null;
        innocent.TauntedKillerId = evt.Source.PlayerId;
        innocent.TargetVoted = false;
        innocent.AboutToWin = false;
        innocent.AwaitingNextMeetingExile = true;
        innocent.WinWindowExpired = false;

        GiveKillerTauntMarker(evt.Source, evt.Target.PlayerId);
    }

    [RegisterEvent]
    public static void OnEjection(EjectionEvent evt)
    {
        var exiled = evt.ExileController?.initData?.networkedPlayer?.Object;
        if (exiled == null)
        {
            return;
        }

        foreach (var innocent in GetInnocents())
        {
            if (!innocent.AwaitingNextMeetingExile || innocent.TauntedKillerId != exiled.PlayerId)
            {
                continue;
            }

            // Require the runtime marker so ejection matches the same state as meeting UI.
            if (!InnocentTauntMeetingDisplay.KillerHasTauntMarkerForInnocent(exiled, innocent.Player.PlayerId))
            {
                continue;
            }

            innocent.AboutToWin = true;
            innocent.AwaitingNextMeetingExile = false;
            innocent.TargetVoted = true;
            RemoveInnocentTauntMarker(exiled.PlayerId, innocent.Player.PlayerId);
        }
    }

    [RegisterEvent]
    public static void OnPlayerDeath(PlayerDeathEvent evt)
    {
        if (evt.DeathReason != DeathReason.Exile)
        {
            return;
        }

        foreach (var innocent in GetInnocents())
        {
            if (innocent.TauntedKillerId == evt.Player.PlayerId && innocent.AboutToWin && !innocent.WinWindowExpired)
            {
                innocent.TargetVoted = true;
            }
        }
    }

    [RegisterEvent]
    public static void OnRoundStart(RoundStartEvent evt)
    {
        if (evt.TriggeredByIntro)
        {
            return;
        }

        foreach (var innocent in GetInnocents())
        {
            if (innocent.AboutToWin && innocent.TauntedKillerId.HasValue)
            {
                innocent.TargetVoted = true;
                RemoveInnocentTauntMarker(innocent.TauntedKillerId.Value, innocent.Player.PlayerId);
            }
            else if (innocent.AwaitingNextMeetingExile)
            {
                if (innocent.TauntedKillerId.HasValue)
                {
                    RemoveInnocentTauntMarker(innocent.TauntedKillerId.Value, innocent.Player.PlayerId);
                }

                innocent.AwaitingNextMeetingExile = false;
                innocent.WinWindowExpired = true;
                innocent.TauntedKillerId = null;
            }
        }
    }

    [RegisterEvent]
    public static void OnReportBody(ReportBodyEvent evt)
    {
        if (evt.Target == null)
        {
            return;
        }

        if (IsReportBlockedFor(evt.Reporter, evt.Target.PlayerId))
        {
            evt.Cancel();
        }
    }

    // Only the killer the innocent taunted (TauntedKillerId) is barred from reporting that
    // innocent's body; everyone else reports normally.
    private static bool IsReportBlockedFor(PlayerControl reporter, byte bodyPlayerId)
    {
        if (reporter == null || OptionGroupSingleton<InnocentOptions>.Instance.TauntedPlayerCanReportBody)
        {
            return false;
        }

        return InnocentRole.ActiveInnocents.TryGetValue(bodyPlayerId, out var innocent) &&
               innocent.AwaitingNextMeetingExile &&
               innocent.TauntedKillerId == reporter.PlayerId;
    }

    [HarmonyPatch(typeof(ReportButton), nameof(ReportButton.DoClick))]
    [HarmonyPriority(Priority.First)]
    [HarmonyPrefix]
    public static bool BlockTauntedReportClick()
    {
        var localPlayer = PlayerControl.LocalPlayer;
        if (localPlayer == null)
        {
            return true;
        }

        var closest = GetClosestReportableBody(localPlayer);
        return closest == null || !IsReportBlockedFor(localPlayer, closest.ParentId);
    }

    public static void UpdateReportButtonGreyout()
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

        var closest = GetClosestReportableBody(localPlayer);
        if (closest != null && IsReportBlockedFor(localPlayer, closest.ParentId))
        {
            HudManager.Instance.ReportButton.SetDisabled();
        }
    }

    private static DeadBody? GetClosestReportableBody(PlayerControl reporter)
    {
        if (reporter.Data == null || reporter.Data.IsDead || reporter.inVent)
        {
            return null;
        }

        DeadBody? best = null;
        var bestDistance = float.MaxValue;
        var reporterPosition = reporter.GetTruePosition();

        foreach (var body in UnityEngine.Object.FindObjectsOfType<DeadBody>())
        {
            if (body == null || body.Reported)
            {
                continue;
            }

            var bodyPosition = body.TruePosition;
            var distance = Vector2.Distance(reporterPosition, bodyPosition);
            if (distance > reporter.MaxReportDistance || distance >= bestDistance)
            {
                continue;
            }

            if (PhysicsHelpers.AnythingBetween(
                    reporterPosition,
                    bodyPosition,
                    Constants.ShipAndObjectsMask,
                    false))
            {
                continue;
            }

            best = body;
            bestDistance = distance;
        }

        return best;
    }

    private static IEnumerable<InnocentRole> GetInnocents()
    {
        return InnocentRole.ActiveInnocents.Values;
    }

    private static void StripAllInnocentTargetMarkers()
    {
        foreach (var pc in PlayerControl.AllPlayerControls.ToArray())
        {
            if (pc == null)
            {
                continue;
            }

            var comp = pc.GetModifierComponent();
            if (comp == null)
            {
                continue;
            }

            foreach (var m in pc.GetModifiers<InnocentTargetModifier>().ToArray())
            {
                comp.RemoveModifier(m);
            }
        }
    }

    private static void GiveKillerTauntMarker(PlayerControl killer, byte innocentPlayerId)
    {
        var comp = killer.GetModifierComponent();
        if (comp == null)
        {
            return;
        }

        foreach (var existing in killer.GetModifiers<InnocentTargetModifier>().ToArray())
        {
            comp.RemoveModifier(existing);
        }

        killer.AddModifier<InnocentTargetModifier>(innocentPlayerId);
    }

    private static void RemoveInnocentTauntMarker(byte killerPlayerId, byte innocentPlayerId)
    {
        var killer = GameData.Instance?.GetPlayerById(killerPlayerId)?.Object;
        if (killer == null)
        {
            return;
        }

        var comp = killer.GetModifierComponent();
        if (comp == null)
        {
            return;
        }

        foreach (var m in killer.GetModifiers<InnocentTargetModifier>().ToArray())
        {
            if (m.InnocentPlayerId == innocentPlayerId)
            {
                comp.RemoveModifier(m);
            }
        }
    }
}

[HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
public static class InnocentReportHudPatch
{
    [HarmonyPostfix]
    public static void HudManagerUpdatePostfix()
    {
        InnocentPatch.UpdateReportButtonGreyout();
    }
}
