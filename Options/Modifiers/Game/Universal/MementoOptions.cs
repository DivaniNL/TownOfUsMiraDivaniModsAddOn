using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Translation;
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
    public override string GroupName => MiraLocaleManager.Get("DivaniMods.Modifier.Memento", "Memento");
    public override Color GroupColor => MementoModifier.MementoColor;
    public override uint GroupPriority => 35;

    public ModdedEnumOption RevealMode { get; } =
        new(MiraLocaleManager.Get("DivaniMods.Options.Memento.RevealMode"), (int)MementoRevealMode.Role, typeof(MementoRevealMode));

    public ModdedToggleOption ShowHeldModifiers { get; } =
        new(MiraLocaleManager.Get("DivaniMods.Options.Memento.ShowHeldModifiers"), true);

    public ModdedToggleOption ShowIfEjected { get; } =
        new(MiraLocaleManager.Get("DivaniMods.Options.Memento.ShowIfEjected"), true);

    public ModdedToggleOption PreventBaitPairing { get; } =
        new(MiraLocaleManager.Get("DivaniMods.Options.Memento.PreventBaitPairing"), false);
}
