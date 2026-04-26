using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using DivaniMods.Modifiers;

namespace DivaniMods.Options;

public class BearTrapOptions : AbstractOptionGroup<BearTrapModifier>
{
    public override string GroupName => "Bear Trap";

    [ModdedNumberOption("Bear Trap Amount", 0, 5, 1)]
    public float BearTrapAmount { get; set; } = 0;

    public ModdedNumberOption BearTrapChance { get; } = new("Bear Trap Chance", 50f, 0, 100f, 10f, MiraNumberSuffixes.Percent)
    {
        Visible = () => OptionGroupSingleton<BearTrapOptions>.Instance.BearTrapAmount > 0
    };

    public ModdedNumberOption FreezeDuration { get; } = new("Bear Trap Freeze Duration", 4f, 2f, 10f, 1f, MiraNumberSuffixes.Seconds)
    {
        Visible = () => OptionGroupSingleton<BearTrapOptions>.Instance.BearTrapAmount > 0
    };
}
