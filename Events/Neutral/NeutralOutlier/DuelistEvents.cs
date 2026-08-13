using System.Collections;
using System.Linq;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Utilities;
using DivaniMods.Assets;
using DivaniMods.Modifiers.Neutral.NeutralOutlier;
using DivaniMods.Modules.Duelist;
using DivaniMods.Roles.Neutral.NeutralOutlier;
using TownOfUs.Events;
using TownOfUs.Events.TouEvents;
using TownOfUs.Modifiers;
using TownOfUs.Modules.Localization;
using TownOfUs.Utilities;
using UnityEngine;

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

        if (!DuelManager.IsSanctionedKill(src.PlayerId, tgt.PlayerId))
        {
            evt.Cancel();
        }
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
        TownOfUs.Modules.GameHistory.UpdatePlayerDeathData(
            tgt, cause,
            roundOfDeath: TownOfUs.Modules.Components.HudManagerHelper.Instance.CurrentRound,
            diedThisRound: TownOfUs.Modules.DeathHandlerOverride.SetTrue,
            lockInfo: TownOfUs.Modules.DeathHandlerOverride.SetTrue);

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
    public static void OnEjection(EjectionEvent evt)
    {
        var duelist = CustomRoleUtils.GetActiveRolesOfType<DuelistRole>().FirstOrDefault();
        if (duelist is { VictoryPending: true } && !duelist.Player.HasDied() &&
            Helpers.GetAlivePlayers().Count > 3)
        {
            Coroutines.Start(CoShowLeaveNotification(duelist.Player));

            TownOfUs.Modules.GameHistory.UpdatePlayerDeathData(duelist.Player, TouLocale.Get("DiedToWinning"),
                roundOfDeath: TownOfUs.Modules.Components.HudManagerHelper.Instance.CurrentRound,
                diedThisRound: TownOfUs.Modules.DeathHandlerOverride.SetFalse,
                lockInfo: TownOfUs.Modules.DeathHandlerOverride.SetTrue);

            duelist.Player.Exiled();
        }
    }

    [RegisterEvent]
    public static void OnRoundStart(RoundStartEvent evt)
    {
        if (evt.TriggeredByIntro)
        {
            DuelManager.ResetAll();
            return;
        }

        var duelist = CustomRoleUtils.GetActiveRolesOfType<DuelistRole>().FirstOrDefault();
        if (duelist is { VictoryPending: true } && !duelist.Player.HasDied() &&
            Helpers.GetAlivePlayers().Count > 3)
        {
            Coroutines.Start(CoShowLeaveNotification(duelist.Player));

            TownOfUs.Modules.GameHistory.UpdatePlayerDeathData(duelist.Player, TouLocale.Get("DiedToWinning"),
                roundOfDeath: TownOfUs.Modules.Components.HudManagerHelper.Instance.CurrentRound,
                diedThisRound: TownOfUs.Modules.DeathHandlerOverride.SetFalse,
                lockInfo: TownOfUs.Modules.DeathHandlerOverride.SetTrue);

            duelist.Player.Exiled();
        }
    }

    private static IEnumerator CoShowLeaveNotification(PlayerControl duelist)
    {
        while (ExileController.Instance != null || MeetingHud.Instance != null)
        {
            yield return null;
        }

        if (duelist == null || duelist.Data == null)
        {
            yield break;
        }

        var hex = ColorUtility.ToHtmlStringRGB(DuelistRole.DuelistColor);
        var text = duelist.AmOwner
            ? $"<b><color=#{hex}>You have successfully won as the Duelist, as you have won enough duels!</color></b>"
            : $"<b><color=#{hex}>The Duelist, {duelist.Data.PlayerName}, has successfully won, as they have won enough duels!</color></b>";

        var notif = Helpers.CreateAndShowNotification(
            text, Color.white, new Vector3(0f, 1f, -20f), spr: DivaniAssets.DuelistIcon.LoadAsset());

        notif.AdjustNotification();
    }
}
