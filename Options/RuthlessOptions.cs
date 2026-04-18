using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using DivaniMods.Modifiers;

namespace DivaniMods.Options;

public class RuthlessOptions : AbstractOptionGroup<RuthlessModifier>
{
    public override string GroupName => "Ruthless";

    [ModdedNumberOption("Ruthless Amount", 0, 5, 1)]
    public float RuthlessAmount { get; set; } = 0;
    
    public ModdedNumberOption RuthlessChance { get; } = new("Ruthless Chance", 50f, 0, 100f, 10f, MiraNumberSuffixes.Percent)
    {
        Visible = () => OptionGroupSingleton<RuthlessOptions>.Instance.RuthlessAmount > 0
    };
}
