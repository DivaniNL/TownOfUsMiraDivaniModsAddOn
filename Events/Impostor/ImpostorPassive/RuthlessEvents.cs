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

        if (!TryPierceShield(@event, source, target))
        {
            return;
        }

        button?.SetTimer(0f);
        source.SetKillTimer(0f);
    }

    public static bool CanPierceShields(PlayerControl source, PlayerControl target)
    {
        if (source == null || target == null || source.PlayerId == target.PlayerId)
        {
            return false;
        }

        if (!source.HasModifier<RuthlessModifier>())
        {
            return false;
        }

        if (target.HasModifier<InvulnerabilityModifier>())
        {
            return false;
        }

        if (!OptionGroupSingleton<RuthlessOptions>.Instance.BypassFirstDeathShield &&
            target.HasModifier<FirstDeadShield>())
        {
            return false;
        }

        return true;
    }

    private static bool TryPierceShield(MiraCancelableEvent @event, PlayerControl source, PlayerControl target)
    {
        if (!CanPierceShields(source, target))
        {
            return false;
        }

        if (target.HasModifier<GuardianAngelProtectModifier>())
        {
            target.RemoveModifier<GuardianAngelProtectModifier>();
        }

        @event.UnCancel();
        return true;
    }
}
