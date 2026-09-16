using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using DivaniMods.Roles.Neutral.NeutralKilling;

namespace DivaniMods.Options;

public class ThiefOptions : AbstractRoleOptionGroup<ThiefRole>
{
    public override string GroupName => "Thief";

    public ModdedNumberOption MaxStolenModifiers { get; } = new(
        "Max Stolen Modifiers", 5f, 1f, 15f, 1f, MiraNumberSuffixes.None);

    public ModdedNumberOption KillCooldown { get; } = new(
        "Kill Cooldown", 25f, 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds, "0.0");

    public ModdedNumberOption PickpocketCooldown { get; } = new(
        "Pickpocket Cooldown", 25f, 10f, 60f, 5f, MiraNumberSuffixes.Seconds);

    public ModdedNumberOption PickpocketDuration { get; } = new(
        "Pickpocket Duration", 3f, 1f, 10f, 0.5f, MiraNumberSuffixes.Seconds);

    [ModdedToggleOption("Stealing Lover Breaks Their Heart")]
    public bool StealingLoverHeartbreaksVictim { get; set; } = true;

    public ModdedToggleOption CanVent { get; } = new("Thief Can Vent", true);

    public ModdedToggleOption CanStealAllianceModifiers { get; } = new("Can Steal Alliance Modifiers", false);
}