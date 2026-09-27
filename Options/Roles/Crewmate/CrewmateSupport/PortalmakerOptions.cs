using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using DivaniMods.Roles.Crewmate.CrewmateSupport;

namespace DivaniMods.Options;

public class PortalmakerOptions : AbstractRoleOptionGroup<PortalmakerRole>
{
    public override string GroupName => MiraLocaleManager.Get("DivaniMods.Role.Portalmaker", "Portalmaker");

    public ModdedNumberOption PlacePortalCooldown { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.Portalmaker.PlacePortalCooldown"), 25f, 10f, 60f, 5f, MiraNumberSuffixes.Seconds);

    public ModdedNumberOption PlacePortalDuration { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.Portalmaker.PlacePortalDuration"), 3f, 1f, 10f, 0.5f, MiraNumberSuffixes.Seconds);

    public ModdedNumberOption UsePortalCooldown { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.Portalmaker.UsePortalCooldown"), 10f, 5f, 60f, 5f, MiraNumberSuffixes.Seconds);

    [ModdedToggleOption("DivaniMods.Options.Portalmaker.EnableAfterFirstMeeting")]
    public bool EnableAfterFirstMeeting { get; set; } = false;

    [ModdedToggleOption("DivaniMods.Options.Portalmaker.PortalmakerDirectTeleport")]
    public bool PortalmakerDirectTeleport { get; set; } = true;
}
