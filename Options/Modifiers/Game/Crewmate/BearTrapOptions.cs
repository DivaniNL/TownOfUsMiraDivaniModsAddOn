using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using DivaniMods.Modifiers.Game.Crewmate;
using TownOfUs.Options;
using UnityEngine;

namespace DivaniMods.Options;

public class BearTrapOptions : AbstractTouModifierOptionGroup<BearTrapModifier>
{
    public override Func<bool> GroupVisible => () => RoleOptions.IsClassicRoleAssignment;
    public override string GroupName => MiraLocaleManager.Get("DivaniMods.Modifier.BearTrap", "Bear Trap");
    public override Color GroupColor => BearTrapModifier.BearTrapColor;
    public override uint GroupPriority => 24;

    public ModdedNumberOption FreezeDuration { get; } =
        new(MiraLocaleManager.Get("DivaniMods.Options.BearTrap.FreezeDuration"), 4f, 2f, 10f, 1f, MiraNumberSuffixes.Seconds);
}
