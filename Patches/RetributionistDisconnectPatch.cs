using HarmonyLib;
using DivaniMods.Events.Crewmate.CrewmateKilling;

namespace DivaniMods.Patches;

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerLeft))]
internal static class RetributionistDisconnectPatch
{
    [HarmonyPostfix]
    public static void Postfix()
    {
        RetributionistManager.PurgeDisconnected();
    }
}
