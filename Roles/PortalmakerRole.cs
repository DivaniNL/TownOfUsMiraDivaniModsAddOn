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

namespace DivaniMods.Roles;

public sealed class PortalmakerRole(IntPtr cppPtr)
    : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public string RoleName => "Portalmaker";
    public string RoleDescription => "Place two portals for everyone to use!";
    public string RoleLongDescription => "Place two portals on the map. Once both portals are placed, anyone can use them to teleport between the two locations.";
    public Color RoleColor => new Color(0.047f, 0.420f, 0.961f);
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateSupport;

    // Doomsayer hint category: teleport/manipulation fits Trickster (same as Plumber).
    public DoomableType DoomHintType => DoomableType.Trickster;

    public string GetAdvancedDescription() => RoleLongDescription + MiscUtils.AppendOptionsText(GetType());

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = DivaniAssets.PortalmakerIcon,
        IntroSound = DivaniAssets.PortalMakerIntroSound,
    };
}
