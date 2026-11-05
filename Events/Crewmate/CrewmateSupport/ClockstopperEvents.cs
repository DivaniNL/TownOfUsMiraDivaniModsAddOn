using System.Linq;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.GameOptions;
using DivaniMods.Networking.Crewmate.CrewmateSupport;
using DivaniMods.Options;
using DivaniMods.Roles.Crewmate.CrewmateSupport;
using TownOfUs.Utilities;

namespace DivaniMods.Events.Crewmate.CrewmateSupport;

public static class ClockstopperEvents
{
    private static int ResetsFired;

    public static int GetNeeded()
    {
        return (int)OptionGroupSingleton<ClockstopperOptions>.Instance.TasksPerReset.Value;
    }

    public static int CountCompleted(PlayerControl player)
    {
        if (player == null || player.myTasks == null)
        {
            return 0;
        }

        return player.myTasks.ToArray().Count(x => x != null && x.IsComplete);
    }

    public static int GetProgress(PlayerControl player)
    {
        var needed = GetNeeded();
        return needed <= 0 ? 0 : CountCompleted(player) % needed;
    }

    [RegisterEvent]
    public static void OnRoundStart(RoundStartEvent @event)
    {
        if (@event.TriggeredByIntro)
        {
            ResetsFired = 0;
        }
    }

    [RegisterEvent]
    public static void OnCompleteTask(CompleteTaskEvent @event)
    {
        if (@event.Player == null || !@event.Player.AmOwner)
        {
            return;
        }

        if (@event.Player.Data?.Role is not ClockstopperRole || @event.Player.HasDied())
        {
            return;
        }

        var needed = GetNeeded();
        if (needed <= 0)
        {
            return;
        }

        var due = CountCompleted(@event.Player) / needed;
        if (due <= ResetsFired)
        {
            return;
        }

        ResetsFired = due;
        ClockstopperRpc.RpcResetCooldowns(@event.Player);
    }
}
