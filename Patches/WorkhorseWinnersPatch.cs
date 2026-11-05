using System.Linq;
using HarmonyLib;
using MiraAPI.GameEnd;
using MiraAPI.Modifiers;
using DivaniMods.GameOver;
using DivaniMods.Roles.Crewmate.CrewmatePower;
using TownOfUs.Modifiers.Game.Alliance;
using TownOfUs.Modules;
using TownOfUs.Roles;
using TownOfUs.Utilities;

namespace DivaniMods.Patches;

// Runs after Mira rebuilds the winner cache from DidWin, but before TOU reads it for the end game summary.
[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
[HarmonyAfter("mira.api")]
[HarmonyBefore("auavengers.tou.mira")]
internal static class WorkhorseWinnersPatch
{
    [HarmonyPostfix]
    public static void Postfix(EndGameResult endGameResult)
    {
        var crewpostorWin = WorkhorseRole.CrewpostorTaskWin;
        WorkhorseRole.CrewpostorTaskWin = false;

        if (endGameResult == null)
        {
            return;
        }

        var egotistWin = endGameResult.GameOverReason == CustomGameOver.GameOverReason<WorkhorseGameOver>();
        crewpostorWin &= endGameResult.GameOverReason == GameOverReason.ImpostorsBySabotage;

        if (!egotistWin && !crewpostorWin)
        {
            return;
        }

        foreach (var player in PlayerControl.AllPlayerControls.ToArray())
        {
            if (player == null || player.Data == null || player.Data.Disconnected)
            {
                continue;
            }

            if (!player.IsImpostorAligned() && !(egotistWin && IsEgotistWinner(player)))
            {
                continue;
            }

            AddWinner(player.Data);
        }
    }

    private static bool IsEgotistWinner(PlayerControl player)
    {
        var role = ResolveRole(player);

        if (role is WorkhorseRole)
        {
            return player.HasModifier<EgotistModifier>();
        }

        return role != null && role.GetRoleAlignment() == RoleAlignment.NeutralKilling;
    }

    private static void AddWinner(NetworkedPlayerInfo data)
    {
        if (EndGameResult.CachedWinners.ToArray().Any(x => x.PlayerName == data.PlayerName))
        {
            return;
        }

        EndGameResult.CachedWinners.Add(new CachedPlayerData(data));
    }

    private static RoleBehaviour? ResolveRole(PlayerControl player)
    {
        var role = player.GetRoleWhenAlive();

        return role is null ? player.Data?.Role : role;
    }
}
