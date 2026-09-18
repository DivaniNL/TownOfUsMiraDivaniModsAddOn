using HarmonyLib;
using MiraAPI.GameOptions;
using DivaniMods.Assets;
using DivaniMods.Options;
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

    // Removed in favor of Miralocale
    // public static void RegisterLocale()
    // {
    //     if (!MiraLocaleManager.Locale.TryGetValue(MiraLanguage.English, out var english))
    //     {
    //         english = new Dictionary<string, string>();
    //         MiraLocaleManager.Locale[MiraLanguage.English] = english;
    //     }

    //     english.TryAdd(TitleKey, "DivaniMods Settings");
    // }
}
