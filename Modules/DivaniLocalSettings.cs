using BepInEx.Configuration;
using MiraAPI.LocalSettings;
using DivaniMods.Assets;
using TownOfUs.LocalSettings.Attributes;
using TownOfUs.LocalSettings.SettingTypes;
using TownOfUs.Modules.Localization;

namespace DivaniMods.Modules;

public class DivaniLocalSettings(ConfigFile config) : LocalSettingsTab(config)
{
    public override string TabName => "Divani Mods";
    protected override bool ShouldCreateLabels => true;

    public override void Open()
    {
        base.Open();

        foreach (var entry in TouLocale.LocalizedToggles)
        {
            var toggleObject = entry.Key;
            LocalizedLocalToggleSetting.UpdateToggleText(toggleObject.Text, entry.Value, toggleObject.onState);
        }
    }

    public override LocalSettingTabAppearance TabAppearance => new()
    {
        TabIcon = DivaniAssets.LocalSettingsTabIcon
    };

    [LocalizedLocalToggleSetting("DivaniLocalSettingDisableRainbowComms")]
    public ConfigEntry<bool> DisableRainbowComms { get; private set; } =
        config.Bind("Accessibility", "DisableRainbowComms", false);
}
