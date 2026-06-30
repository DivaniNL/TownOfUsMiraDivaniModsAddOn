using HarmonyLib;
using MiraAPI.GameOptions;
using DivaniMods.Modules.Watcher;
using DivaniMods.Options;

namespace DivaniMods.Patches;

[HarmonyPatch]
public static class WatcherMeetingBlockPatch
{
    public static bool ShouldBlock =>
        WatcherLightSystem.IsActive &&
        OptionGroupSingleton<WatcherOptions>.Instance.DisableEmergencyButton.Value;

    [HarmonyPatch(typeof(EmergencyMinigame), nameof(EmergencyMinigame.Begin))]
    [HarmonyPrefix]
    public static bool EmergencyMinigameBeginPrefix(EmergencyMinigame __instance)
    {
        if (!ShouldBlock)
        {
            return true;
        }

        __instance.Close();
        return false;
    }

    [HarmonyPatch(typeof(SystemConsole), nameof(SystemConsole.CanUse))]
    [HarmonyPostfix]
    public static void EmergencyConsoleCanUsePostfix(
        SystemConsole __instance,
        ref bool canUse,
        ref bool couldUse)
    {
        if (!ShouldBlock || !IsEmergencyConsole(__instance))
        {
            return;
        }

        canUse = false;
        couldUse = false;
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CmdReportDeadBody))]
    [HarmonyPrefix]
    public static bool CmdReportDeadBodyPrefix([HarmonyArgument(0)] NetworkedPlayerInfo? target)
    {
        return target != null || !ShouldBlock;
    }

    private static bool IsEmergencyConsole(SystemConsole console)
    {
        return console?.MinigamePrefab != null
            && console.MinigamePrefab.TryCast<EmergencyMinigame>() != null;
    }
}
