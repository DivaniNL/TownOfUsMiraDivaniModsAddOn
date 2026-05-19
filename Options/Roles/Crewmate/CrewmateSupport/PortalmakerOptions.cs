using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using DivaniMods.Roles.Crewmate.CrewmateSupport;

namespace DivaniMods.Options;

public class PortalmakerOptions : AbstractOptionGroup<PortalmakerRole>
{
    public override string GroupName => "Portalmaker";

    [ModdedNumberOption("Place Portal Cooldown", 10, 60, 5, MiraNumberSuffixes.Seconds)]
    public float PlacePortalCooldown { get; set; } = 20;

    [ModdedNumberOption("Place Portal Duration", 1f, 10f, 0.5f, MiraNumberSuffixes.Seconds)]
    public float PlacePortalDuration { get; set; } = 3f;

    [ModdedNumberOption("Use Portal Cooldown", 5, 60, 5, MiraNumberSuffixes.Seconds)]
    public float UsePortalCooldown { get; set; } = 10;
}
