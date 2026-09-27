using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Translation;
using DivaniMods.Modifiers.Game.Universal;
using TownOfUs.Options;
using UnityEngine;

namespace DivaniMods.Options;

public class MisvoteOptions : AbstractTouModifierOptionGroup<MisvoteModifier>
{
    public override Func<bool> GroupVisible => () => RoleOptions.IsClassicRoleAssignment;
    public override string GroupName => MiraLocaleManager.Get("DivaniMods.Modifier.Misvote", "Misvote");
    public override Color GroupColor => MisvoteModifier.MisvoteColor;
    public override uint GroupPriority => 36;

    public ModdedToggleOption ProsecutorVotesRandom { get; } =
        new(MiraLocaleManager.Get("DivaniMods.Options.Misvote.ProsecutorVotesRandom"), true);
}
