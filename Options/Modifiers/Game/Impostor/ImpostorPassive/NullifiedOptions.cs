using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using DivaniMods.Modifiers.Game.Impostor.ImpostorPassive;
using UnityEngine;
using TownOfUs.Options;

namespace DivaniMods.Options;

public class NullifiedOptions : AbstractOptionGroup<NullifiedModifier>
{
    public override Func<bool> GroupVisible => () => OptionGroupSingleton<RoleOptions>.Instance.IsClassicRoleAssignment;
    public override string GroupName => "Nullified";
    public override Color GroupColor => NullifiedModifier.NullifiedColor;
    public override uint GroupPriority => 41;

    [ModdedToggleOption("Silences Celebrity")]
    public bool SilencesCelebrity { get; set; } = false;
}
