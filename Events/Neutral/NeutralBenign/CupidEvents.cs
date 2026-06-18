using System.Linq;
using AmongUs.GameOptions;
using MiraAPI.Events;
using MiraAPI.Events.Mira;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using Reactor.Utilities;
using DivaniMods.Modifiers.Neutral.NeutralBenign;
using DivaniMods.Options;
using DivaniMods.Roles.Neutral.NeutralBenign;
using TownOfUs;
using TownOfUs.Buttons;
using TownOfUs.Events;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Modules.Localization;
using TownOfUs.Networking;
using TownOfUs.Options;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;

namespace DivaniMods.Events.Neutral.NeutralBenign;

public static class CupidEvents
{
    [RegisterEvent]
    public static void RoundStartHandler(RoundStartEvent @event)
    {
        if (@event.TriggeredByIntro || !AmongUsClient.Instance.AmHost)
        {
            return;
        }

        foreach (var cupid in CustomRoleUtils.GetActiveRolesOfType<CupidRole>().ToList())
        {
            if (cupid.Finalized || cupid.Player == null || cupid.Player.HasDied())
            {
                continue;
            }

            var alive = cupid.ProvisionalTargets
                .Select(id => MiscUtils.PlayerById(id))
                .Where(p => p != null && !p.HasDied())
                .ToList();

            if (alive.Count >= 2)
            {
                CupidRole.RpcFinalizeLovers(cupid.Player, alive[0]!.PlayerId, alive[1]!.PlayerId);
            }
        }
    }

    [RegisterEvent]
    public static void PlayerDeathHandler(PlayerDeathEvent @event)
    {
        if (!@event.Player)
        {
            return;
        }

        foreach (var cupid in CustomRoleUtils.GetActiveRolesOfType<CupidRole>().ToList())
        {
            CheckLoverDeath(cupid, @event.Player);
        }
    }

    private static void CheckLoverDeath(CupidRole cupid, PlayerControl victim)
    {
        if (cupid.Player == null || cupid.Player.HasDied())
        {
            return;
        }

        var isMine = !cupid.Finalized
            ? cupid.ProvisionalTargets.Contains(victim.PlayerId)
            : cupid.GetCurrentCouple().Any(x => x != null && x.PlayerId == victim.PlayerId);

        if (!isMine)
        {
            return;
        }

        var outcome = (CupidBecomeOptions)OptionGroupSingleton<CupidOptions>.Instance.OnLoverDeath.Value;

        if (outcome == CupidBecomeOptions.CupidDies)
        {
            KillCupid(cupid.Player);
            return;
        }

        var roleType = outcome switch
        {
            CupidBecomeOptions.Crew => (ushort)RoleTypes.Crewmate,
            CupidBecomeOptions.Jester => RoleId.Get<JesterRole>(),
            CupidBecomeOptions.Survivor => RoleId.Get<SurvivorRole>(),
            CupidBecomeOptions.Amnesiac => RoleId.Get<AmnesiacRole>(),
            CupidBecomeOptions.Mercenary => RoleId.Get<MercenaryRole>(),
            _ => (ushort)RoleTypes.Crewmate
        };

        cupid.Player.ChangeRole(roleType);
    }

    private static void KillCupid(PlayerControl cupid)
    {
        var inMeeting = MeetingHud.Instance || ExileController.Instance;
        DeathHandlerModifier.UpdateDeathHandlerImmediate(cupid, TouLocale.Get("DiedToHeartbreak"),
            DeathEventHandlers.CurrentRound,
            inMeeting ? DeathHandlerOverride.SetFalse : DeathHandlerOverride.SetTrue,
            lockInfo: DeathHandlerOverride.SetTrue);

        if (inMeeting)
        {
            cupid.Exiled();
            return;
        }

        if (AmongUsClient.Instance.AmHost)
        {
            cupid.RpcSpecialMurder(cupid, isIndirect: true, resetKillTimer: false,
                teleportMurderer: false, showKillAnim: false, playKillSound: false,
                causeOfDeath: "Heartbreak");
        }
    }

    [RegisterEvent]
    public static void MiraButtonClickEventHandler(MiraButtonClickEvent @event)
    {
        var button = @event.Button as CustomActionButton<PlayerControl>;
        var target = button?.Target;

        if (target == null || button == null || !button.CanClick() || button is not IKillButton)
        {
            return;
        }

        CheckForCupidProtection(@event, target, PlayerControl.LocalPlayer);
    }

    [RegisterEvent]
    public static void MiraButtonCancelledEventHandler(MiraButtonCancelledEvent @event)
    {
        var source = PlayerControl.LocalPlayer;
        var button = @event.Button as CustomActionButton<PlayerControl>;
        var target = button?.Target;
        if (target == null || button is not IKillButton)
        {
            return;
        }

        if (target && !target!.HasModifier<CupidProtectModifier>())
        {
            return;
        }

        ResetButtonTimer(source, button);
    }

    [RegisterEvent]
    public static void BeforeMurderEventHandler(BeforeMurderEvent @event)
    {
        var source = @event.Source;
        if (CheckForCupidProtection(@event, @event.Target, source))
        {
            ResetButtonTimer(source);
        }
    }

    private static bool CheckForCupidProtection(MiraCancelableEvent @event, PlayerControl target, PlayerControl source)
    {
        if (MeetingHud.Instance || ExileController.Instance)
        {
            return false;
        }

        if (!target.HasModifier<CupidProtectModifier>() ||
            source.PlayerId == target.PlayerId ||
            (source.TryGetModifier<IndirectAttackerModifier>(out var indirect) && indirect.IgnoreShield))
        {
            return false;
        }

        @event.Cancel();
        return true;
    }

    private static void ResetButtonTimer(PlayerControl source, CustomActionButton<PlayerControl>? button = null)
    {
        if (!source.AmOwner)
        {
            return;
        }

        var reset = OptionGroupSingleton<GeneralOptions>.Instance.TempSaveCdReset;
        button?.SetTimer(reset);
        source.SetKillTimer(reset);
        Coroutines.Start(MiscUtils.CoFlash(TownOfUsColors.NeutralWiki, alpha: 0.5f));
    }
}
