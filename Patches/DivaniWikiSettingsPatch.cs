using AmongUs.GameOptions;
using HarmonyLib;
using MiraAPI.GameOptions;
using DivaniMods.Assets;
using DivaniMods.Options;
using TownOfUs.Modules.Localization;
using TownOfUs.Modules.Wiki;

namespace DivaniMods.Patches;

[HarmonyPatch(typeof(IngameWikiMinigame))]
public static class DivaniWikiSettingsPatch
{
    private const string TitleKey = "WikiSettingsDivaniModsTitle";

    [HarmonyPatch("Awake")]
    [HarmonyPostfix]
    public static void AwakePostfix(IngameWikiMinigame __instance)
    {
        RegisterLocale();
        AddSettings(__instance);
    }

    private static void AddSettings(IngameWikiMinigame instance)
    {
        if (instance == null || instance._activeSettings == null)
        {
            return;
        }

        if (instance._activeSettings.Any(x => x.Title == TitleKey))
        {
            return;
        }

        instance._activeSettings.Add(new OptionWikiInfo(TitleKey,
            new List<AbstractOptionGroup>()
            {
                OptionGroupSingleton<DivaniOptions>.Instance
            }, DivaniAssets.ModNewsLogo));
    }

    public static void RegisterLocale()
    {
        if (!TouLocale.TouLocalization.TryGetValue(SupportedLangs.English, out var english))
        {
            english = new Dictionary<string, string>();
            TouLocale.TouLocalization[SupportedLangs.English] = english;
        }

        english.TryAdd(TitleKey, "DivaniMods Settings");
    }
}
