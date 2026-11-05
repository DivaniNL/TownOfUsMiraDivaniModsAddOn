using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using DivaniMods.Roles.Impostor.ImpostorPower;

namespace DivaniMods.Options;

public class ObfuscatorOptions : AbstractRoleOptionGroup<ObfuscatorRole>
{
    public override string GroupName => MiraLocaleManager.Get("DivaniMods.Role.Obfuscator", "Obfuscator");

    public ModdedNumberOption InitialCharges { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.Obfuscator.InitialCharges"), 1f, 0f, 5f, 1f, MiraNumberSuffixes.None);

    public ModdedNumberOption KillsPerExtraCharge { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.Obfuscator.KillsPerExtraCharge"), 2f, 0f, 10f, 1f, MiraNumberSuffixes.None);
}
