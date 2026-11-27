using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Translation;
using MiraAPI.Utilities;

namespace DivaniMods.Options;

public sealed class ClockstopperAbilityOptions : AbstractOptionGroup
{
    public override string GroupName =>
        MiraLocaleManager.Get("DivaniMods.Role.ClockstopperAbility.Name", "Clockstopper Ability");

    public ModdedToggleOption Enabled { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.Clockstopper.Enabled"), true);

    public ModdedToggleOption UnlimitedUses { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.Clockstopper.UnlimitedUses"), false);

    public ModdedNumberOption MaxCharges { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.Clockstopper.MaxCharges"), 3f, 1f, 10f, 1f, MiraNumberSuffixes.None);

    public ModdedNumberOption StartingCharges { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.Clockstopper.StartingCharges"), 1f, 0f, 10f, 1f, MiraNumberSuffixes.None);

    public ModdedToggleOption GainChargesFromTasks { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.Clockstopper.GainChargesFromTasks"), true);

    public ModdedNumberOption TasksPerCharge { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.Clockstopper.TasksPerCharge"), 3f, 1f, 10f, 1f, MiraNumberSuffixes.None)
    {
        Visible = () => OptionGroupSingleton<ClockstopperAbilityOptions>.Instance.GainChargesFromTasks
    };

    public ModdedToggleOption GainChargesOverTime { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.Clockstopper.GainChargesOverTime"), false);

    public ModdedNumberOption TimePerCharge { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.Clockstopper.TimePerCharge"), 90f, 15f, 300f, 15f, MiraNumberSuffixes.Seconds)
    {
        Visible = () => OptionGroupSingleton<ClockstopperAbilityOptions>.Instance.GainChargesOverTime
    };

    public ModdedToggleOption GainChargeOnRewind { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.Clockstopper.GainChargeOnRewind"), false);

    public ModdedToggleOption ResetNeutralBenign { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.Clockstopper.ResetNeutralBenign"), false);

    public ModdedToggleOption ResetNeutralEvil { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.Clockstopper.ResetNeutralEvil"), true);

    public ModdedToggleOption ResetNeutralKilling { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.Clockstopper.ResetNeutralKilling"), true);

    public ModdedToggleOption ResetNeutralOutlier { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.Clockstopper.ResetNeutralOutlier"), true);
}
