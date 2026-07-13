using System;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using DivaniMods.Modifiers.Game.Crewmate;
using TownOfUs.Options;
using UnityEngine;

namespace DivaniMods.Options;

public sealed class BountyOptions : AbstractOptionGroup<BountyModifier>
{
    public override Func<bool> GroupVisible => () => OptionsGroupSingleton<RoleOptions>.Instance.IsClassicRoleAssignment;
    
    public override string GroupName => "Bounty";

    public override Color GroupColor => BountyColor;

    public override uint GroupPriority => 26;

    public ModdedNumberOption DecreasePerTask  { get; } = new(
        "cooldown decrease per task (Killer)", 2.5f, 0.5f, 15f, 0.5f, MiraNumberSuffixes.Seconds);
}
