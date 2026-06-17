using MiraAPI.GameOptions;
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

    public ModdedNumberOption FragileAmount { get; } = new(
        "Fragile Amount", 0f, 0f, 5f, 1f, MiraNumberSuffixes.None);

    public ModdedNumberOption FragileChance { get; } =
        new("Fragile Chance", 50f, 0, 100f, 10f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<UniversalModifierOptions>.Instance.FragileAmount.Value > 0
        };

    public ModdedNumberOption ShuffleAmount { get; } = new(
        "Shuffle Amount", 1f, 0f, 5f, 1f, MiraNumberSuffixes.None);

    public ModdedNumberOption ShuffleChance { get; } =
        new("Shuffle Chance", 50f, 0, 100f, 10f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<UniversalModifierOptions>.Instance.ShuffleAmount.Value > 0
        };

    public ModdedNumberOption MisvoteAmount { get; } = new(
        "Misvote Amount", 1f, 0f, 5f, 1f, MiraNumberSuffixes.None);

    public ModdedNumberOption MisvoteChance { get; } =
        new("Misvote Chance", 50f, 0, 100f, 10f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<UniversalModifierOptions>.Instance.MisvoteAmount.Value > 0
        };

    public ModdedNumberOption MementoAmount { get; } = new(
        "Memento Amount", 0f, 0f, 5f, 1f, MiraNumberSuffixes.None);

    public ModdedNumberOption MementoChance { get; } =
        new("Memento Chance", 50f, 0, 100f, 10f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<UniversalModifierOptions>.Instance.MementoAmount.Value > 0
        };

    public ModdedNumberOption ArmoredAmount { get; } = new(
        "Armored Amount", 0f, 0f, 5f, 1f, MiraNumberSuffixes.None);

    public ModdedNumberOption ArmoredChance { get; } =
        new("Armored Chance", 50f, 0, 100f, 10f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<UniversalModifierOptions>.Instance.ArmoredAmount.Value > 0
        };

    public ModdedNumberOption UavAmount { get; } = new(
        "UAV Amount", 0f, 0f, 5f, 1f, MiraNumberSuffixes.None);

    public ModdedNumberOption UavChance { get; } =
        new("UAV Chance", 50f, 0, 100f, 10f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<UniversalModifierOptions>.Instance.UavAmount.Value > 0
        };
}
