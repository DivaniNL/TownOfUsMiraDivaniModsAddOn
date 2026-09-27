using System;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using DivaniMods.Modifiers.Game.Crewmate;
using TownOfUs.Options;
using UnityEngine;

namespace DivaniMods.Options;

public sealed class BloodyOptions : AbstractTouModifierOptionGroup<BloodyModifier>
{
    public override Func<bool> GroupVisible => () => RoleOptions.IsClassicRoleAssignment;

    public override string GroupName => MiraLocaleManager.Get("DivaniMods.Modifier.Bloody", "Bloody");

    public override Color GroupColor => BloodyModifier.ModifierUiColor;

    public override uint GroupPriority => 26;

    public ModdedEnumOption FootprintMode { get; set; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.Bloody.FootprintMode"),
        (int)BloodyPrintMode.Distance,
        typeof(BloodyPrintMode));

    public ModdedNumberOption FootprintIntervalDistance { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.Bloody.FootprintIntervalDistance"),
        0.5f, 0.25f, 3f, 0.5f, MiraNumberSuffixes.None)
    {
        Visible = () =>
            (BloodyPrintMode)OptionGroupSingleton<BloodyOptions>.Instance.FootprintMode.Value is BloodyPrintMode.Distance
    };

    public ModdedNumberOption FootprintIntervalTime { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.Bloody.FootprintIntervalTime"),
        4f, 0.5f, 6f, 0.5f, MiraNumberSuffixes.Seconds)
    {
        Visible = () =>
            (BloodyPrintMode)OptionGroupSingleton<BloodyOptions>.Instance.FootprintMode.Value is BloodyPrintMode.Time
    };

    public float FootprintInterval =>
        (BloodyPrintMode)FootprintMode.Value is BloodyPrintMode.Distance
            ? FootprintIntervalDistance.Value
            : FootprintIntervalTime.Value;

    public ModdedNumberOption FootprintSize { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.Bloody.FootprintSize"), 4f, 1f, 10f, 1f, MiraNumberSuffixes.Multiplier);

    public ModdedNumberOption SingleFootprintFadeSeconds { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.Bloody.SingleFootprintFade"), 4f, 1f, 15f, 0.5f, MiraNumberSuffixes.Seconds);

    public ModdedNumberOption KillerTrailDurationSeconds { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.Bloody.FootprintDuration"), 4f, 1f, 15f, 1f, MiraNumberSuffixes.Seconds);

    [ModdedToggleOption("DivaniMods.Options.Bloody.ShowFootprintVent")]
    public bool ShowFootprintVent { get; set; } = false;
}

public enum BloodyPrintMode
{
    Distance,
    Time
}
