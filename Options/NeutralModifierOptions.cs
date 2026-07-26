using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs;
using TownOfUs.Options;
using UnityEngine;

namespace DivaniMods.Options;

public sealed class NeutralModifierOptions : AbstractOptionGroup
{
    public override string GroupName => "Neutral Modifiers";
    public override Func<bool> GroupVisible => () => OptionGroupSingleton<RoleOptions>.Instance.IsClassicRoleAssignment;
    public override Color GroupColor => TownOfUsColors.Neutral;
    public override MenuCategory ParentMenu => MenuCategory.Modifiers;
    public override uint GroupPriority => 4;

    public ModdedNumberOption SniperAmount { get; } = new(
        "Sniper Amount", 0f, 0f, 5f, 1f, MiraNumberSuffixes.None);

    public ModdedNumberOption SniperChance { get; } =
        new("Sniper Chance", 50f, 0, 100f, 10f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<NeutralModifierOptions>.Instance.SniperAmount.Value > 0
        };
}
