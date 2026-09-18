using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using DivaniMods.Roles.Crewmate.CrewmateSupport;

namespace DivaniMods.Options;

public class LocatorOptions : AbstractRoleOptionGroup<LocatorRole>
{
    public override string GroupName => MiraLocaleManager.Get("DivaniMods.Role.Locator", "Locator");

    public ModdedNumberOption AbilityUses { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.Locator.AbilityUses"), 5f, 1f, 10f, 1f, MiraNumberSuffixes.None);

    public ModdedNumberOption MarksPerRound { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.Locator.MarksPerRound"), 1f, 1f, 5f, 1f, MiraNumberSuffixes.None);

    public ModdedToggleOption EarnMoreUses { get; } = new(MiraLocaleManager.Get("DivaniMods.Options.Locator.EarnMoreUses"), false);

    public ModdedNumberOption TasksPerUse { get; } =
        new(MiraLocaleManager.Get("DivaniMods.Options.Locator.TasksPerUse"), 3f, 1f, 5f, 1f, MiraNumberSuffixes.None)
        {
            Visible = () => OptionGroupSingleton<LocatorOptions>.Instance.EarnMoreUses
        };

    public ModdedToggleOption TargetKnows { get; } = new(MiraLocaleManager.Get("DivaniMods.Options.Locator.TargetKnows"), false);
}
