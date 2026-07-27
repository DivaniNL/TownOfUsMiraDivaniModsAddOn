using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using DivaniMods.Options;
using DivaniMods.Roles.Neutral.NeutralKilling;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modifiers.Neutral;

namespace DivaniMods.Events.Misc;

public static class VeteranAlertPierceEvents
{
    [RegisterEvent(2000)]
    public static void BeforeMurderEventHandler(BeforeMurderEvent @event)
    {
        if (!@event.IsCancelled)
        {
            return;
        }

        var source = @event.Source;

        if (!CanPierceVeteranAlert(source, @event.Target) ||
            source?.HasModifier<IndirectAttackerModifier>() != true)
        {
            return;
        }

        @event.UnCancel();
    }

    public static bool CanPierceVeteranAlert(PlayerControl? source, PlayerControl? target)
    {
        if (source == null || target == null || source.PlayerId == target.PlayerId)
        {
            return false;
        }

        if (!target.HasModifier<VeteranAlertModifier>() || HasOtherProtection(target))
        {
            return false;
        }

        return source.Data?.Role switch
        {
            FragRole => OptionGroupSingleton<FragOptions>.Instance.NegatesVeteranAlert.Value,
            WatcherRole => true,
            _ => false,
        };
    }

    private static bool HasOtherProtection(PlayerControl target)
    {
        if (target.HasModifier<InvulnerabilityModifier>() ||
            target.HasModifier<GuardianAngelProtectModifier>() ||
            target.HasModifier<FirstDeadShield>())
        {
            return true;
        }

        foreach (var modifier in target.GetModifiers<BaseModifier>())
        {
            var type = modifier.GetType();
            while (type != null && type != typeof(object))
            {
                if (type.Name == "BaseShieldModifier")
                {
                    return true;
                }

                type = type.BaseType;
            }
        }

        return false;
    }
}
