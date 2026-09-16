using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using DivaniMods.Modifiers.Game.Universal;
using TownOfUs.Options;
using UnityEngine;

namespace DivaniMods.Options;

public enum MementoRevealMode
{
    Role,
    Alignment,
    Faction,
}

public class MementoOptions : AbstractTouModifierOptionGroup<MementoModifier>
{
    public override Func<bool> GroupVisible => () => RoleOptions.IsClassicRoleAssignment;
    public override string GroupName => "Memento";
    public override Color GroupColor => MementoModifier.MementoColor;
    public override uint GroupPriority => 35;

    public ModdedEnumOption RevealMode { get; } =
        new("Reveal Mode", (int)MementoRevealMode.Role, typeof(MementoRevealMode));

    public ModdedToggleOption ShowHeldModifiers { get; } =
        new("Show Held Modifiers", true);

    public ModdedToggleOption ShowIfEjected { get; } =
        new("Reveal If Ejected", true);

    public ModdedToggleOption PreventBaitPairing { get; } =
        new("Prevent Pairing With Bait", false);
}
