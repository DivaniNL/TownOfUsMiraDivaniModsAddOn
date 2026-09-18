using System.Collections.Generic;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using DivaniMods.Roles.Crewmate.CrewmateKilling;
using TownOfUs.Interfaces;
using TownOfUs.Utilities;

namespace DivaniMods.Options;

public class RetributionistOptions : AbstractRoleOptionGroup<RetributionistRole>, IWikiOptionsSummaryProvider
{
    public override string GroupName => MiraLocaleManager.Get("DivaniMods.Role.Retributionist", "Retributionist");

    public ModdedNumberOption RevengeTimerSkeld { get; } =
        new(MiraLocaleManager.Get("DivaniMods.Options.Retributionist.RevengeTimer.Skeld"), 30f, 10f, 120f, 5f, MiraNumberSuffixes.Seconds)
        {
            Visible = () => ShouldShowMapTimerOption(ExpandedMapNames.Skeld),
        };

    public ModdedNumberOption RevengeTimerMiraHQ { get; } =
        new(MiraLocaleManager.Get("DivaniMods.Options.Retributionist.RevengeTimer.MiraHQ"), 45f, 10f, 120f, 5f, MiraNumberSuffixes.Seconds)
        {
            Visible = () => ShouldShowMapTimerOption(ExpandedMapNames.MiraHq),
        };

    public ModdedNumberOption RevengeTimerPolus { get; } =
        new(MiraLocaleManager.Get("DivaniMods.Options.Retributionist.RevengeTimer.Polus"), 60f, 10f, 120f, 5f, MiraNumberSuffixes.Seconds)
        {
            Visible = () => ShouldShowMapTimerOption(ExpandedMapNames.Polus),
        };

    public ModdedNumberOption RevengeTimerFungle { get; } =
        new(MiraLocaleManager.Get("DivaniMods.Options.Retributionist.RevengeTimer.Fungle"), 60f, 10f, 120f, 5f, MiraNumberSuffixes.Seconds)
        {
            Visible = () => ShouldShowMapTimerOption(ExpandedMapNames.Fungle),
        };

    public ModdedNumberOption RevengeTimerAirship { get; } =
        new(MiraLocaleManager.Get("DivaniMods.Options.Retributionist.RevengeTimer.Airship"), 90f, 10f, 180f, 5f, MiraNumberSuffixes.Seconds)
        {
            Visible = () => ShouldShowMapTimerOption(ExpandedMapNames.Airship),
        };

    public float RevengeTimer => GetRevengeTimerOptionForMap(MiscUtils.GetCurrentMap).Value;

    public ModdedNumberOption VengefulSoulSpeed { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.Retributionist.VengefulSoulSpeed"), 1.00f, 0.90f, 1.25f, 0.05f, MiraNumberSuffixes.Multiplier, "0.00");

    [ModdedEnumOption("DivaniMods.Options.Retributionist.VengefulSoulVisibleTo", typeof(VengefulSoulVisibility))]
    public VengefulSoulVisibility SoulVisibleTo { get; set; } = VengefulSoulVisibility.All;

    [ModdedToggleOption("DivaniMods.Options.Retributionist.TurnIntoSoulOnce")]
    public bool TurnIntoSoulOnce { get; set; } = true;

    [ModdedToggleOption("DivaniMods.Options.Retributionist.StallGame")]
    public bool StallGame { get; set; } = false;

    [ModdedToggleOption("DivaniMods.Options.Retributionist.RevengeBreaksShields")]
    public bool RevengeBreaksShields { get; set; } = false;

    [ModdedToggleOption("DivaniMods.Options.Retributionist.RevengeOnCrewmateKill")]
    public bool RevengeOnCrewmateKill { get; set; } = false;

    public IReadOnlySet<StringNames> WikiHiddenOptionKeys =>
        ShipStatus.Instance != null
            ? new HashSet<StringNames>
            {
                RevengeTimerSkeld.StringName,
                RevengeTimerMiraHQ.StringName,
                RevengeTimerPolus.StringName,
                RevengeTimerFungle.StringName,
                RevengeTimerAirship.StringName,
            }
            : new HashSet<StringNames>();

    public IEnumerable<string> GetWikiOptionSummaryLines()
    {
        if (ShipStatus.Instance == null)
        {
            return [];
        }

        var option = GetRevengeTimerOptionForMap(MiscUtils.GetCurrentMap);
        var valueStr = FormatWikiNumberValue(option);
        var title = TranslationController.Instance != null
            ? TranslationController.Instance.GetString(option.StringName)
            : option.StringName.ToString();

        return new[] { $"{title}: {valueStr}" };
    }

    private ModdedNumberOption GetRevengeTimerOptionForMap(ExpandedMapNames map) =>
        map switch
        {
            ExpandedMapNames.MiraHq => RevengeTimerMiraHQ,
            ExpandedMapNames.Polus => RevengeTimerPolus,
            ExpandedMapNames.Fungle => RevengeTimerFungle,
            ExpandedMapNames.Airship => RevengeTimerAirship,
            _ => RevengeTimerSkeld,
        };

    private static bool ShouldShowMapTimerOption(ExpandedMapNames mapOption)
    {
        if (ShipStatus.Instance == null)
        {
            return true;
        }

        return mapOption == GetMapTimerOptionKey(MiscUtils.GetCurrentMap);
    }

    private static ExpandedMapNames GetMapTimerOptionKey(ExpandedMapNames currentMap) =>
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

public enum VengefulSoulVisibility
{
    All,
    Evil,
    Killer
}
