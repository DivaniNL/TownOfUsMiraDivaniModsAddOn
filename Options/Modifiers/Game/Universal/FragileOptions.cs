using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using DivaniMods.Modifiers.Game.Universal;
using TownOfUs.Options;
using UnityEngine;

namespace DivaniMods.Options;

public class FragileOptions : AbstractTouModifierOptionGroup<FragileModifier>
{
    public override Func<bool> GroupVisible => () => RoleOptions.IsClassicRoleAssignment;
    public override string GroupName => MiraLocaleManager.Get("DivaniMods.Modifier.Fragile", "Fragile");
    public override Color GroupColor => FragileModifier.FragileColor;
    public override uint GroupPriority => 34;
    
    public ModdedNumberOption ChanceToBreak { get; } =
        new(MiraLocaleManager.Get("DivaniMods.Options.Fragile.ChanceToBreak"), 100f, 0, 100f, 5f, MiraNumberSuffixes.Percent);
}
