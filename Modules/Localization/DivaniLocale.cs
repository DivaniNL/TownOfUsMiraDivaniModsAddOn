using AmongUs.GameOptions;
using TownOfUs.Modules.Localization;

namespace DivaniMods.Modules.Localization;

public static class DivaniLocale
{
    public static void Register()
    {
        if (!TouLocale.TouLocalization.TryGetValue(SupportedLangs.English, out var en))
        {
            return;
        }

        RegisterDeathCauses(en);
        RegisterLocalSettings(en);
    }

    private static void RegisterLocalSettings(Dictionary<string, string> en)
    {
        en.TryAdd("DivaniLocalSettingDisableRainbowComms", "Disable Rainbow Comms");
    }

    private static void RegisterDeathCauses(Dictionary<string, string> en)
    {
        en.TryAdd("DiedToSilencer", "Silenced");
        en.TryAdd("DiedToDeadlock", "Deadlocked");
        en.TryAdd("DiedToRecruiter", "Recruited");
        en.TryAdd("DiedToFrag", "Fragged");
        en.TryAdd("DiedToFragile", "Shattered");
        en.TryAdd("DiedToTalkedTrash", "Provoked");
        en.TryAdd("DiedToDemolitionist", "Sabotaged");
        en.TryAdd("DiedToSummoner", "Killed");
        en.TryAdd("DiedToRevenant", "Clawed");
        en.TryAdd("DiedToMosquito", "Stung");
        en.TryAdd("DiedToCunctator", "Delayed");
        en.TryAdd("DiedToObfuscator", "Obfuscated");
        en.TryAdd("DiedToDuelist", "Defeated");
    }
}
