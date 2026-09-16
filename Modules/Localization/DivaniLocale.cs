using MiraAPI.Translation;

namespace DivaniMods.Modules.Localization;

public static class DivaniLocale
{
    public static void Register()
    {
        MiraLocaleManager.Register(DivaniPlugin.Id);
    }
}