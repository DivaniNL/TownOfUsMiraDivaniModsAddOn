using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using DivaniMods.Roles.Impostor.ImpostorSupport;

namespace DivaniMods.Options;

public class DeadlockOptions : AbstractRoleOptionGroup<DeadlockRole>
{
    public override string GroupName => MiraLocaleManager.Get("DivaniMods.Role.Deadlock", "Deadlock");

    public ModdedNumberOption LockdownDuration { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.Deadlock.LockdownDuration"), 10f, 5f, 30f, 5f, MiraNumberSuffixes.Seconds);

    public ModdedNumberOption LockdownCooldown { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.Deadlock.LockdownCooldown"), 45f, 20f, 120f, 5f, MiraNumberSuffixes.Seconds);

    public ModdedNumberOption InitialCharges { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.Deadlock.InitialCharges"), 1f, 0f, 5f, 1f, MiraNumberSuffixes.None);

    public ModdedNumberOption ChargesPerKill { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.Deadlock.ChargesPerKill"), 1f, 0f, 3f, 1f, MiraNumberSuffixes.None);
}
