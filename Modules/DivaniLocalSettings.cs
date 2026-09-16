using BepInEx.Configuration;
using MiraAPI.LocalSettings;
using DivaniMods.Assets;
using MiraAPI.LocalSettings.Attributes;
using MiraAPI.LocalSettings.SettingTypes;
using TownOfUs.Modules.Localization;
using MiraAPI.Translation;

namespace DivaniMods.Modules;

public class DivaniLocalSettings(ConfigFile config) : LocalSettingsTab(config)
{
    public override string TabName => "Divani Mods";
    protected override bool ShouldCreateLabels => true;

    public override LocalSettingTabAppearance TabAppearance => new()
    {
        TabIcon = DivaniAssets.LocalSettingsTabIcon
    };

    [LocalToggleSetting("DivaniLocalSettingDisableRainbowComms")]
    public ConfigEntry<bool> DisableRainbowComms { get; private set; } =
        config.Bind("Accessibility", "Disable Rainbow Comms", false);

    [LocalToggleSetting("DivaniLocalSettingDisableDemoAlternatingColors")]
    public ConfigEntry<bool> DisableDemoAlternatingColors { get; private set; } =
        config.Bind("Accessibility", "Disable Demo Alternating Colors", false);
}
