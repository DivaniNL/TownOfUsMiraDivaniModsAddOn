using System;
using AmongUs.GameOptions;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using DivaniMods.Assets;
using TownOfUs.Extensions;
using TownOfUs.Modules.Wiki;
using TownOfUs.Roles;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace DivaniMods.Roles;

public sealed class FragRole(IntPtr cppPtr)
    : ImpostorRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable, ICrewVariant
{
    public string RoleName => "Frag";
    public string RoleDescription => "Start a hot-potato time bomb!";
    public string RoleLongDescription =>
        "Give a time bomb to a player.\n" +
        "After a short random delay the timer starts.\n" +
        "The holder can pass it on, but not back\n" +
        "to the previous holder until it moves again.";
    public Color RoleColor => Palette.ImpostorRed;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorKilling;

    public DoomableType DoomHintType => DoomableType.Relentless;

    public RoleBehaviour CrewVariant =>
        RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<TrapperRole>());

    public string GetAdvancedDescription() => RoleLongDescription + MiscUtils.AppendOptionsText(GetType());

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = DivaniAssets.FragIcon,
        IntroSound = DivaniAssets.FragIntroSound,
        MaxRoleCount = 1,
    };
}
