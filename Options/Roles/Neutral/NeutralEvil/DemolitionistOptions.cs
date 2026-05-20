using System.Collections.Generic;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using DivaniMods.Roles.Neutral.NeutralEvil;
using TownOfUs.Interfaces;
using TownOfUs.Utilities;

namespace DivaniMods.Options;

public class DemolitionistOptions : AbstractOptionGroup<DemolitionistRole>, IWikiOptionsSummaryProvider
{
    public override string GroupName => "Demolitionist";

    [ModdedNumberOption("Successful Sabotages To Win", 1f, 4f, 1f)]
    public float SabotagesToWin { get; set; } = 2f;

    [ModdedNumberOption("Plant Cooldown", 10f, 60f, 5f, MiraNumberSuffixes.Seconds)]
    public float PlantCooldown { get; set; } = 30f;

    [ModdedNumberOption("Plant To Sabotage Delay", 1f, 10f, 1f, MiraNumberSuffixes.Seconds)]
    public float PlantToSabotageDelay { get; set; } = 3f;

    public ModdedNumberOption SabotageDurationSkeld { get; } =
        new("Sabotage Duration (Skeld)", 30f, 10f, 120f, 5f, MiraNumberSuffixes.Seconds)
        {
            Visible = () => ShouldShowMapDurationOption(ExpandedMapNames.Skeld),
        };

    public ModdedNumberOption SabotageDurationMiraHQ { get; } =
        new("Sabotage Duration (MIRA HQ)", 45f, 10f, 120f, 5f, MiraNumberSuffixes.Seconds)
        {
            Visible = () => ShouldShowMapDurationOption(ExpandedMapNames.MiraHq),
        };

    public ModdedNumberOption SabotageDurationPolus { get; } =
        new("Sabotage Duration (Polus)", 60f, 10f, 120f, 5f, MiraNumberSuffixes.Seconds)
        {
            Visible = () => ShouldShowMapDurationOption(ExpandedMapNames.Polus),
        };

    public ModdedNumberOption SabotageDurationFungle { get; } =
        new("Sabotage Duration (Fungle)", 60f, 10f, 120f, 5f, MiraNumberSuffixes.Seconds)
        {
            Visible = () => ShouldShowMapDurationOption(ExpandedMapNames.Fungle),
        };

    public ModdedNumberOption SabotageDurationAirship { get; } =
        new("Sabotage Duration (Airship)", 90f, 10f, 180f, 5f, MiraNumberSuffixes.Seconds)
        {
            Visible = () => ShouldShowMapDurationOption(ExpandedMapNames.Airship),
        };

    public float SabotageDuration => GetSabotageDurationOptionForMap(MiscUtils.GetCurrentMap).Value;

    public ModdedEnumOption SabotageStyle { get; } = new(
        "Sabotage Style",
        (int)DemolitionistSabotageStyle.Numpad,
        typeof(DemolitionistSabotageStyle));

    public ModdedNumberOption PlantTime { get; } = new(
        "Plant Time", 5f, 2f, 10f, 1f, MiraNumberSuffixes.Seconds)
    {
        Visible = () => OptionGroupSingleton<DemolitionistOptions>.Instance.IsTimedSabotageStyle,
    };

    public ModdedNumberOption DefuseTime { get; } = new(
        "Defuse Time", 5f, 2f, 10f, 1f, MiraNumberSuffixes.Seconds)
    {
        Visible = () => OptionGroupSingleton<DemolitionistOptions>.Instance.IsTimedSabotageStyle,
    };

    [ModdedToggleOption("Demolitionist Can Vent")]
    public bool CanVent { get; set; } = false;

    [ModdedToggleOption("Disable Exploded Utility For Game")]
    public bool DisableExplodedConsoles { get; set; } = true;

    [ModdedToggleOption("Explosion Kills Active Defusers")]
    public bool ExplosionKillsDefusers { get; set; } = false;

    public bool IsTimedSabotageStyle => (DemolitionistSabotageStyle)SabotageStyle.Value is DemolitionistSabotageStyle.Timed;

    public IReadOnlySet<StringNames> WikiHiddenOptionKeys =>
        ShipStatus.Instance != null
            ? new HashSet<StringNames>
            {
                SabotageDurationSkeld.StringName,
                SabotageDurationMiraHQ.StringName,
                SabotageDurationPolus.StringName,
                SabotageDurationFungle.StringName,
                SabotageDurationAirship.StringName,
            }
            : new HashSet<StringNames>();

    public IEnumerable<string> GetWikiOptionSummaryLines()
    {
        if (ShipStatus.Instance == null)
        {
            return [];
        }

        var option = GetSabotageDurationOptionForMap(MiscUtils.GetCurrentMap);
        var valueStr = FormatWikiNumberValue(option);
        var title = TranslationController.Instance != null
            ? TranslationController.Instance.GetString(option.StringName)
            : option.StringName.ToString();

        return new[] { $"{title}: {valueStr}" };
    }

    private ModdedNumberOption GetSabotageDurationOptionForMap(ExpandedMapNames map) =>
        map switch
        {
            ExpandedMapNames.MiraHq => SabotageDurationMiraHQ,
            ExpandedMapNames.Polus => SabotageDurationPolus,
            ExpandedMapNames.Fungle => SabotageDurationFungle,
            ExpandedMapNames.Airship => SabotageDurationAirship,
            _ => SabotageDurationSkeld,
        };

    private static bool ShouldShowMapDurationOption(ExpandedMapNames mapOption)
    {
        if (ShipStatus.Instance == null)
        {
            return true;
        }

        return mapOption == GetMapDurationOptionKey(MiscUtils.GetCurrentMap);
    }

    private static ExpandedMapNames GetMapDurationOptionKey(ExpandedMapNames currentMap) =>
        currentMap switch
        {
            ExpandedMapNames.MiraHq => ExpandedMapNames.MiraHq,
            ExpandedMapNames.Polus => ExpandedMapNames.Polus,
            ExpandedMapNames.Fungle => ExpandedMapNames.Fungle,
            ExpandedMapNames.Airship => ExpandedMapNames.Airship,
            _ => ExpandedMapNames.Skeld,
        };

    private static string FormatWikiNumberValue(ModdedNumberOption numberOption)
    {
        var optionStr = numberOption.Data.GetValueString(numberOption.Value);
        if (optionStr.Contains(".000"))
        {
            optionStr = optionStr.Replace(".000", "");
        }
        else if (optionStr.Contains(".00"))
        {
            optionStr = optionStr.Replace(".00", "");
        }
        else if (optionStr.Contains(".0"))
        {
            optionStr = optionStr.Replace(".0", "");
        }

        return optionStr;
    }
}

public enum DemolitionistSabotageStyle
{
    Timed,
    Numpad,
}
