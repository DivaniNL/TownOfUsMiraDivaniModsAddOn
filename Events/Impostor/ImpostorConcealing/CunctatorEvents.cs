using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using DivaniMods.Options;
using DivaniMods.Patches;
using DivaniMods.Roles.Crewmate.CrewmateKilling;
using DivaniMods.Roles.Impostor.ImpostorConcealing;
using TownOfUs.Utilities;
using TownOfUs.Modules;

namespace DivaniMods.Events.Impostor.ImpostorConcealing;

public static class CunctatorEvents
{
    [RegisterEvent(-100)]
    public static void AfterMurderEventHandler(AfterMurderEvent @event)
    {
        if (@event.Source == null || @event.Target == null)
        {
            return;
        }

        if (@event.Source.Data?.Role is not CunctatorRole)
        {
            return;
        }

        // Only delay bodies of players the Cunctator kills — never its own death
        // (misguess, veteran reflect, suicide-bomb, etc. have source == target).
        if (@event.Source.PlayerId == @event.Target.PlayerId)
        {
            return;
        }

        if (MeetingHud.Instance || ExileController.Instance)
        {
            return;
        }

        if (@event.Target.GetRoleWhenAlive() is RetributionistRole)
        {
            return;
        }

        var delay = OptionGroupSingleton<CunctatorOptions>.Instance.BodyDelay.Value;
        CunctatorBodyManager.Schedule(@event.Target, @event.Source, delay);
    }
}
