using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using DivaniMods.Roles.Crewmate.CrewmateSupport;

namespace DivaniMods.Options;

public class ClockstopperOptions : AbstractRoleOptionGroup<ClockstopperRole>
{
    public override string GroupName => MiraLocaleManager.Get("DivaniMods.Role.Clockstopper", "Clockstopper");

    public ModdedNumberOption TasksPerReset { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.Clockstopper.TasksPerReset"), 2f, 1f, 5f, 1f, MiraNumberSuffixes.None);

    [ModdedToggleOption("DivaniMods.Options.Clockstopper.ResetNeutralBenign")]
    public bool ResetNeutralBenign { get; set; } = false;

    [ModdedToggleOption("DivaniMods.Options.Clockstopper.ResetNeutralEvil")]
    public bool ResetNeutralEvil { get; set; } = true;

    [ModdedToggleOption("DivaniMods.Options.Clockstopper.ResetNeutralKilling")]
    public bool ResetNeutralKilling { get; set; } = true;

    [ModdedToggleOption("DivaniMods.Options.Clockstopper.ResetNeutralOutlier")]
    public bool ResetNeutralOutlier { get; set; } = true;
}
