using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Options;
using UnityEngine;

namespace DivaniMods.Options;

public sealed class NeutralModifierOptions : AbstractOptionGroup
{
    public override string GroupName => "Neutral Modifiers";
    public override Func<bool> GroupVisible => () => OptionGroupSingleton<RoleOptions>.Instance.IsClassicRoleAssignment;
    public override Color GroupColor => Color.gray;
    public override bool ShowInModifiersMenu => true;
    public override uint GroupPriority => 4;

    [ModdedNumberOption("Sniper Amount", 0, 5, 1)]
    public float SniperAmount { get; set; } = 0;

    public ModdedNumberOption SniperChance { get; } =
        new("Sniper Chance", 50f, 0, 100f, 10f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<NeutralModifierOptions>.Instance.SniperAmount > 0
        };
}
