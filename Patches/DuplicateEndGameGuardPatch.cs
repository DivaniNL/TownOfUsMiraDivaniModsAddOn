using HarmonyLib;
using MiraAPI.GameEnd;
using TownOfUs.GameOver;

namespace DivaniMods.Patches;

[HarmonyPatch(typeof(GameManager), nameof(GameManager.RpcEndGame))]
internal static class DuplicateEndGameGuardPatch
{
    [HarmonyPriority(Priority.VeryHigh)]
    [HarmonyPrefix]
    public static bool Prefix([HarmonyArgument(0)] GameOverReason endReason)
    {
        if (endReason == CustomGameOver.GameOverReason<DrawGameOver>())
        {
            return true;
        }

        return CustomGameOver.Instance is not DrawGameOver;
    }
}
