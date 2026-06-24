using Il2CppInterop.Runtime.Attributes;
using System;
using System.Collections.Generic;
using MiraAPI.Roles;
using DivaniMods.Assets;
using TownOfUs.Extensions;
using TownOfUs.Modules.Wiki;
using TownOfUs.Roles;
using TownOfUs.Utilities;
using UnityEngine;
using MiraAPI.Modifiers;
using TownOfUs.Modifiers.Game;

namespace DivaniMods.Roles.Crewmate.CrewmateProtective;

public sealed class DomesmithRole(IntPtr cppPtr)
    : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public static readonly Color DomesmithColor = new Color32(0x0E, 0xAA, 0xC3, 255);

    public string RoleName => "Domesmith";
    public string RoleDescription => "Shield the group!";
    public string RoleLongDescription =>
        PlayerControl.LocalPlayer
        && PlayerControl.LocalPlayer.TryGetModifier<AllianceGameModifier>(out var allyMod) && !allyMod.GetsPunished
            ? "Drop protective domes on the ground to <b>protect evils!</b>"
            : "Drop protective domes on the ground to protect the group!";
    
    public Color RoleColor => DomesmithColor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateProtective;

    public DoomableType DoomHintType => DoomableType.Protective;

    public string GetAdvancedDescription() => RoleLongDescription + MiscUtils.AppendOptionsText(GetType());

    [HideFromIl2Cpp] public List<CustomButtonWikiDescription> Abilities { get; } =
    [
        new("Place Dome", "Drop a dome to protect players inside from kills.", DivaniAssets.DomesmithPlaceDomeButton)
    ];

    public CustomRoleConfiguration Configuration => new(this)
    {
        OptionsScreenshot = DivaniAssets.DomesmithBanner,
        Icon = DivaniAssets.DomesmithIcon,
        IntroSound = DivaniAssets.DomesmithIntroSound,
        MaxRoleCount = 1,
    };
}
