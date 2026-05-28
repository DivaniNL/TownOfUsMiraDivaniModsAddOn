using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Options;
using UnityEngine;

namespace DivaniMods.Options;

public sealed class CrewmateModifierOptions : AbstractOptionGroup
{
    public override string GroupName => "Crewmate Modifiers";
    public override Func<bool> GroupVisible => () => OptionGroupSingleton<RoleOptions>.Instance.IsClassicRoleAssignment;
    public override Color GroupColor => Palette.CrewmateRoleHeaderBlue;
    public override bool ShowInModifiersMenu => true;
    public override uint GroupPriority => 2;

    public ModdedNumberOption BlindspotAmount { get; } = new(
        "Blindspot Amount", 1f, 0f, 5f, 1f, MiraNumberSuffixes.None);

    public ModdedNumberOption BlindspotChance { get; } =
        new("Blindspot Chance", 50f, 0, 100f, 10f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<CrewmateModifierOptions>.Instance.BlindspotAmount.Value > 0
        };

    public ModdedNumberOption BearTrapAmount { get; } = new(
        "Bear Trap Amount", 0f, 0f, 5f, 1f, MiraNumberSuffixes.None);

    public ModdedNumberOption BearTrapChance { get; } =
        new("Bear Trap Chance", 50f, 0, 100f, 10f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<CrewmateModifierOptions>.Instance.BearTrapAmount.Value > 0
        };

    public ModdedNumberOption BloodyAmount { get; } = new(
        "Bloody Amount", 0f, 0f, 5f, 1f, MiraNumberSuffixes.None);

    public ModdedNumberOption BloodyChance { get; } =
        new("Bloody Chance", 50f, 0, 100f, 10f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<CrewmateModifierOptions>.Instance.BloodyAmount.Value > 0
        };

    public ModdedNumberOption SproutAmount { get; } = new(
        "Sprout Amount", 0f, 0f, 5f, 1f, MiraNumberSuffixes.None);

    public ModdedNumberOption SproutChance { get; } =
        new("Sprout Chance", 50f, 0, 100f, 10f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<CrewmateModifierOptions>.Instance.SproutAmount.Value > 0
        };
}
