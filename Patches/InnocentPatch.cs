using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Events.Vanilla.Player;
using HarmonyLib;
using DivaniMods.Roles;

namespace DivaniMods.Patches;

[HarmonyPatch]
public static class InnocentPatch
{
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CoStartGame))]
    [HarmonyPostfix]
    public static void ResetOnGameStart()
    {
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
        innocent.ShowTauntedTargetSymbol = true;
        innocent.TargetVoted = false;
        innocent.AboutToWin = false;
        innocent.AwaitingNextMeetingExile = true;
        innocent.WinWindowExpired = false;
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
            if (innocent.AwaitingNextMeetingExile && innocent.TauntedKillerId == exiled.PlayerId)
            {
                innocent.AboutToWin = true;
                innocent.AwaitingNextMeetingExile = false;
                innocent.TargetVoted = true;
            }
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
                innocent.ShowTauntedTargetSymbol = false;
            }
            else if (innocent.AwaitingNextMeetingExile)
            {
                innocent.AwaitingNextMeetingExile = false;
                innocent.WinWindowExpired = true;
                innocent.ShowTauntedTargetSymbol = false;
                innocent.TauntedKillerId = null;
            }
        }
    }

    private static IEnumerable<InnocentRole> GetInnocents()
    {
        return InnocentRole.ActiveInnocents.Values;
    }
}
