using AmongUs.GameOptions;
using HarmonyLib;
using MiraAPI.Utilities;
using DivaniMods.Assets;
using DivaniMods.Roles.Neutral.NeutralEvil;
using TownOfUs.Modules.Localization;
using TownOfUs.Modules.Wiki;

namespace DivaniMods.Patches;

[HarmonyPatch(typeof(IngameWikiMinigame), nameof(IngameWikiMinigame.AddNewTerms))]
public static class DivaniWikiTermsPatch
{
    private const string TitleKey = "DivaniTermsTitle";
    private const string DescKey = "DivaniTermsDesc";

    public static void Postfix(IngameWikiMinigame instance)
    {
        instance._activeTerms.Add(new TermWikiInfo(TitleKey, DescKey, DivaniAssets.ModNewsLogo));
    }

    public static void RegisterLocale()
    {
        TouLocale.TouLocalization[SupportedLangs.English].TryAdd(TitleKey, "DivaniMods Symbols");
        TouLocale.TouLocalization[SupportedLangs.English].TryAdd(DescKey,
            "These symbols are the custom symbols from DivaniMods. " +
            $"\n• Infected players (Plague Doctor) are marked with <b>{PlagueDoctorRole.PlagueDoctorColor.ToTextColor()}µ</color></b> " +
            $"\n• Taunted killers (Innocent) are marked with <b>{InnocentRole.InnocentColor.ToTextColor()}⊕</color></b>"
        );
    }
}
