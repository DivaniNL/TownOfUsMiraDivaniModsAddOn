using MiraAPI.Events;
using MiraAPI.Events.Mira;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using DivaniMods.Modifiers.Game.Impostor.ImpostorPassive;
using DivaniMods.Options;
using TownOfUs.Buttons;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Neutral;

namespace DivaniMods.Events.Impostor.ImpostorPassive;

public static class RuthlessEvents
{
    [RegisterEvent(2000)]
    public static void BeforeMurderEventHandler(BeforeMurderEvent @event)
    {
        if (@event.IsCancelled)
        {
            TryPierceShield(@event, @event.Source, @event.Target);
        }
    }

    [RegisterEvent(2000)]
    public static void MiraButtonClickEventHandler(MiraButtonClickEvent @event)
    {
        if (!@event.IsCancelled)
        {
            return;
        }

        var source = PlayerControl.LocalPlayer;
        var button = @event.Button as CustomActionButton<PlayerControl>;
        var target = button?.Target;

        if (target == null || button is not IKillButton)
        {
            return;
        }

        TryPierceShield(@event, source, target);
    }

    private static void TryPierceShield(MiraCancelableEvent @event, PlayerControl source, PlayerControl target)
    {
        if (source == null || target == null || source.PlayerId == target.PlayerId)
        {
            return;
        }

        if (!source.HasModifier<RuthlessModifier>())
        {
            return;
        }

        if (target.HasModifier<InvulnerabilityModifier>())
        {
            return;
        }

        if (!OptionGroupSingleton<RuthlessOptions>.Instance.BypassFirstDeathShield &&
            target.HasModifier<FirstDeadShield>())
        {
            return;
        }

        if (target.HasModifier<GuardianAngelProtectModifier>())
        {
            target.RemoveModifier<GuardianAngelProtectModifier>();
        }

        @event.UnCancel();
    }
}
