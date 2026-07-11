using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Options;
using UnityEngine;

namespace DivaniMods.Options;

public sealed class AllianceModifierOptions : AbstractOptionGroup
{
    public override string GroupName => "Alliance Modifiers";
    public override Func<bool> GroupVisible => () => OptionGroupSingleton<RoleOptions>.Instance.IsClassicRoleAssignment;
    public override Color GroupColor => Color.white;
    public override bool ShowInModifiersMenu => true;
    public override uint GroupPriority => 0;

    public ModdedNumberOption BetrayerAmount { get; } = new(
        "Betrayer Amount", 1f, 0f, 3f, 1f, MiraNumberSuffixes.None);

    public ModdedNumberOption BetrayerChance { get; } =
        new("Betrayer Chance", 0f, 0f, 100f, 10f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<AllianceModifierOptions>.Instance.BetrayerAmount.Value > 0
        };
}
