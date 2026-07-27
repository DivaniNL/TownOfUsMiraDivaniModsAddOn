using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using DivaniMods.Modifiers.Game.Alliance;
using TownOfUs.Options;
using UnityEngine;

namespace DivaniMods.Options;

public sealed class BetrayerOptions : AbstractTouModifierOptionGroup<BetrayerModifier>
{
    public override string GroupName => "Betrayer";
    public override Func<bool> GroupVisible => () => OptionGroupSingleton<RoleOptions>.Instance.IsClassicRoleAssignment;
    public override Color GroupColor => BetrayerModifier.BetrayerColor;
    public override uint GroupPriority => 13;

    public ModdedToggleOption CanSabotage { get; } = new("Can Sabotage", true);

    public ModdedToggleOption HasImpostorVision { get; } = new("Has Impostor Vision", true);

    public ModdedNumberOption KillCooldown { get; } =
        new("Kill Cooldown", 17f, 0f, 60f, 1f, MiraNumberSuffixes.Seconds);

    public ModdedNumberOption RevealAtPlayersLeftDuoImp { get; } =
        new("Reveal At Players Left (2 Imp-Aligned)", 5f, 3f, 15f, 1f, MiraNumberSuffixes.None);

    public ModdedNumberOption RevealAtPlayersLeftMultiImp { get; } =
        new("Reveal At Players Left (3+ Imp-Aligned)", 8f, 3f, 15f, 1f, MiraNumberSuffixes.None);
}
