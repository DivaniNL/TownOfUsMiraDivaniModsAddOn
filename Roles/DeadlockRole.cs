using System;
using MiraAPI.Roles;
using DivaniMods.Assets;
using TownOfUs.Roles;
using UnityEngine;

namespace DivaniMods.Roles;

public sealed class DeadlockRole(IntPtr cppPtr) : ImpostorRole(cppPtr), ITownOfUsRole
{
    public string RoleName => "Deadlock";
    public string RoleDescription => "Lock down crewmate tasks!";
    public string RoleLongDescription => "Use your Lockdown ability to temporarily\ndisable all crewmate tasks.\nDuring lockdown, crewmates cannot access\nor complete any tasks.";
    public Color RoleColor => Palette.ImpostorRed;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorSupport;

    public CustomRoleConfiguration Configuration => new(this)
    {
        TasksCountForProgress = false,
        Icon = DivaniAssets.DeadlockIcon,
        UseVanillaKillButton = true,
        IntroSound = DivaniAssets.DeadlockIntroSound,
    };
}
