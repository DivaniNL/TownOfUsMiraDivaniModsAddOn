using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using DivaniMods.Roles.Crewmate.CrewmateInvestigative;

namespace DivaniMods.Options;

public class SentinelOptions : AbstractRoleOptionGroup<SentinelRole>
{
    public override string GroupName => MiraLocaleManager.Get("DivaniMods.Role.Sentinel", "Sentinel");

    public ModdedNumberOption MaxBeacons { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.Sentinel.MaxBeacons"), 3f, 1f, 5f, 1f, MiraNumberSuffixes.None);

    public ModdedNumberOption PlaceBeaconCooldown { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.Sentinel.PlaceBeaconCooldown"), 15f, 5f, 60f, 5f, MiraNumberSuffixes.Seconds);

    public ModdedNumberOption PlaceBeaconDuration { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.Sentinel.PlaceBeaconDuration"), 3f, 1f, 10f, 0.5f, MiraNumberSuffixes.Seconds);

    [ModdedToggleOption("DivaniMods.Options.Sentinel.ShowRoomActivityInChat")]
    public bool ShowChatReport { get; set; } = true;

    [ModdedToggleOption("DivaniMods.Options.Sentinel.ShowBodiesFoundInBeaconRooms")]
    public bool ShowBodyReport { get; set; } = false;
}
