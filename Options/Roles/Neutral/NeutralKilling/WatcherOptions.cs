using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using DivaniMods.Roles.Neutral.NeutralKilling;

namespace DivaniMods.Options;

public class WatcherOptions : AbstractRoleOptionGroup<WatcherRole>
{
    public override string GroupName => MiraLocaleManager.Get("DivaniMods.Role.Watcher", "Watcher");

    public ModdedNumberOption GreenLightMinDuration { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.Watcher.GreenLightMinDuration"), 2f, 1f, 15f, 1f, MiraNumberSuffixes.Seconds, "0");

    public ModdedNumberOption GreenLightMaxDuration { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.Watcher.GreenLightMaxDuration"), 10f, 1f, 15f, 1f, MiraNumberSuffixes.Seconds, "0");

    public ModdedNumberOption RedLightDuration { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.Watcher.RedLightDuration"), 4f, 2f, 8f, 0.5f, MiraNumberSuffixes.Seconds, "0.0");

    public ModdedNumberOption RedLightGracePeriod { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.Watcher.RedLightGracePeriod"), 0.4f, 0f, 1.5f, 0.1f, MiraNumberSuffixes.Seconds, "0.0");

    public ModdedNumberOption RedLightGreenLightLoops { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.Watcher.RedLightGreenLightLoops"), 2f, 1f, 4f, 1f, MiraNumberSuffixes.None);

    public ModdedNumberOption KillCooldown { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.Watcher.KillCooldown"), 25f, 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds, "0.0");

    public ModdedNumberOption WatchCooldown { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.Watcher.WatchCooldown"), 25f, 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds, "0.0");

    public ModdedNumberOption InitialWatchCharges { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.Watcher.InitialWatchCharges"), 0f, 0f, 5f, 1f, MiraNumberSuffixes.None);

    public ModdedNumberOption KillsPerExtraCharge { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.Watcher.KillsPerExtraCharge"), 3f, 1f, 5f, 1f, MiraNumberSuffixes.None);
    public ModdedToggleOption InstantKillOnMovement { get; } = new(MiraLocaleManager.Get("DivaniMods.Options.Watcher.InstantKillOnMovement"), true);

    public ModdedToggleOption LinkWatchKillCooldown { get; } = new(MiraLocaleManager.Get("DivaniMods.Options.Watcher.LinkWatchKillCooldown"), true);

    public ModdedToggleOption KillsDuringLightsCount { get; } =
        new(MiraLocaleManager.Get("DivaniMods.Options.Watcher.KillsDuringLightsCount"), false)
        {
            Visible = () => !OptionGroupSingleton<WatcherOptions>.Instance.LinkWatchKillCooldown.Value
        };

    public ModdedToggleOption GunshotSoundOnDeath { get; } = new(MiraLocaleManager.Get("DivaniMods.Options.Watcher.GunshotSoundOnDeath"), true);

    public ModdedToggleOption BlockSabotage { get; } = new(MiraLocaleManager.Get("DivaniMods.Options.Watcher.BlockSabotage"), true);

    public ModdedToggleOption DisableEmergencyButton { get; } = new(MiraLocaleManager.Get("DivaniMods.Options.Watcher.DisableEmergencyButton"), true);

    public ModdedToggleOption GhostwalkersMustFreeze { get; } = new(MiraLocaleManager.Get("DivaniMods.Options.Watcher.GhostwalkersMustFreeze"), true);

    public ModdedToggleOption CanVent { get; } = new(MiraLocaleManager.Get("DivaniMods.Options.Watcher.CanVent"), true);
}
