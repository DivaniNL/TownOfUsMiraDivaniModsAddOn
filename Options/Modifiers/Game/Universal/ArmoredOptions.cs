using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using DivaniMods.Modifiers.Game.Universal;
using TownOfUs.Options;
using UnityEngine;

namespace DivaniMods.Options;

public class ArmoredOptions : AbstractTouModifierOptionGroup<ArmoredModifier>
{
    public override Func<bool> GroupVisible => () => OptionGroupSingleton<RoleOptions>.Instance.IsClassicRoleAssignment;
    public override string GroupName => "Armored";
    public override Color GroupColor => ArmoredModifier.ArmoredColor;
    public override uint GroupPriority => 33;

    public ModdedNumberOption AttacksToSurvive { get; } =
        new("Attacks to Survive", 1f, 1f, 5f, 1f, MiraNumberSuffixes.None);
}
