using System.Linq;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using DivaniMods.Modifiers.Neutral.NeutralOutlier;
using DivaniMods.Modules.Duelist;
using DivaniMods.Options;
using TownOfUs.Events;
using TownOfUs.Events.TouEvents;
using TownOfUs.Modifiers;
using TownOfUs.Modules.Localization;

namespace DivaniMods.Events.Neutral.NeutralOutlier;

public static class DuelistEvents
{
    [RegisterEvent]
    public static void OnBeforeMurder(BeforeMurderEvent evt)
    {
        var src = evt.Source;
        var tgt = evt.Target;
        if (src == null || tgt == null)
        {
            return;
        }

        var srcDuel = src.TryGetModifier<DuelModifier>(out var sm);
        var tgtDuel = tgt.TryGetModifier<DuelModifier>(out var tm);
        if (!srcDuel && !tgtDuel)
        {
            return;
        }

        if (srcDuel != tgtDuel)
        {
            evt.Cancel();
            return;
        }

        if (sm!.OpponentId != tgt.PlayerId || tm!.OpponentId != src.PlayerId)
        {
            evt.Cancel();
            return;
        }

        if (DuelManager.IsResolved(src.PlayerId) || DuelManager.IsResolved(tgt.PlayerId))
        {
            evt.Cancel();
            return;
        }

        if (tm.IsDuelist)
        {
            DuelManager.AddLoss(tgt.PlayerId);

            var lossesToDie = (int)OptionGroupSingleton<DuelistOptions>.Instance.DuelsLostToDie.Value;
            if (DuelManager.GetLosses(tgt.PlayerId) >= lossesToDie)
            {
                DuelManager.MarkResolved(src.PlayerId, tgt.PlayerId);
                return;
            }

            DuelManager.MarkResolved(src.PlayerId, tgt.PlayerId);
            evt.Cancel();
            DuelManager.EndDuel(src, tgt, false);
            return;
        }

        if (sm.IsDuelist)
        {
            DuelManager.AddWin(src.PlayerId);
        }
        DuelManager.MarkResolved(src.PlayerId, tgt.PlayerId);
    }

    [RegisterEvent]
    public static void OnAfterMurder(AfterMurderEvent evt)
    {
        var src = evt.Source;
        var tgt = evt.Target;
        if (src == null || tgt == null)
        {
            return;
        }

        if (!src.TryGetModifier<DuelModifier>(out var sm) || !tgt.HasModifier<DuelModifier>() ||
            sm.OpponentId != tgt.PlayerId)
        {
            return;
        }

        DuelManager.MarkDuelDeath(tgt.PlayerId);

        var cause = TouLocale.Get("DiedToDuelist");
        DeathHandlerModifier.UpdateDeathHandlerImmediate(
            tgt, cause, DeathEventHandlers.CurrentRound, DeathHandlerOverride.SetTrue,
            TouLocale.GetParsed("DiedByStringBasic").Replace("<player>", src.Data.PlayerName),
            lockInfo: DeathHandlerOverride.SetTrue);

        DuelManager.EndDuel(src, tgt, true);
    }

    [RegisterEvent]
    public static void OnPlayerRevive(PlayerReviveEvent evt)
    {
        var p = evt.Player;
        if (p == null || !DuelManager.DiedInDuel(p.PlayerId))
        {
            return;
        }

        if (DuelManager.GetLosses(p.PlayerId) <= 0)
        {
            return;
        }

        DuelManager.RefundLoss(p.PlayerId);
    }

    [RegisterEvent]
    public static void OnStartMeeting(StartMeetingEvent _)
    {
        foreach (var p in PlayerControl.AllPlayerControls)
        {
            if (p != null && p.TryGetModifier<DuelModifier>(out var mod))
            {
                p.RemoveModifier(mod);
            }
        }
        DuelManager.ClearActiveDuelers();
    }

    [RegisterEvent]
    public static void OnRoundStart(RoundStartEvent evt)
    {
        if (evt.TriggeredByIntro)
        {
            DuelManager.ResetAll();
        }
    }
}
