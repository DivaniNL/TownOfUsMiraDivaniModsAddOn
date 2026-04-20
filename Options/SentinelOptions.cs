using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using DivaniMods.Roles;

namespace DivaniMods.Options;

public class SentinelOptions : AbstractOptionGroup<SentinelRole>
{
    public override string GroupName => "Sentinel";

    [ModdedNumberOption("Max Beacons", 1, 5, 1)]
    public float MaxBeacons { get; set; } = 3;

    [ModdedNumberOption("Place Beacon Cooldown", 5, 60, 5, MiraNumberSuffixes.Seconds)]
    public float PlaceBeaconCooldown { get; set; } = 15;

    [ModdedToggleOption("Show Room Activity In Chat")]
    public bool ShowChatReport { get; set; } = false;
}
