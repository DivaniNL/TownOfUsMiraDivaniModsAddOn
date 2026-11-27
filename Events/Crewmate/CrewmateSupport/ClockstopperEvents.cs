using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.GameOptions;
using DivaniMods.Buttons.Crewmate.CrewmateSupport;
using DivaniMods.Options;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;

namespace DivaniMods.Events.Crewmate.CrewmateSupport;

public static class ClockstopperEvents
{
    [RegisterEvent]
    public static void OnRoundStart(RoundStartEvent @event)
    {
        if (@event.TriggeredByIntro)
        {
            ClockstopperButton.ResetCharges();
        }
    }

    [RegisterEvent]
    public static void OnCompleteTask(CompleteTaskEvent @event)
    {
        var player = @event.Player;
        if (player == null || !player.AmOwner || player.HasDied())
        {
            return;
        }

        if (player.Data?.Role is not TimeLordRole ||
            !OptionGroupSingleton<ClockstopperAbilityOptions>.Instance.Enabled)
        {
            return;
        }

        ClockstopperButton.GrantTaskProgress();
    }
}
