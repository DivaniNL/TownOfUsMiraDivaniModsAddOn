using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
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

    [ModdedNumberOption("Blindspot Amount", 0, 5, 1)]
    public float BlindspotAmount { get; set; } = 1;

    public ModdedNumberOption BlindspotChance { get; } =
        new("Blindspot Chance", 50f, 0, 100f, 10f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<CrewmateModifierOptions>.Instance.BlindspotAmount > 0
        };

    [ModdedNumberOption("Bear Trap Amount", 0, 5, 1)]
    public float BearTrapAmount { get; set; } = 0;

    public ModdedNumberOption BearTrapChance { get; } =
        new("Bear Trap Chance", 50f, 0, 100f, 10f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<CrewmateModifierOptions>.Instance.BearTrapAmount > 0
        };
}
