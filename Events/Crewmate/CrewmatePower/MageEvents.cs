using MiraAPI.Events;
using MiraAPI.Events.Mira;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using DivaniMods.Modifiers.Crewmate.CrewmatePower;
using DivaniMods.Modules;
using DivaniMods.Options;
using DivaniMods.Roles.Crewmate.CrewmatePower;
using TownOfUs.Buttons;
using TownOfUs.Modifiers;

namespace DivaniMods.Events.Crewmate.CrewmatePower;

public static class MageEvents
{
    [RegisterEvent]
    public static void RoundStartHandler(RoundStartEvent @event)
    {
        if (@event.TriggeredByIntro)
        {
            return;
        }

        MageEnergize.ApplyPending();
    }

    [RegisterEvent(1)]
    public static void MiraButtonClickHandler(MiraButtonClickEvent @event)
    {
        var source = PlayerControl.LocalPlayer;
        var button = @event.Button as CustomActionButton<PlayerControl>;
        var target = button?.Target;

        if (source == null || target == null || button is not IKillButton || !button.CanClick())
        {
            return;
        }

        CheckForShockShield(@event, source, target);
    }

    [RegisterEvent(1)]
    public static void BeforeMurderHandler(BeforeMurderEvent @event)
    {
        CheckForShockShield(@event, @event.Source, @event.Target);
    }

    private static void CheckForShockShield(MiraCancelableEvent miraEvent, PlayerControl source, PlayerControl target)
    {
        if (MeetingHud.Instance || ExileController.Instance)
        {
            return;
        }
        if (source == null || target == null || source.PlayerId == target.PlayerId)
        {
            return;
        }
        if (!target.HasModifier<ShockShieldModifier>())
        {
            return;
        }

        var preventAttack = source.TryGetModifier<IndirectAttackerModifier>(out var indirectMod);

        if (indirectMod == null || !indirectMod.IgnoreShield)
        {
            miraEvent.Cancel();
        }

        // Pestilence / invulnerable attackers can't be reflected without softlocking the game.
        if (source.HasModifier<InvulnerabilityModifier>())
        {
            return;
        }

        if ((TutorialManager.InstanceExists || source.AmOwner) && !preventAttack)
        {
            target.RpcCustomMurder(source, MeetingCheck.OutsideMeeting);

            var shield = target.GetModifier<ShockShieldModifier>();
            if (shield?.Mage != null && OptionGroupSingleton<MageOptions>.Instance.MageNotifiedOnAttack.Value)
            {
                MageRole.RpcShockShieldAttacked(shield.Mage, source, target);
            }
        }
    }
}
