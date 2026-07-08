using System;
using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using DivaniMods.Assets;
using TownOfUs.Assets;
using TownOfUs.Modules.Wiki;
using TownOfUs.Roles;
using TownOfUs.Utilities;
using UnityEngine;
using TownOfUs.Extensions;

namespace DivaniMods.Roles.Crewmate.CrewmateSupport;

public sealed class LocatorRole(IntPtr cppPtr)
    : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public static readonly Color LocatorColor = new Color32(0xDD, 0xAB, 0x99, 255);

    public static int MarksRemaining { get; set; }
    public static int MarksThisRound { get; set; }

    public string RoleName => "Locator";
    public string RoleDescription => "Tag the noisy ones!";
    public string RoleLongDescription =>
        "Mark a player to give them the Noisemaker Modifier until the next meeting.";
    public Color RoleColor => LocatorColor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateSupport;

    public DoomableType DoomHintType => DoomableType.Trickster;

    public string GetAdvancedDescription() => RoleLongDescription + MiscUtils.AppendOptionsText(GetType());

    [HideFromIl2Cpp] public List<CustomButtonWikiDescription> Abilities { get; } =
    [
        new("Mark", "Mark a player to give them the Noisemaker Modifier until the next meeting.", DivaniAssets.LocatorIcon)
    ];

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = DivaniAssets.LocatorIcon,
        OptionsScreenshot = TouBanners.CrewmateRoleBanner,
        IntroSound = TouAudio.NoisemakerIntroSound,
        MaxRoleCount = 1,
    };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var stringB = ITownOfUsRole.SetNewTabText(this);
        stringB.AppendLine($"{RoleColor.ToTextColor()}<b>Marks left: {MarksRemaining}</b></color>");
        return stringB;
    }
}
