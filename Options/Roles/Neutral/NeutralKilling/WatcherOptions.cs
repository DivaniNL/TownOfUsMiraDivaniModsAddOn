using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using DivaniMods.Roles.Neutral.NeutralKilling;

namespace DivaniMods.Options;

public class WatcherOptions : AbstractOptionGroup<WatcherRole>
{
    public override string GroupName => "Watcher";

    public ModdedNumberOption GreenLightMinDuration { get; } = new(
        "Green Light Min Duration", 2f, 1f, 15f, 1f, MiraNumberSuffixes.Seconds, "0");

    public ModdedNumberOption GreenLightMaxDuration { get; } = new(
        "Green Light Max Duration", 10f, 1f, 15f, 1f, MiraNumberSuffixes.Seconds, "0");

    public ModdedNumberOption RedLightDuration { get; } = new(
        "Red Light Duration", 4f, 2f, 8f, 0.5f, MiraNumberSuffixes.Seconds, "0.0");

    public ModdedNumberOption RedLightGracePeriod { get; } = new(
        "Red Light Grace Period", 0.7f, 0f, 1.5f, 0.1f, MiraNumberSuffixes.Seconds, "0.0");

    public ModdedNumberOption RedLightGreenLightLoops { get; } = new(
        "Red Light, Green Light Loops", 2f, 1f, 4f, 1f, MiraNumberSuffixes.None);

    public ModdedNumberOption KillCooldown { get; } = new(
        "Kill Cooldown", 25f, 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds, "0.0");

    public ModdedNumberOption WatchCooldown { get; } = new(
        "Watch Cooldown", 25f, 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds, "0.0");

    public ModdedNumberOption InitialWatchCharges { get; } = new(
        "Initial Watch Charges", 0f, 0f, 5f, 1f, MiraNumberSuffixes.None);

    public ModdedNumberOption KillsPerExtraCharge { get; } = new(
        "Kills Required for Extra Watch Charge", 3f, 1f, 5f, 1f, MiraNumberSuffixes.None);
    public ModdedToggleOption InstantKillOnMovement { get; } = new("Punish Movers Instantly", true);

    public ModdedToggleOption LinkWatchKillCooldown { get; } = new("Link Watch & Kill Cooldowns", true);

    public ModdedToggleOption GunshotSoundOnDeath { get; } = new("Enable Gunshot Sound effect on Deaths", true);

    public ModdedToggleOption BlockSabotage { get; } = new("Block Sabotage during Red Light, Green Light", true);

    public ModdedToggleOption DisableEmergencyButton { get; } = new("Disable Emergency Button during Red Light, Green Light", true);

    public ModdedToggleOption GhostwalkersMustFreeze { get; } = new("Ghostwalkers Must Freeze", true);

    public ModdedToggleOption CanVent { get; } = new("Watcher Can Vent", true);
}
