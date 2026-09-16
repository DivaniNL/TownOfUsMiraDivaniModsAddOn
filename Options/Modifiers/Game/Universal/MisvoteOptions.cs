using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using DivaniMods.Modifiers.Game.Universal;
using TownOfUs.Options;
using UnityEngine;

namespace DivaniMods.Options;

public class MisvoteOptions : AbstractTouModifierOptionGroup<MisvoteModifier>
{
    public override Func<bool> GroupVisible => () => RoleOptions.IsClassicRoleAssignment;
    public override string GroupName => "Misvote";
    public override Color GroupColor => MisvoteModifier.MisvoteColor;
    public override uint GroupPriority => 36;

    public ModdedToggleOption ProsecutorVotesRandom { get; } =
        new("Misvoted Prosecutor Prosecutes Random", true);
}
