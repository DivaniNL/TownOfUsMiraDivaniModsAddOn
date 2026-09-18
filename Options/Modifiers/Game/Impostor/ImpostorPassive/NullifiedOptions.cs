using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Translation;
using DivaniMods.Modifiers.Game.Impostor.ImpostorPassive;
using UnityEngine;
using TownOfUs.Options;

namespace DivaniMods.Options;

public class NullifiedOptions : AbstractTouModifierOptionGroup<NullifiedModifier>
{
    public override Func<bool> GroupVisible => () => RoleOptions.IsClassicRoleAssignment;
    public override string GroupName => MiraLocaleManager.Get("DivaniMods.Modifier.Nullified", "Nullified");
    public override Color GroupColor => NullifiedModifier.NullifiedColor;
    public override uint GroupPriority => 41;

    [ModdedToggleOption("DivaniMods.Options.Nullified.SilencesCelebrity")]
    public bool SilencesCelebrity { get; set; } = false;
}
