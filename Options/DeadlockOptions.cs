using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using DivaniMods.Roles;

namespace DivaniMods.Options;

public class DeadlockOptions : AbstractOptionGroup<DeadlockRole>
{
    public override string GroupName => "Deadlock";

    [ModdedNumberOption("Lockdown Duration", 5, 30, 5, MiraNumberSuffixes.Seconds)]
    public float LockdownDuration { get; set; } = 10;
    
    [ModdedNumberOption("Lockdown Cooldown", 20, 120, 5, MiraNumberSuffixes.Seconds)]
    public float LockdownCooldown { get; set; } = 45;
    
    [ModdedNumberOption("Initial Charges", 0, 5, 1)]
    public float InitialCharges { get; set; } = 1;
    
    [ModdedNumberOption("Charges Per Kill", 0, 3, 1)]
    public float ChargesPerKill { get; set; } = 1;
}
