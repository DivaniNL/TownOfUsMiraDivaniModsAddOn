using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using DivaniMods.Modifiers.Crewmate.CrewmatePower;
using DivaniMods.Networking.Crewmate.CrewmatePower;
using DivaniMods.Roles.Crewmate.CrewmatePower;

namespace DivaniMods.Events.Crewmate.CrewmatePower;

public static class WorkhorseEvents
{
    [RegisterEvent]
    public static void OnCompleteTask(CompleteTaskEvent @event)
    {
        var player = @event.Player;
        if (player?.Data?.Role is not WorkhorseRole workhorse || player.Data.IsDead || player.Data.Disconnected)
        {
            return;
        }

        workhorse.CheckRevealProgress();

        if (!AmongUsClient.Instance.AmHost || !HasFinishedTasks(player))
        {
            return;
        }

        if (player.HasModifier<TasklisttwoModifier>())
        {
            GameManager.Instance.RpcEndGame(GameOverReason.CrewmatesByTask, false);
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

    private static bool HasFinishedTasks(PlayerControl player)
    {
        var tasks = player.myTasks.ToArray()
            .Where(x => !PlayerTask.TaskIsEmergency(x) && x.TryCast<ImportantTextTask>() == null)
            .ToList();

        return tasks.Count > 0 && tasks.All(x => x.IsComplete);
    }
}
