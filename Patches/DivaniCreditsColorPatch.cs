using System.Text.RegularExpressions;
using HarmonyLib;

namespace DivaniMods.Patches;

[HarmonyPatch(typeof(Reactor.Utilities.ReactorCredits), "GetText")]
public static class DivaniCreditsColorPatch
{
    private const string CreditsColor = "#FF9B00";
    private const string CreditsLabel = "Divani Mods " + DivaniPlugin.Version;
    private const string CreditsDisplay = "Divani Mods v" + DivaniPlugin.Version + "-starlight-betatesting";

    private static void Postfix(ref string? __result)
    {
        if (string.IsNullOrEmpty(__result))
        {
            return;
        }

        var coloredLabel = $"<color={CreditsColor}><noparse>{CreditsDisplay}</noparse></color>";
        var updated = Regex.Replace(
            __result,
            $@"<color=#[0-9A-Fa-f]{{3,8}}><noparse>{Regex.Escape(CreditsLabel)}</noparse></color>",
            coloredLabel);

        if (ReferenceEquals(updated, __result) || updated == __result)
        {
            updated = __result.Replace($"<noparse>{CreditsLabel}</noparse>", coloredLabel);
        }

        if (ReferenceEquals(updated, __result) || updated == __result)
        {
            updated = __result.Replace(CreditsLabel, coloredLabel);
        }

        __result = updated;
    }
}
