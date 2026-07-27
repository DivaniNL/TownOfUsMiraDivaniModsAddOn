using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using DivaniMods.Roles.Crewmate.CrewmateSupport;

namespace DivaniMods.Options;

public class PortalmakerOptions : AbstractRoleOptionGroup<PortalmakerRole>
{
    public override string GroupName => "Portalmaker";

    public ModdedNumberOption PlacePortalCooldown { get; } = new(
        "Place Portal Cooldown", 25f, 10f, 60f, 5f, MiraNumberSuffixes.Seconds);

    public ModdedNumberOption PlacePortalDuration { get; } = new(
        "Place Portal Duration", 3f, 1f, 10f, 0.5f, MiraNumberSuffixes.Seconds);

    public ModdedNumberOption UsePortalCooldown { get; } = new(
        "Use Portal Cooldown", 10f, 5f, 60f, 5f, MiraNumberSuffixes.Seconds);

    [ModdedToggleOption("Enable Portals After First Meeting")]
    public bool EnableAfterFirstMeeting { get; set; } = false;

    [ModdedToggleOption("Portalmaker Can Teleport To Own Portals")]
    public bool PortalmakerDirectTeleport { get; set; } = true;
}
