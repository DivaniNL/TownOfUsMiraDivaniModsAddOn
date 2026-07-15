using HarmonyLib;
using TownOfUs.Modules.Wiki;

namespace DivaniMods.Patches;

[HarmonyPatch(typeof(IngameWikiMinigame), "RemoveNonCaps")]
public static class DivaniWikiAbbreviationPatch
{
    private const string Abbreviation = "DM";

    private static void Postfix(ref string __result)
    {
        if (__result == Abbreviation)
        {
            __result = $"<color={DivaniCreditsColorPatch.CreditsColor}><b>{Abbreviation}</b></color>";
        }
    }
}
