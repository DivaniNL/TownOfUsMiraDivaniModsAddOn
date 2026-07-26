using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Options;
using UnityEngine;

namespace DivaniMods.Options;

public sealed class ImpostorModifierOptions : AbstractOptionGroup
{
    public override string GroupName => "Impostor Modifiers";
    public override Func<bool> GroupVisible => () => OptionGroupSingleton<RoleOptions>.Instance.IsClassicRoleAssignment;
    public override Color GroupColor => Palette.ImpostorRoleHeaderRed;
    public override MenuCategory ParentMenu => MenuCategory.Modifiers;
    public override uint GroupPriority => 3;

    public ModdedNumberOption NullifiedAmount { get; } = new(
        "Nullified Amount", 0f, 0f, 5f, 1f, MiraNumberSuffixes.None);

    public ModdedNumberOption NullifiedChance { get; } =
        new("Nullified Chance", 50f, 0, 100f, 10f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<ImpostorModifierOptions>.Instance.NullifiedAmount.Value > 0
        };

    public ModdedNumberOption RuthlessAmount { get; } = new(
        "Ruthless Amount", 0f, 0f, 5f, 1f, MiraNumberSuffixes.None);

    public ModdedNumberOption RuthlessChance { get; } =
        new("Ruthless Chance", 50f, 0, 100f, 10f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<ImpostorModifierOptions>.Instance.RuthlessAmount.Value > 0
        };
}
