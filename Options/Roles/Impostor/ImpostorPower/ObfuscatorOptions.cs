using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using DivaniMods.Roles.Impostor.ImpostorPower;

namespace DivaniMods.Options;

public class ObfuscatorOptions : AbstractRoleOptionGroup<ObfuscatorRole>
{
    public override string GroupName => "Obfuscator";

    public ModdedNumberOption InitialCharges { get; } = new(
        "Initial Charges", 1f, 0f, 5f, 1f, MiraNumberSuffixes.None);

    public ModdedNumberOption KillsPerExtraCharge { get; } = new(
        "Kills Per Extra Charge", 2f, 0f, 10f, 1f, MiraNumberSuffixes.None);
}
