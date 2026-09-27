using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using DivaniMods.Roles.Crewmate.CrewmateProtective;

namespace DivaniMods.Options;

public enum DomesmithVisibility
{
    Domesmith,
    NonImpostor,
    Crewmates,
    Everyone,
}

public class DomesmithOptions : AbstractRoleOptionGroup<DomesmithRole>
{
    public override string GroupName => MiraLocaleManager.Get("DivaniMods.Role.Domesmith", "Domesmith");

    public ModdedNumberOption Charges { get; } = new(MiraLocaleManager.Get("DivaniMods.Options.Domesmith.Charges"), 2f, 1f, 5f, 1f, MiraNumberSuffixes.None);

    public ModdedNumberOption UsesPerTasks { get; } = new(MiraLocaleManager.Get("DivaniMods.Options.Domesmith.UsesPerTasks"), 3f, 0f, 15f, 1f, "Off", "#",
        MiraNumberSuffixes.None, "0");

    public ModdedNumberOption DomeSize { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.Domesmith.DomeSize"), 0.25f, 0.05f, 1f, 0.05f, MiraNumberSuffixes.Multiplier, "0.00");

    public ModdedNumberOption PlaceDomeCooldown { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.Domesmith.PlaceDomeCooldown"), 25f, 5f, 60f, 5f, MiraNumberSuffixes.Seconds);

    public ModdedNumberOption PlaceDomeDuration { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.Domesmith.PlaceDomeDuration"), 3f, 0f, 10f, 0.5f, MiraNumberSuffixes.Seconds);

    public ModdedNumberOption ActiveSeconds { get; } =
        new(MiraLocaleManager.Get("DivaniMods.Options.Domesmith.ActiveSeconds"), 10f, 2f, 30f, 2f, MiraNumberSuffixes.Seconds);

    public ModdedToggleOption AllowCrewmateKillsInDome { get; } =
        new(MiraLocaleManager.Get("DivaniMods.Options.Domesmith.AllowCrewmateKillsInDome"), false);

    public ModdedEnumOption SeenBy { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.Domesmith.SeenBy"),
        (int)DomesmithVisibility.Domesmith,
        typeof(DomesmithVisibility));
}
