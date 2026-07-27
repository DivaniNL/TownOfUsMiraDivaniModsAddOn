using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using DivaniMods.Modifiers.Game.Universal;
using TownOfUs.Options;
using UnityEngine;

namespace DivaniMods.Options;

public class FragileOptions : AbstractTouModifierOptionGroup<FragileModifier>
{
    public override Func<bool> GroupVisible => () => OptionGroupSingleton<RoleOptions>.Instance.IsClassicRoleAssignment;
    public override string GroupName => "Fragile";
    public override Color GroupColor => FragileModifier.FragileColor;
    public override uint GroupPriority => 34;
    
    public ModdedNumberOption ChanceToBreak { get; } =
        new("Chance to Break", 100f, 0, 100f, 5f, MiraNumberSuffixes.Percent);
}
