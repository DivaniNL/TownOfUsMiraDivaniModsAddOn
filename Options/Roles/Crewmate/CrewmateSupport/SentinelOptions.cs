using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using DivaniMods.Roles.Crewmate.CrewmateSupport;

namespace DivaniMods.Options;

public class SentinelOptions : AbstractOptionGroup<SentinelRole>
{
    public override string GroupName => "Sentinel";

    [ModdedNumberOption("Max Beacons", 1, 5, 1)]
    public float MaxBeacons { get; set; } = 3;

    [ModdedNumberOption("Place Beacon Cooldown", 5, 60, 5, MiraNumberSuffixes.Seconds)]
    public float PlaceBeaconCooldown { get; set; } = 15;

    [ModdedNumberOption("Place Beacon Duration", 1f, 10f, 0.5f, MiraNumberSuffixes.Seconds)]
    public float PlaceBeaconDuration { get; set; } = 3f;

    [ModdedToggleOption("Show Room Activity In Chat")]
    public bool ShowChatReport { get; set; } = false;
}
