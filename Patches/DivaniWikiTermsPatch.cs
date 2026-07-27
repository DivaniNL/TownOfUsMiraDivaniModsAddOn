using AmongUs.GameOptions;
using HarmonyLib;
using MiraAPI.Utilities;
using DivaniMods.Assets;
using DivaniMods.Roles.Crewmate.CrewmateSupport;
using DivaniMods.Roles.Neutral.NeutralBenign;
using DivaniMods.Roles.Neutral.NeutralEvil;
using TownOfUs.Modules.Localization;
using TownOfUs.Modules.Wiki;

namespace DivaniMods.Patches;

[HarmonyPatch(typeof(IngameWikiMinigame))]
public static class DivaniWikiTermsPatch
{
    private const string TitleKey = "DivaniTermsTitle";
    private const string DescKey = "DivaniTermsDesc";

    [HarmonyPatch("Awake")]
    [HarmonyPostfix]
    public static void AwakePostfix(IngameWikiMinigame __instance)
    {
        RegisterLocale();
        AddTerm(__instance);
    }

    private static void AddTerm(IngameWikiMinigame instance)
    {
        if (instance == null || instance._activeTerms == null)
        {
            return;
        }

        if (instance._activeTerms.Any(x => x.Title == TitleKey))
        {
            return;
        }

        instance._activeTerms.Add(new TermWikiInfo(TitleKey, DescKey, DivaniAssets.ModNewsLogo));
    }

    public static void RegisterLocale()
    {
        if (!TouLocale.TouLocalization.TryGetValue(SupportedLangs.English, out var english))
        {
            english = new Dictionary<string, string>();
            TouLocale.TouLocalization[SupportedLangs.English] = english;
        }

        english.TryAdd(TitleKey, "DivaniMods Symbols");
        english.TryAdd(DescKey,
            "These symbols are the custom symbols from DivaniMods. " +
            $"\n• Infected players (Plague Doctor) are marked with <b>{PlagueDoctorRole.PlagueDoctorColor.ToTextColor()}µ</color></b> " +
            $"\n• Taunted killers (Innocent) are marked with <b>{InnocentRole.InnocentColor.ToTextColor()}⊕</color></b>" +
            $"\n• Players marked by the Locator are shown with <b>{LocatorRole.LocatorColor.ToTextColor()}※</color></b>" +
            $"\n• Provisional lovers (Cupid) are marked with <b>{CupidRole.CupidColor.ToTextColor()}♡</color></b>" +
            $"\n• Lovers (Cupid) are marked with <b>{CupidRole.CupidColor.ToTextColor()}♥</color></b>"
        );
    }
}
