using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using DivaniMods.Modifiers.Game.Alliance;
using TownOfUs.Options;
using UnityEngine;

namespace DivaniMods.Options;

public sealed class BetrayerOptions : AbstractTouModifierOptionGroup<BetrayerModifier>
{
    public override string GroupName => MiraLocaleManager.Get("DivaniMods.Modifier.Betrayer", "Betrayer");
    public override Func<bool> GroupVisible => () => RoleOptions.IsClassicRoleAssignment;
    public override Color GroupColor => BetrayerModifier.BetrayerColor;
    public override uint GroupPriority => 13;

    public ModdedToggleOption CanSabotage { get; } = new(MiraLocaleManager.Get("DivaniMods.Options.Betrayer.CanSabotage"), true);

    public ModdedToggleOption HasImpostorVision { get; } = new(MiraLocaleManager.Get("DivaniMods.Options.Betrayer.HasImpostorVision"), true);

    public ModdedNumberOption KillCooldown { get; } =
        new(MiraLocaleManager.Get("DivaniMods.Options.Betrayer.KillCooldown"), 17f, 0f, 60f, 1f, MiraNumberSuffixes.Seconds);

    public ModdedNumberOption RevealAtPlayersLeftDuoImp { get; } =
        new(MiraLocaleManager.Get("DivaniMods.Options.Betrayer.RevealAtPlayersLeftDuoImp"), 5f, 3f, 15f, 1f, MiraNumberSuffixes.None);

    public ModdedNumberOption RevealAtPlayersLeftMultiImp { get; } =
        new(MiraLocaleManager.Get("DivaniMods.Options.Betrayer.RevealAtPlayersLeftMultiImp"), 8f, 3f, 15f, 1f, MiraNumberSuffixes.None);
}
