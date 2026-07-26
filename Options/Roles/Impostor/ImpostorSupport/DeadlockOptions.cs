using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using DivaniMods.Roles.Impostor.ImpostorSupport;

namespace DivaniMods.Options;

public class DeadlockOptions : AbstractRoleOptionGroup<DeadlockRole>
{
    public override string GroupName => "Deadlock";

    public ModdedNumberOption LockdownDuration { get; } = new(
        "Lockdown Duration", 10f, 5f, 30f, 5f, MiraNumberSuffixes.Seconds);

    public ModdedNumberOption LockdownCooldown { get; } = new(
        "Lockdown Cooldown", 45f, 20f, 120f, 5f, MiraNumberSuffixes.Seconds);

    public ModdedNumberOption InitialCharges { get; } = new(
        "Initial Charges", 1f, 0f, 5f, 1f, MiraNumberSuffixes.None);

    public ModdedNumberOption ChargesPerKill { get; } = new(
        "Charges Per Kill", 1f, 0f, 3f, 1f, MiraNumberSuffixes.None);
}
