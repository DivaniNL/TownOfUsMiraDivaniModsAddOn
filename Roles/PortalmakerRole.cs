using System;
using MiraAPI.GameOptions;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using DivaniMods.Assets;
using DivaniMods.Options;
using TownOfUs.Roles;
using UnityEngine;

namespace DivaniMods.Roles;

public sealed class PortalmakerRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITownOfUsRole
{
    public string RoleName => "Portalmaker";
    public string RoleDescription => "Place two portals for everyone to use!";
    public string RoleLongDescription => "Place two portals on the map. Once both portals are placed, anyone can use them to teleport between the two locations.";
    public Color RoleColor => new Color(0.047f, 0.420f, 0.961f);
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateSupport;

    public CustomRoleConfiguration Configuration => new(this)
    {
        TasksCountForProgress = true,
        Icon = DivaniAssets.PortalmakerIcon,
        IntroSound = DivaniAssets.PortalMakerIntroSound,
    };

    public override void Initialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Initialize(this, targetPlayer);
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);
    }
}
