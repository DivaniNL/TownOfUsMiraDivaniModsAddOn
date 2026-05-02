using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Options;

namespace DivaniMods.Options;

public sealed class UniversalModifierOptions : AbstractOptionGroup
{
    public override string GroupName => "Universal Modifiers";
    public override Func<bool> GroupVisible => () => OptionGroupSingleton<RoleOptions>.Instance.IsClassicRoleAssignment;
    public override bool ShowInModifiersMenu => true;
    public override uint GroupPriority => 1;

    [ModdedNumberOption("Fragile Amount", 0, 5, 1)]
    public float FragileAmount { get; set; } = 0;

    public ModdedNumberOption FragileChance { get; } =
        new("Fragile Chance", 50f, 0, 100f, 10f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<UniversalModifierOptions>.Instance.FragileAmount > 0
        };

    [ModdedNumberOption("Shuffle Amount", 0, 5, 1)]
    public float ShuffleAmount { get; set; } = 1;

    public ModdedNumberOption ShuffleChance { get; } =
        new("Shuffle Chance", 50f, 0, 100f, 10f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<UniversalModifierOptions>.Instance.ShuffleAmount > 0
        };

    [ModdedNumberOption("Misvote Amount", 0, 5, 1)]
    public float MisvoteAmount { get; set; } = 1;

    public ModdedNumberOption MisvoteChance { get; } =
        new("Misvote Chance", 50f, 0, 100f, 10f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<UniversalModifierOptions>.Instance.MisvoteAmount > 0
        };
}
