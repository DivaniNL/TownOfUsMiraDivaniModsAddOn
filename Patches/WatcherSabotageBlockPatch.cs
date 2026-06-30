using HarmonyLib;
using DivaniMods.Modules.Watcher;

namespace DivaniMods.Patches;

[HarmonyPatch(typeof(SabotageSystemType), nameof(SabotageSystemType.UpdateSystem))]
public static class WatcherSabotageBlockPatch
{
    [HarmonyPrefix]
    public static bool Prefix([HarmonyArgument(0)] PlayerControl player)
    {
        if (!AmongUsClient.Instance.AmHost || player == null)
        {
            return true;
        }

        return !WatcherLightSystem.BlocksSabotage(player);
    }
}
