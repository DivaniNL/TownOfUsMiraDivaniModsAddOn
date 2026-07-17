using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using DivaniMods.Modifiers.Game.Impostor.ImpostorPassive;
using UnityEngine;
using TownOfUs.Options;

namespace DivaniMods.Options;

public class RuthlessOptions : AbstractOptionGroup<RuthlessModifier>
{
    public override Func<bool> GroupVisible => () => OptionGroupSingleton<RoleOptions>.Instance.IsClassicRoleAssignment;
    public override string GroupName => "Ruthless";
    public override Color GroupColor => RuthlessModifier.RuthlessColor;
    public override uint GroupPriority => 42;
    [ModdedToggleOption("Bypass First Death Shield")]
    public bool BypassFirstDeathShield { get; set; } = true;
}
