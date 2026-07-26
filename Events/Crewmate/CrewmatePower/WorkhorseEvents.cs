using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.GameEnd;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using DivaniMods.GameOver;
using DivaniMods.Modifiers.Crewmate.CrewmatePower;
using DivaniMods.Networking.Crewmate.CrewmatePower;
using DivaniMods.Roles.Crewmate.CrewmatePower;
using TownOfUs.Modifiers.Game.Alliance;

namespace DivaniMods.Events.Crewmate.CrewmatePower;

public static class WorkhorseEvents
{
    [RegisterEvent]
    public static void OnRoundStart(RoundStartEvent @event)
    {
        if (@event.TriggeredByIntro)
        {
            WorkhorseRole.CrewpostorTaskWin = false;
        }
    }

    [RegisterEvent]
    public static void OnCompleteTask(CompleteTaskEvent @event)
    {
        var player = @event.Player;
        if (player?.Data?.Role is not WorkhorseRole workhorse || player.Data.IsDead || player.Data.Disconnected)
        {
            return;
        }

        workhorse.CheckRevealProgress();

        if (!HasFinishedTasks(player))
        {
            return;
        }

        if (player.HasModifier<TasklisttwoModifier>())
        {
            WorkhorseRole.CrewpostorTaskWin = player.HasModifier<CrewpostorModifier>();

            if (AmongUsClient.Instance.AmHost)
            {
                TriggerTaskWin(player);
            }

            return;
        }

        if (!AmongUsClient.Instance.AmHost)
        {
            return;
        }

        var ids = WorkhorseRole.GenerateSecondListTaskIds(player);
        if (ids.Count == 0)
        {
            return;
        }

        WorkhorseRpc.RpcGrantSecondTaskList(player, string.Join(",", ids));
    }

    [RegisterEvent]
    public static void OnPlayerDeath(PlayerDeathEvent @event)
    {
        var workhorse = CustomRoleUtils.GetActiveRolesOfType<WorkhorseRole>()
            .FirstOrDefault(x => x.Player != null && x.Revealed);

        workhorse?.RemoveArrowForPlayer(@event.Player.PlayerId);
    }

    private static void TriggerTaskWin(PlayerControl player)
    {
        if (player.HasModifier<EgotistModifier>())
        {
            CustomGameOver.Trigger<WorkhorseGameOver>([player.Data]);
            return;
        }

        var reason = player.HasModifier<CrewpostorModifier>()
            ? GameOverReason.ImpostorsBySabotage
            : GameOverReason.CrewmatesByTask;

        GameManager.Instance.RpcEndGame(reason, false);
    }

    private static bool HasFinishedTasks(PlayerControl player)
    {
        var tasks = player.myTasks.ToArray()
            .Where(x => !PlayerTask.TaskIsEmergency(x) && x.TryCast<ImportantTextTask>() == null)
            .ToList();

        return tasks.Count > 0 && tasks.All(x => x.IsComplete);
    }
}
