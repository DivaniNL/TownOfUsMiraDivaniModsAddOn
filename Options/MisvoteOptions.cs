using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using DivaniMods.Modifiers;

namespace DivaniMods.Options;

public class MisvoteOptions : AbstractOptionGroup<MisvoteModifier>
{
    public override string GroupName => "Misvote";

    [ModdedNumberOption("Misvote Amount", 0, 5, 1)]
    public float MisvoteAmount { get; set; } = 1;

    public ModdedNumberOption MisvoteChance { get; } = new("Misvote Chance", 50f, 0, 100f, 10f, MiraNumberSuffixes.Percent)
    {
        Visible = () => OptionGroupSingleton<MisvoteOptions>.Instance.MisvoteAmount > 0
    };
}
