using System.Linq;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.Events.Vanilla.Usables;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using DivaniMods.Modifiers.Crewmate.CrewmateSupport;
using DivaniMods.Options;
using DivaniMods.Roles.Crewmate.CrewmateKilling;
using DivaniMods.Roles.Crewmate.CrewmateSupport;
using TownOfUs.Modifiers.Game.Crewmate;
using TownOfUs.Modules;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;

namespace DivaniMods.Events.Crewmate.CrewmateSupport;

public static class LocatorEvents
{
    private static int ActiveTaskCount;
    private static uint LastUseTaskId = uint.MaxValue;

    [RegisterEvent]
    public static void OnRoundStart(RoundStartEvent @event)
    {
        LocatorRole.MarksThisRound = 0;

        if (@event.TriggeredByIntro)
        {
            ActiveTaskCount = 0;
            LastUseTaskId = uint.MaxValue;
            LocatorRole.MarksRemaining = (int)OptionGroupSingleton<LocatorOptions>.Instance.AbilityUses.Value;
        }
    }

    [RegisterEvent]
    public static void OnCompleteTask(CompleteTaskEvent @event)
    {
        if (@event.Player == null || !@event.Player.AmOwner)
        {
            return;
        }

        if (@event.Player.Data?.Role is not LocatorRole)
        {
            return;
        }

        var opt = OptionGroupSingleton<LocatorOptions>.Instance;
        if (!opt.EarnMoreUses)
        {
            return;
        }

        if (@event.Task != null && @event.Task.Id != LastUseTaskId)
        {
            ++ActiveTaskCount;
            LastUseTaskId = @event.Task.Id;
        }

        var needed = (int)opt.TasksPerUse.Value;
        if (needed > 0 && ActiveTaskCount >= needed)
        {
            ++LocatorRole.MarksRemaining;
            ActiveTaskCount = 0;
        }
    }

    [RegisterEvent]
    public static void OnMeetingStart(StartMeetingEvent _)
    {
        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player == null)
            {
                continue;
            }

            foreach (var modifier in player.GetModifiers<LocatorMarkModifier>().ToList())
            {
                player.RemoveModifier(modifier);
            }
        }
    }

    [RegisterEvent]
    public static void OnAfterMurder(AfterMurderEvent @event)
    {
        if (@event.Target == null || @event.Source == null || MeetingHud.Instance)
        {
            return;
        }

        if (!@event.Target.TryGetModifier<LocatorMarkModifier>(out var mark))
        {
            return;
        }

        var suppressed = @event.Source.IsRole<SoulCollectorRole>()
                         || @event.Target.GetRoleWhenAlive() is RetributionistRole
                         || @event.Target.HasModifier<NoisemakerModifier>();

        if (!suppressed)
        {
            mark.NotifyOfDeath(@event.Target);
        }

        @event.Target.RemoveModifier(mark);
    }
}
