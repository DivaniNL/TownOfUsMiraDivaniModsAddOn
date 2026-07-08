using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using DivaniMods.Roles.Crewmate.CrewmateInvestigative;

namespace DivaniMods.Options;

public class SentinelOptions : AbstractOptionGroup<SentinelRole>
{
    public override string GroupName => "Sentinel";

    public ModdedNumberOption MaxBeacons { get; } = new(
        "Max Beacons", 3f, 1f, 5f, 1f, MiraNumberSuffixes.None);

    public ModdedNumberOption PlaceBeaconCooldown { get; } = new(
        "Place Beacon Cooldown", 15f, 5f, 60f, 5f, MiraNumberSuffixes.Seconds);

    public ModdedNumberOption PlaceBeaconDuration { get; } = new(
        "Place Beacon Duration", 3f, 1f, 10f, 0.5f, MiraNumberSuffixes.Seconds);

    [ModdedToggleOption("Show Room Activity In Chat")]
    public bool ShowChatReport { get; set; } = true;

    [ModdedToggleOption("Show Bodies Found In Beacon Rooms")]
    public bool ShowBodyReport { get; set; } = false;
}
