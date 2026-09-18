using System;
using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using MiraAPI.Translation;
using DivaniMods.Assets;
using DivaniMods.Interfaces;
using DivaniMods.Events.Crewmate.CrewmateSupport;
using TownOfUs.Extensions;
using TownOfUs.Interfaces;
using TownOfUs.Modules.Wiki;
using TownOfUs.Options;
using TownOfUs.Roles;
using TownOfUs.Utilities;
using UnityEngine;
using MiraAPI.Modifiers;
using TownOfUs.Modifiers.Game.Alliance;
using TownOfUs.Modifiers.Game;

namespace DivaniMods.Roles.Crewmate.CrewmateSupport;

public sealed class ClockstopperRole(IntPtr cppPtr)
    : CrewmateRole(cppPtr), IDivaniRole, IWikiDiscoverable, IDoomable, IProgressTally
{
    public string RoleName => MiraLocaleManager.Get("DivaniMods.Role.Clockstopper", "Clockstopper");
    public string RoleDescription => MiraLocaleManager.Get("DivaniMods.Role.Clockstopper.Description");
    public string RoleMedDescription => MiraLocaleManager.Get("DivaniMods.Role.Clockstopper.MedDescription");
    public string RoleLongDescription =>
        PlayerControl.LocalPlayer
        && PlayerControl.LocalPlayer.TryGetModifier<AllianceGameModifier>(out var allyMod) && !allyMod.GetsPunished
            ? MiraLocaleManager.Get("DivaniMods.Role.Clockstopper.LongDescription.Evil")
            : MiraLocaleManager.Get("DivaniMods.Role.Clockstopper.LongDescription");
    public Color RoleColor => new Color32(175, 138, 162, 255);
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateSupport;

    public DoomableType DoomHintType => DoomableType.Trickster;

    public string GetAdvancedDescription() => RoleLongDescription + MiscUtils.AppendOptionsText(GetType());

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = MiraAPI.Utilities.Assets.TmpSpriteUtils.CreateSpriteAsset(DivaniAssets.ClockstopperIcon.LoadAsset(), "DivaniMod.Role.Crewmate.Clockstopper", 1.45f),
        OptionsScreenshot = DivaniAssets.ClockstopperBanner,
        Icon = DivaniAssets.ClockstopperIcon,
        IntroSound = DivaniAssets.ClockstopperIntroSound,
        MaxRoleCount = 1,
    };

    public string GetResetTally() =>
        $"{RoleColor.ToTextColor()}({ClockstopperEvents.GetProgress(Player)}/{ClockstopperEvents.GetNeeded()})</color>";

    public bool ProgressOnName(bool localDead, bool inMeeting, bool amOwner, out string progress)
    {
        if (!(amOwner || (localDead && OptionGroupSingleton<GeneralOptions>.Instance.TheDeadKnow)))
        {
            progress = string.Empty;
            return false;
        }

        var showTasks = amOwner || (localDead && OptionGroupSingleton<PostmortemOptions>.Instance.ShowTaskDead);
        progress = showTasks ? $"{GetResetTally()} {Player.TaskInfo()}" : GetResetTally();
        return true;
    }

    public string ProgressOnSummaryNormal => Player.TaskInfo();

    public string ProgressOnSummaryDetailed =>
        MiraLocaleManager.Get("StatsTaskCount")
            .Replace("<count>", Player.TaskInfo().Replace("(", "").Replace(")", ""));

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var stringB = ITownOfUsRole.SetNewTabText(this);
        stringB.AppendLine(
            $"{RoleColor.ToTextColor()}<b>{MiraLocaleManager.Get("DivaniMods.Role.Clockstopper.Tab.ResetProgress")
            .Replace("<progress>", ClockstopperEvents.GetProgress(Player).ToString())
            .Replace("<needed>", ClockstopperEvents.GetNeeded().ToString())}</b></color>");
            if (Player.HasModifier<EgotistModifier>())
            {
                stringB.AppendLine($"<b>{MiraLocaleManager.Get("DivaniMods.Role.Clockstopper.Tab.Egotist")}</b>");
            }
            if (Player.IsImpostorAligned())
            {
                stringB.AppendLine($"<b>{MiraLocaleManager.Get("DivaniMods.Role.Clockstopper.Tab.Impostor")}</b>");
            }
            if (Player.IsLover())
            {
                stringB.AppendLine($"<b>{MiraLocaleManager.Get("DivaniMods.Role.Clockstopper.Tab.Lover")}</b>");
            }
        return stringB;
    }
}
