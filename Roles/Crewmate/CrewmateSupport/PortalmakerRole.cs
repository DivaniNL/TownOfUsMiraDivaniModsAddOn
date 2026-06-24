using Il2CppInterop.Runtime.Attributes;
using System;
using MiraAPI.GameOptions;
using MiraAPI.Roles;
using DivaniMods.Assets;
using DivaniMods.Options;
using TownOfUs.Extensions;
using TownOfUs.Modules.Wiki;
using TownOfUs.Roles;
using TownOfUs.Utilities;
using UnityEngine;

namespace DivaniMods.Roles.Crewmate.CrewmateSupport;

public sealed class PortalmakerRole(IntPtr cppPtr)
    : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public string RoleName => "Portalmaker";
    public string RoleDescription => "From here, to there!";
    public string RoleLongDescription => "Place two portals on the map. Once both portals are placed, anyone can use them to teleport between the two locations.";
    public Color RoleColor => new Color(0.047f, 0.420f, 0.961f);
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateSupport;

    public DoomableType DoomHintType => DoomableType.Trickster;

    public string GetAdvancedDescription() => RoleLongDescription + MiscUtils.AppendOptionsText(GetType());

    [HideFromIl2Cpp] public List<CustomButtonWikiDescription> Abilities { get; } =
    [
        new("Place Portal", "Place a portal on the ground. Place two to open a route.", DivaniAssets.PlacePortalButton),
        new("Use Portal", "Teleport from one portal to another.", DivaniAssets.UsePortalButton)
    ];

    public CustomRoleConfiguration Configuration => new(this)
    {
        OptionsScreenshot = DivaniAssets.PortalmakerBanner,
        Icon = DivaniAssets.PortalmakerIcon,
        IntroSound = DivaniAssets.PortalMakerIntroSound,
        MaxRoleCount = 1,
    };
}
