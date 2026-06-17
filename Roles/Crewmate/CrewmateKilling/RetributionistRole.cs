using System;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using DivaniMods.Assets;
using TownOfUs.Extensions;
using TownOfUs.Modules.Wiki;
using TownOfUs.Roles;
using TownOfUs.Utilities;
using UnityEngine;

namespace DivaniMods.Roles.Crewmate.CrewmateKilling;

public sealed class RetributionistRole(IntPtr cppPtr)
    : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public static readonly Color RetributionistColor = new Color32(175, 22, 48, 255);

    public string RoleName => "Retributionist";
    public string RoleDescription => "Seek revenge on your killer!";
    public string RoleLongDescription => "When you die, you get to seek revenge on your killer";
    public Color RoleColor => RetributionistColor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateKilling;

    public DoomableType DoomHintType => DoomableType.Relentless;

    public string GetAdvancedDescription() =>
        "When you get killed, you spawn on a random vent as the Vengeful Soul and you get a " +
        "limited time to find and kill your killer. If you succeed, you get to live again. " +
        "If you fail, you become a normal ghost. Your killer cannot vent or use their ability " +
        "if they're an Impostor Concealing role." +
        MiscUtils.AppendOptionsText(GetType());

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = DivaniAssets.RetributionistIcon,
        IntroSound = DivaniAssets.RetributionistIntroSound,
        MaxRoleCount = 1,
    };
}
