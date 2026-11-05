using HarmonyLib;
using DivaniMods.Buttons.Crewmate.CrewmateProtective;

namespace DivaniMods.Patches;

[HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
internal static class DomesmithHudTick
{
    [HarmonyPostfix]
    public static void Postfix()
    {
        DomeManager.Tick();
    }
}

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
internal static class DomesmithGameEndCleanup
{
    [HarmonyPostfix]
    public static void Postfix()
    {
        DomeManager.Clear();
    }
}
