using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using DivaniMods.Modifiers;

namespace DivaniMods.Options;

public class ShuffleOptions : AbstractOptionGroup<ShuffleModifier>
{
    public override string GroupName => "Shuffle";

    [ModdedNumberOption("Shuffle Amount", 0, 5, 1)]
    public float ShuffleAmount { get; set; } = 1;
    
    public ModdedNumberOption ShuffleChance { get; } = new("Shuffle Chance", 50f, 0, 100f, 10f, MiraNumberSuffixes.Percent)
    {
        Visible = () => OptionGroupSingleton<ShuffleOptions>.Instance.ShuffleAmount > 0
    };

    public ModdedNumberOption ShuffleUses { get; } = new("Shuffle Uses", 1f, 0, 5f, 1f, MiraNumberSuffixes.None)
    {
        Visible = () => OptionGroupSingleton<ShuffleOptions>.Instance.ShuffleAmount > 0
    };

    public ModdedNumberOption ShuffleCooldown { get; } = new("Shuffle Cooldown", 30f, 10f, 60f, 5f, MiraNumberSuffixes.Seconds)
    {
        Visible = () => OptionGroupSingleton<ShuffleOptions>.Instance.ShuffleAmount > 0
    };
    
    public ModdedToggleOption ShuffleDeadBodiesOption { get; } = new("Shuffle Dead Bodies", false)
    {
        Visible = () => OptionGroupSingleton<ShuffleOptions>.Instance.ShuffleAmount > 0
    };
    
    public bool ShuffleDeadBodies => ShuffleDeadBodiesOption.Value;
}
