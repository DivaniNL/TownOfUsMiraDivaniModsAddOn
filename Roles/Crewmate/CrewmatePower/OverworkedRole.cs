using Il2CppInterop.Runtime.Attributes;
using System;
using System.Collections.Generic;
using MiraAPI.GameOptions;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using DivaniMods.Assets;
using DivaniMods.Options;
using TownOfUs.Extensions;
using TownOfUs.Modules.Wiki;
using TownOfUs.Roles;
using TownOfUs.Utilities;
using UnityEngine;

namespace DivaniMods.Roles.Crewmate.CrewmatePower;

public sealed class OverworkedRole(IntPtr cppPtr)
    : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public string RoleName => "Overworked";
    public string RoleDescription => "Task. Overwork. Win.";
    public string RoleLongDescription => "Do the work for others to bring forth victory for the Crewmates!";
    public Color RoleColor => new Color(0.5f, 0.3f, 0.1f);
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmatePower;

    public DoomableType DoomHintType => DoomableType.Trickster;

    public string GetAdvancedDescription() => RoleLongDescription + MiscUtils.AppendOptionsText(GetType());

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = DivaniAssets.OverworkedIcon,
        MaxRoleCount = 1,
    };
}