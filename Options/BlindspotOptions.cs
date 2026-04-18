using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using DivaniMods.Modifiers;

namespace DivaniMods.Options;

public class BlindspotOptions : AbstractOptionGroup<BlindspotModifier>
{
    public override string GroupName => "Blindspot";

    [ModdedNumberOption("Blindspot Amount", 0, 5, 1)]
    public float BlindspotAmount { get; set; } = 1;
    
    public ModdedNumberOption BlindspotChance { get; } = new("Blindspot Chance", 50f, 0, 100f, 10f, MiraNumberSuffixes.Percent)
    {
        Visible = () => OptionGroupSingleton<BlindspotOptions>.Instance.BlindspotAmount > 0
    };
}
