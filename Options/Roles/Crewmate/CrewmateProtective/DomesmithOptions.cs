using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
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
    public override string GroupName => "Domesmith";

    public ModdedNumberOption Charges { get; } = new("Dome Charges", 2f, 1f, 5f, 1f, MiraNumberSuffixes.None);

    public ModdedNumberOption UsesPerTasks { get; } = new("Tasks Required For Additional Dome Use", 3f, 0f, 15f, 1f, "Off", "#",
        MiraNumberSuffixes.None, "0");

    public ModdedNumberOption DomeSize { get; } = new(
        "Dome Size", 0.25f, 0.05f, 1f, 0.05f, MiraNumberSuffixes.Multiplier, "0.00");

    public ModdedNumberOption PlaceDomeCooldown { get; } = new(
        "Place Dome Cooldown", 25f, 5f, 60f, 5f, MiraNumberSuffixes.Seconds);

    public ModdedNumberOption PlaceDomeDuration { get; } = new(
        "Place Dome Duration", 3f, 0f, 10f, 0.5f, MiraNumberSuffixes.Seconds);

    public ModdedNumberOption ActiveSeconds { get; } =
        new("Dome Active Seconds", 10f, 2f, 30f, 2f, MiraNumberSuffixes.Seconds);

    public ModdedToggleOption AllowCrewmateKillsInDome { get; } =
        new("Allow Crewmate Kills In Dome", false);

    public ModdedEnumOption SeenBy { get; } = new(
        "Dome Visible To",
        (int)DomesmithVisibility.Domesmith,
        typeof(DomesmithVisibility));
}
