using HarmonyLib;
using MiraAPI.GameOptions;
using DivaniMods.Events.Crewmate.CrewmateKilling;
using DivaniMods.Options;
using TownOfUs.Patches;

namespace DivaniMods.Patches;

[HarmonyPatch]
internal static class RetributionistStallGamePatch
{
    private static bool ShouldStall =>
        OptionGroupSingleton<RetributionistOptions>.Instance.StallGame &&
        RetributionistManager.AnyRevengeActive;

    // Blocks the vanilla end-game logic (impostor majority, crew by tasks, ...).
    [HarmonyPatch(typeof(LogicGameFlowNormal), nameof(LogicGameFlowNormal.CheckEndCriteria))]
    [HarmonyPriority(Priority.First)]
    [HarmonyPrefix]
    public static bool CheckEndCriteriaPrefix()
    {
        return !ShouldStall;
    }

    // Returning false above only skips the original method, not TownOfUs' own prefix, which is
    // where custom win conditions (Betrayer, neutrals, lovers) are evaluated and triggered.
    [HarmonyPatch(typeof(WinConditionRegistry), nameof(WinConditionRegistry.TryEvaluate))]
    [HarmonyPriority(Priority.First)]
    [HarmonyPrefix]
    public static bool TryEvaluatePrefix(ref bool __result)
    {
        if (!ShouldStall)
        {
            return true;
        }

        __result = false;
        return false;
    }

    // Same reason: TownOfUs' prefix can still call RpcEndGame directly before ours takes effect.
    [HarmonyPatch(typeof(GameManager), nameof(GameManager.RpcEndGame))]
    [HarmonyPriority(Priority.First)]
    [HarmonyPrefix]
    public static bool RpcEndGamePrefix()
    {
        return !ShouldStall;
    }
}
