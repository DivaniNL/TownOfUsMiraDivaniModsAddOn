using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using DivaniMods.Modifiers;
using TownOfUs.Options;
using UnityEngine;

namespace DivaniMods.Options;

public class BearTrapOptions : AbstractOptionGroup<BearTrapModifier>
{
    public override Func<bool> GroupVisible => () => OptionGroupSingleton<RoleOptions>.Instance.IsClassicRoleAssignment;
    public override string GroupName => "Bear Trap";
    public override Color GroupColor => BearTrapModifier.BearTrapColor;
    public override uint GroupPriority => 24;

    public ModdedNumberOption FreezeDuration { get; } =
        new("Bear Trap Freeze Duration", 4f, 2f, 10f, 1f, MiraNumberSuffixes.Seconds);
}
