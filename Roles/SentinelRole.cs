using System;
using MiraAPI.Roles;
using DivaniMods.Assets;
using TownOfUs.Extensions;
using TownOfUs.Modules.Wiki;
using TownOfUs.Roles;
using TownOfUs.Utilities;
using UnityEngine;

namespace DivaniMods.Roles;

public sealed class SentinelRole(IntPtr cppPtr)
    : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
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

    // Doomsayer hint category: surveillance/info fits Insight (same as Sentry/Trapper).
    public DoomableType DoomHintType => DoomableType.Insight;

    public string GetAdvancedDescription() => RoleLongDescription + MiscUtils.AppendOptionsText(GetType());

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = DivaniAssets.SentinelIcon,
    };
}
