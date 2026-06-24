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
using MiraAPI.Modifiers;
using TownOfUs.Modifiers.Game.Alliance;
using TownOfUs.Modifiers.Game;

namespace DivaniMods.Roles.Crewmate.CrewmateSupport;

public sealed class ClockstopperRole(IntPtr cppPtr)
    : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public string RoleName => "Clockstopper";
    public string RoleDescription => "Reset, Rinse and Repeat!";
    public string RoleLongDescription =>
        PlayerControl.LocalPlayer
        && PlayerControl.LocalPlayer.TryGetModifier<AllianceGameModifier>(out var allyMod) && !allyMod.GetsPunished
            ? "Finish a set amount of tasks to <b>reset the cooldowns of those not on your team!</b>"
            : "Finish a set amount of tasks to reset cooldowns!";
    public Color RoleColor => new Color32(175, 138, 162, 255);
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateSupport;

    public DoomableType DoomHintType => DoomableType.Trickster;

    public string GetAdvancedDescription() => RoleLongDescription + MiscUtils.AppendOptionsText(GetType());

    public CustomRoleConfiguration Configuration => new(this)
    {
        OptionsScreenshot = DivaniAssets.ClockstopperBanner,
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
            if (Player.HasModifier<EgotistModifier>())
            {
                stringB.AppendLine($"<b>Use your resets to sabotage the Crew!</b>");
            }
            if (Player.IsImpostorAligned())
            {
                stringB.AppendLine($"<b>Reset Non-Impostor Cooldowns!</b>");
            }
            if (Player.IsLover())
            {
                stringB.AppendLine($"<b>Reset all player cooldowns except for your lover's!</b>");
            }
        return stringB;
    }
}
