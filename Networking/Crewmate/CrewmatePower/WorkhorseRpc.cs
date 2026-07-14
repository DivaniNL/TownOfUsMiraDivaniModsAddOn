using System.Globalization;
using MiraAPI.Modifiers;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using DivaniMods.Modifiers.Crewmate.CrewmatePower;
using DivaniMods.Roles.Crewmate.CrewmatePower;
using TownOfUs.Utilities;
using Object = UnityEngine.Object;

namespace DivaniMods.Networking.Crewmate.CrewmatePower;

public static class WorkhorseRpc
{
    [MethodRpc((uint)DivaniRpcCalls.WorkhorseGrantSecondList, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcGrantSecondTaskList(PlayerControl player, string taskTypeIds)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(player);
            return;
        }

        if (player == null || player.Data == null || ShipStatus.Instance == null || GameData.Instance == null)
        {
            return;
        }

        if (player.HasModifier<TasklisttwoModifier>())
        {
            return;
        }

        var ids = taskTypeIds.Split(',')
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => byte.TryParse(x, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id) ? id : (byte?)null)
            .Where(x => x.HasValue)
            .Select(x => x!.Value)
            .ToList();

        if (ids.Count == 0)
        {
            return;
        }

        var nextId = 0u;
        foreach (var existing in player.Data.Tasks)
        {
            if (existing.Id >= nextId)
            {
                nextId = existing.Id + 1;
            }
        }

        var role = player.Data.Role as WorkhorseRole;

        foreach (var typeId in ids)
        {
            var prefab = ShipStatus.Instance.GetTaskById(typeId);
            if (prefab == null)
            {
                continue;
            }

            var info = new NetworkedPlayerInfo.TaskInfo(typeId, nextId++);
            player.Data.Tasks.Add(info);

            var task = Object.Instantiate(prefab, player.transform);
            task.Id = info.Id;
            task.Owner = player;
            task.Initialize();
            player.myTasks.Add(task);

            role?.SecondListTaskIds.Add(info.Id);
        }

        player.AddModifier<TasklisttwoModifier>();
        GameData.Instance.RecomputeTaskCounts();

        role?.OnSecondListGranted();
    }
}
