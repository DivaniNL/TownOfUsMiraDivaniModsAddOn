using HarmonyLib;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using DivaniMods.Roles.Crewmate.CrewmatePower;
using TownOfUs.Extensions;
using TownOfUs.Modifiers.Game;
using TownOfUs.Utilities;

namespace DivaniMods.Patches;

[HarmonyPatch(typeof(GameData), nameof(GameData.RecomputeTaskCounts))]
public static class OverworkedTaskCountPatch
{
    [HarmonyPostfix]
    public static void Postfix(GameData __instance)
    {
        if (__instance == null || GameManager.Instance == null)
        {
            return;
        }

        var optionsManager = GameOptionsManager.Instance;
        var ghostsDoTasks = optionsManager != null && optionsManager.currentNormalGameOptions != null &&
                            optionsManager.currentNormalGameOptions.GhostsDoTasks;
        var removed = false;

        foreach (var role in CustomRoleUtils.GetActiveRolesOfType<OverworkedRole>())
        {
            var player = role.Player;
            if (player == null || !player || player.Data == null || player.Data.Disconnected ||
                role.SecondListTaskIds.Count == 0)
            {
                continue;
            }

            if ((player.Data.IsDead && !ghostsDoTasks) || player.IsImpostor())
            {
                continue;
            }

            if (player.Data.Role == null || !player.Data.Role.TasksCountTowardProgress)
            {
                continue;
            }

            if (player.TryGetModifier<AllianceGameModifier>(out var alliance) && !alliance.DoesTasks)
            {
                continue;
            }

            foreach (var id in role.SecondListTaskIds)
            {
                var task = player.Data.FindTaskById(id);
                if (task == null)
                {
                    continue;
                }

                if (__instance.TotalTasks > 0)
                {
                    __instance.TotalTasks--;
                    removed = true;
                }

                if (task.Complete && __instance.CompletedTasks > 0)
                {
                    __instance.CompletedTasks--;
                }
            }
        }

        if (removed && __instance.TotalTasks <= 0)
        {
            __instance.TotalTasks = 1;
            __instance.CompletedTasks = 0;
        }
    }
}
