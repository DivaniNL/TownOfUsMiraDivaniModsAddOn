using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using DivaniMods.Modifiers;
using DivaniMods.Modules.Watcher;
using TownOfUs.Buttons;

namespace DivaniMods.Patches;

[HarmonyPatch]
public static class WatcherKillButtonExemptPatch
{
    private static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(typeof(TownOfUsButton), nameof(TownOfUsButton.CanUse));
        yield return AccessTools.Method(typeof(TownOfUsTargetButton<PlayerControl>), nameof(TownOfUsButton.CanUse));
    }

    [HarmonyPrefix]
    public static void Prefix(object __instance)
    {
        if (__instance is IKillButton && WatcherLightSystem.IsRedLightActive)
        {
            WatcherWatchedModifier.KillButtonContext = true;
        }
    }

    [HarmonyFinalizer]
    public static void Finalizer()
    {
        WatcherWatchedModifier.KillButtonContext = false;
    }
}
