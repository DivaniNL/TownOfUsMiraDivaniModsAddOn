using System;
using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using DivaniMods.Assets;
using DivaniMods.Events.Crewmate.CrewmateSupport;
using TownOfUs.Extensions;
using TownOfUs.Modules.Wiki;
using TownOfUs.Roles;
using TownOfUs.Utilities;
using UnityEngine;

namespace DivaniMods.Roles.Crewmate.CrewmateSupport;

public sealed class ClockstopperRole(IntPtr cppPtr)
    : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public string RoleName => "Clockstopper";
    public string RoleDescription => "Reset, Rinse and Repeat!";
    public string RoleLongDescription =>
        "Finish a set amount of tasks to reset cooldowns!";
    public Color RoleColor => new Color32(175, 138, 162, 255);
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateSupport;

    public DoomableType DoomHintType => DoomableType.Insight;

    public string GetAdvancedDescription() => RoleLongDescription + MiscUtils.AppendOptionsText(GetType());

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = DivaniAssets.ClockstopperIcon,
        IntroSound = DivaniAssets.ClockstopperIntroSound,
        MaxRoleCount = 1,
    };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var stringB = ITownOfUsRole.SetNewTabText(this);
        stringB.AppendLine(
            $"{RoleColor.ToTextColor()}<b>Reset cooldown progress: {ClockstopperEvents.GetProgress(Player)} / {ClockstopperEvents.GetNeeded()}</b></color>");
        return stringB;
    }
}
