using System;
using MiraAPI.Roles;
using MiraAPI.Patches.Stubs;
using DivaniMods.Assets;
using TownOfUs.Modules.Wiki;
using TownOfUs.Roles;
using TownOfUs.Utilities;
using UnityEngine;

namespace DivaniMods.Roles;

public sealed class SentinelRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable
{
    public static readonly Color SentinelColor = new Color32(244, 169, 60, 255);

    public string RoleName => "Sentinel";
    public string RoleDescription => "Place beacons to monitor rooms!";
    public string RoleLongDescription => "Place beacons in rooms to track who\npasses through them.\n" +
        "You will see a flash when someone\nenters a room with your beacon.\n" +
        "During meetings you can see who\npassed through each beacon's room.";
    public Color RoleColor => SentinelColor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateSupport;

    public string GetAdvancedDescription() => RoleLongDescription + MiscUtils.AppendOptionsText(GetType());

    public CustomRoleConfiguration Configuration => new(this)
    {
        TasksCountForProgress = true,
        Icon = DivaniAssets.SentinelIcon,
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
