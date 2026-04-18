using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using DivaniMods.Modifiers;

namespace DivaniMods.Options;

public class FragileOptions : AbstractOptionGroup<FragileModifier>
{
    public override string GroupName => "Fragile";

    [ModdedNumberOption("Fragile Amount", 0, 5, 1)]
    public float FragileAmount { get; set; } = 0;
    
    public ModdedNumberOption FragileChance { get; } = new("Fragile Chance", 50f, 0, 100f, 10f, MiraNumberSuffixes.Percent)
    {
        Visible = () => OptionGroupSingleton<FragileOptions>.Instance.FragileAmount > 0
    };
    
    public ModdedNumberOption ChanceToBreak { get; } = new("Chance to Break", 100f, 0, 100f, 5f, MiraNumberSuffixes.Percent)
    {
        Visible = () => OptionGroupSingleton<FragileOptions>.Instance.FragileAmount > 0
    };
}
