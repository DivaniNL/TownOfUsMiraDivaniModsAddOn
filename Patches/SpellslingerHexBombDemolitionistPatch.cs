using HarmonyLib;
using TownOfUs.Buttons.Impostor;

namespace DivaniMods.Patches;

[HarmonyPatch(typeof(SpellslingerHexBombButton), nameof(SpellslingerHexBombButton.CanUse))]
internal static class SpellslingerHexBombDemolitionistPatch
{
    [HarmonyPostfix]
    public static void CanUsePostfix(ref bool __result)
    {
        if (__result && DemolitionistSabotageState.IsActive)
        {
            __result = false;
        }
    }
}
