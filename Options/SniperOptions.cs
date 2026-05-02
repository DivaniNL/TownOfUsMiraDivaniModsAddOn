using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using DivaniMods.Modifiers;
using TownOfUs.Options;
using UnityEngine;

namespace DivaniMods.Options;

public class SniperOptions : AbstractOptionGroup<SniperModifier>
{
    public override Func<bool> GroupVisible => () => OptionGroupSingleton<RoleOptions>.Instance.IsClassicRoleAssignment;
    public override string GroupName => "Sniper";
    public override Color GroupColor => SniperModifier.SniperColor;
    public override uint GroupPriority => 50;

    public ModdedNumberOption KillDistanceMultiplier { get; } =
        new("Kill Distance Multiplier", 1.5f, 1.1f, 2.0f, 0.1f, MiraNumberSuffixes.Multiplier, "0.0");
}
