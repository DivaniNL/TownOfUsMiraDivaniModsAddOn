using System;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using DivaniMods.Modifiers.Game.Crewmate;
using TownOfUs.Options;
using UnityEngine;

namespace DivaniMods.Options;

public sealed class BloodyOptions : AbstractOptionGroup<BloodyModifier>
{
    public override Func<bool> GroupVisible => () => OptionGroupSingleton<RoleOptions>.Instance.IsClassicRoleAssignment;

    public override string GroupName => "Bloody";

    public override Color GroupColor => BloodyModifier.ModifierUiColor;

    public override uint GroupPriority => 25;

    public ModdedEnumOption FootprintMode { get; set; } = new(
        "Bloody Footprint Placement",
        (int)BloodyPrintMode.Distance,
        typeof(BloodyPrintMode));

    public ModdedNumberOption FootprintIntervalDistance { get; } = new(
        "Bloody Footprint Interval (Distance)",
        0.5f, 0.25f, 3f, 0.5f, MiraNumberSuffixes.None)
    {
        Visible = () =>
            (BloodyPrintMode)OptionGroupSingleton<BloodyOptions>.Instance.FootprintMode.Value is BloodyPrintMode.Distance
    };

    public ModdedNumberOption FootprintIntervalTime { get; } = new(
        "Bloody Footprint Interval (Time)",
        1f, 0.5f, 6f, 0.5f, MiraNumberSuffixes.Seconds)
    {
        Visible = () =>
            (BloodyPrintMode)OptionGroupSingleton<BloodyOptions>.Instance.FootprintMode.Value is BloodyPrintMode.Time
    };

    public float FootprintInterval =>
        (BloodyPrintMode)FootprintMode.Value is BloodyPrintMode.Distance
            ? FootprintIntervalDistance.Value
            : FootprintIntervalTime.Value;

    [ModdedNumberOption("Bloody Footprint Size", 1f, 10f, suffixType: MiraNumberSuffixes.Multiplier)]
    public float FootprintSize { get; set; } = 4f;

    /// <summary>How long each footprint sprite stays on the map (Investigator-style).</summary>
    public ModdedNumberOption SingleFootprintFadeSeconds { get; } = new(
        "Single Footprint Fade", 10f, 1f, 15f, 0.5f, MiraNumberSuffixes.Seconds);

    /// <summary>How long the killer keeps leaving footprints after slaying a Bloody crewmate.</summary>
    public ModdedNumberOption KillerTrailDurationSeconds { get; } = new(
        "Footprint Duration", 10f, 1f, 15f, 1f, MiraNumberSuffixes.Seconds);

    [ModdedToggleOption("Bloody Footprints While In Vent Area")]
    public bool ShowFootprintVent { get; set; } = false;
}

public enum BloodyPrintMode
{
    Distance,
    Time
}
