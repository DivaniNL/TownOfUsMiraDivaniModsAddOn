using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using DivaniMods.Modifiers.Game.Universal;
using TownOfUs.Options;
using UnityEngine;

namespace DivaniMods.Options;

public class ArmoredOptions : AbstractTouModifierOptionGroup<ArmoredModifier>
{
    public override Func<bool> GroupVisible => () => RoleOptions.IsClassicRoleAssignment;
    public override string GroupName => MiraLocaleManager.Get("DivaniMods.Modifier.Armored", "Armored");
    public override Color GroupColor => ArmoredModifier.ArmoredColor;
    public override uint GroupPriority => 33;

    public ModdedNumberOption AttacksToSurvive { get; } =
        new(MiraLocaleManager.Get("DivaniMods.Options.Armored.AttacksToSurvive"), 1f, 1f, 5f, 1f, MiraNumberSuffixes.None);
}
