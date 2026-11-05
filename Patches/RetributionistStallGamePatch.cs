using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Networking;
using DivaniMods.Events.Crewmate.CrewmateKilling;
using DivaniMods.Options;
using TownOfUs.Patches;

namespace DivaniMods.Patches;

[HarmonyPatch]
internal static class RetributionistStallGamePatch
{
    private static bool ShouldStall =>
        RetributionistManager.AnyReviveInProgress ||
        (OptionGroupSingleton<RetributionistOptions>.Instance.StallGame &&
         RetributionistManager.AnyRevengeActive);

    [HarmonyPatch(typeof(LogicGameFlowNormal), nameof(LogicGameFlowNormal.CheckEndCriteria))]
    [HarmonyPriority(Priority.First)]
    [HarmonyPrefix]
    public static bool CheckEndCriteriaPrefix()
    {
        return !ShouldStall;
    }

    [HarmonyPatch(typeof(LogicGameFlowPatches), nameof(LogicGameFlowPatches.CheckEndCriteriaPatch))]
    [HarmonyPriority(Priority.First)]
    [HarmonyPrefix]
    public static bool TouCheckEndCriteriaPrefix(ref bool __result)
    {
        if (!ShouldStall)
        {
            return true;
        }

        __result = false;
        return false;
    }

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

    [HarmonyPatch(typeof(GameManager), nameof(GameManager.RpcEndGame))]
    [HarmonyPriority(Priority.First)]
    [HarmonyPrefix]
    public static bool RpcEndGamePrefix()
    {
        return !ShouldStall;
    }

    [HarmonyPatch(typeof(CustomGameOverRpc), nameof(CustomGameOverRpc.Handle))]
    [HarmonyPriority(Priority.First)]
    [HarmonyPrefix]
    public static bool CustomGameOverHandlePrefix()
    {
        return !ShouldStall;
    }
}
