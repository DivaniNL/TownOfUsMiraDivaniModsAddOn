using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using DivaniMods.Roles.Crewmate.CrewmateSupport;

namespace DivaniMods.Options;

public class LocatorOptions : AbstractOptionGroup<LocatorRole>
{
    public override string GroupName => "Locator";

    public ModdedNumberOption AbilityUses { get; } = new(
        "Mark Uses", 5f, 1f, 10f, 1f, MiraNumberSuffixes.None);

    public ModdedNumberOption MarksPerRound { get; } = new(
        "Marks Per Round", 1f, 1f, 5f, 1f, MiraNumberSuffixes.None);

    public ModdedToggleOption EarnMoreUses { get; } = new("Earn More Uses After Tasks", false);

    public ModdedNumberOption TasksPerUse { get; } =
        new("Tasks Required For Additional Use", 3f, 1f, 5f, 1f, MiraNumberSuffixes.None)
        {
            Visible = () => OptionGroupSingleton<LocatorOptions>.Instance.EarnMoreUses
        };

    public ModdedToggleOption TargetKnows { get; } = new("Target Knows About Being Marked", false);
}
