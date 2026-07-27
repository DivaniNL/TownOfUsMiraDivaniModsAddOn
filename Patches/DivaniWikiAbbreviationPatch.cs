using HarmonyLib;
using TMPro;
using TownOfUs.Modules.Wiki;
using UnityEngine;

namespace DivaniMods.Patches;

[HarmonyPatch(typeof(InGameWikiEntry), nameof(InGameWikiEntry.SetData))]
public static class DivaniWikiAbbreviationPatch
{
    private const string Abbreviation = "DM";

    private static void Postfix(InGameWikiEntry __instance, string source)
    {
        if (source != Abbreviation || !ColorUtility.TryParseHtmlString(DivaniCreditsColorPatch.CreditsColor, out var color))
        {
            return;
        }

        var tmp = __instance.EntrySourceTmp.Value;
        tmp.color = color;
        tmp.fontStyle |= FontStyles.Bold;
    }
}
