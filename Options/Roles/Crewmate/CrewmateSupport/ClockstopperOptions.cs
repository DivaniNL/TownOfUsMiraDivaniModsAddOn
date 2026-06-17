using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using DivaniMods.Roles.Crewmate.CrewmateSupport;

namespace DivaniMods.Options;

public class ClockstopperOptions : AbstractOptionGroup<ClockstopperRole>
{
    public override string GroupName => "Clockstopper";

    public ModdedNumberOption TasksPerReset { get; } = new(
        "Tasks Per Cooldown Reset", 2f, 1f, 5f, 1f, MiraNumberSuffixes.None);

    [ModdedToggleOption("Resets Neutral Benign Cooldowns")]
    public bool ResetNeutralBenign { get; set; } = false;

    [ModdedToggleOption("Resets Neutral Evil Cooldowns")]
    public bool ResetNeutralEvil { get; set; } = true;

    [ModdedToggleOption("Resets Neutral Killing Cooldowns")]
    public bool ResetNeutralKilling { get; set; } = true;

    [ModdedToggleOption("Resets Neutral Outlier Cooldowns")]
    public bool ResetNeutralOutlier { get; set; } = true;
}
