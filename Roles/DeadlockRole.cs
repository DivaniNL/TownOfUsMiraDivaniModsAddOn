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

public sealed class DeadlockRole(IntPtr cppPtr)
    : ImpostorRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable, ICrewVariant
{
    public string RoleName => "Deadlock";
    public string RoleDescription => "Lock down crewmate tasks!";
    public string RoleLongDescription => "Use your Lockdown ability to temporarily\ndisable all crewmate tasks.\nDuring lockdown, crewmates cannot access\nor complete any tasks.";
    public Color RoleColor => Palette.ImpostorRed;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorSupport;

    // Doomsayer hint category: sabotage/disruption fits Insight (same as Blackmailer).
    public DoomableType DoomHintType => DoomableType.Insight;

    // Imitator: Engineer is the closest crew equivalent (task infrastructure).
    public RoleBehaviour CrewVariant =>
        RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<EngineerTouRole>());

    public string GetAdvancedDescription() => RoleLongDescription + MiscUtils.AppendOptionsText(GetType());

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = DivaniAssets.DeadlockIcon,
        IntroSound = DivaniAssets.DeadlockIntroSound,
        MaxRoleCount = 1,
    };
}
