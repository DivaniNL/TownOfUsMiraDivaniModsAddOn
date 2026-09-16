using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using DivaniMods.Modifiers.Game.Impostor.ImpostorPassive;
using UnityEngine;
using TownOfUs.Options;

namespace DivaniMods.Options;

public class NullifiedOptions : AbstractTouModifierOptionGroup<NullifiedModifier>
{
    public override Func<bool> GroupVisible => () => RoleOptions.IsClassicRoleAssignment;
    public override string GroupName => "Nullified";
    public override Color GroupColor => NullifiedModifier.NullifiedColor;
    public override uint GroupPriority => 41;

    [ModdedToggleOption("Silences Celebrity")]
    public bool SilencesCelebrity { get; set; } = false;
}
