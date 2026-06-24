using Il2CppInterop.Runtime.Attributes;
using System;
using MiraAPI.Roles;
using DivaniMods.Assets;
using TownOfUs.Extensions;
using TownOfUs.Modules.Wiki;
using TownOfUs.Roles;
using TownOfUs.Utilities;
using UnityEngine;

namespace DivaniMods.Roles.Crewmate.CrewmateInvestigative;

public sealed class SentinelRole(IntPtr cppPtr)
    : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public static readonly Color SentinelColor = new Color32(244, 169, 60, 255);

    public string RoleName => "Sentinel";
    public string RoleDescription => "Monitor rooms!";
    public string RoleLongDescription => "Place beacons in rooms to track who\npasses through them.\n" +
        "You will see a flash when someone\nenters a room with your beacon.\n" +
        "During meetings you can see who\npassed through each beacon's room.";
    public Color RoleColor => SentinelColor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateInvestigative;

    public DoomableType DoomHintType => DoomableType.Insight;

    public string GetAdvancedDescription() => RoleLongDescription + MiscUtils.AppendOptionsText(GetType());

    [HideFromIl2Cpp] public List<CustomButtonWikiDescription> Abilities { get; } =
    [
        new("Place Beacon", "Place a Beacon in a room to monitor it's activity", DivaniAssets.SentinelPlaceBeaconButton)
    ];

    public CustomRoleConfiguration Configuration => new(this)
    {
        OptionsScreenshot = DivaniAssets.SentinelBanner,
        Icon = DivaniAssets.SentinelIcon,
        IntroSound = DivaniAssets.SentinelIntroSound,
        MaxRoleCount = 1,
    };
}
