using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using DivaniMods.Modifiers.Game.Universal;
using TownOfUs.Options;
using UnityEngine;

namespace DivaniMods.Options;

public enum UAVRevealMode
{
    Constant,
    Sweeping,
}

public class UAVOptions : AbstractOptionGroup<UAVModifier>
{
    public override Func<bool> GroupVisible => () => OptionGroupSingleton<RoleOptions>.Instance.IsClassicRoleAssignment;
    public override string GroupName => "UAV";
    public override Color GroupColor => UAVModifier.UavColor;
    public override uint GroupPriority => 35;

    public ModdedNumberOption UavUses { get; } =
        new("UAV Uses", 1f, 1f, 3f, 1f, MiraNumberSuffixes.None);

    public ModdedNumberOption UavDuration { get; } =
        new("UAV Duration", 30f, 10f, 45f, 5f, MiraNumberSuffixes.Seconds);

    public ModdedNumberOption UavCooldown { get; } =
        new("UAV Cooldown", 30f, 15f, 60f, 5f, MiraNumberSuffixes.Seconds);

    public ModdedToggleOption ShowPlayerColorsOption { get; } =
        new("UAV Shows Player Colors", false);

    public ModdedToggleOption NotifyOthersOption { get; } =
        new("Notify Players of UAV", true);

    public ModdedToggleOption FriendliesShareVisionOption { get; } =
        new("Friendlies Share UAV Map", true);

    public ModdedEnumOption RevealMode { get; } =
        new("Reveal Mode", (int)UAVRevealMode.Constant, typeof(UAVRevealMode));

    public ModdedNumberOption RadarSweepInterval { get; } =
        new("Radar Sweep Interval", 1f, 0.5f, 5f, 0.5f, MiraNumberSuffixes.Seconds)
        {
            Visible = () => OptionGroupSingleton<UAVOptions>.Instance.RevealMode.Value == (int)UAVRevealMode.Sweeping
        };

    public bool ShowPlayerColors => ShowPlayerColorsOption.Value;
    public bool NotifyOthers => NotifyOthersOption.Value;
    public bool FriendliesShareVision => FriendliesShareVisionOption.Value;
    public bool Sweeping => RevealMode.Value == (int)UAVRevealMode.Sweeping;
    public float SweepInterval => RadarSweepInterval.Value;
}
