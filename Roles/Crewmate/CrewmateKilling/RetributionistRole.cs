using System;
using MiraAPI.GameOptions;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using DivaniMods.Assets;
using DivaniMods.Events.Crewmate.CrewmateKilling;
using DivaniMods.Options;
using TownOfUs.Extensions;
using TownOfUs.Interfaces;
using TownOfUs.Modules.Localization;
using TownOfUs.Modules.Wiki;
using TownOfUs.Options;
using TownOfUs.Roles;
using TownOfUs.Utilities;
using UnityEngine;

namespace DivaniMods.Roles.Crewmate.CrewmateKilling;

public sealed class RetributionistRole(IntPtr cppPtr)
    : CrewmateRole(cppPtr), ITouCrewRole, IWikiDiscoverable, IDoomable, IProgressTally
{
    public static readonly Color RetributionistColor = new Color32(175, 22, 81, 255);

    public bool IsPowerCrew
    {
        get
        {
            if (Player == null)
            {
                return false;
            }

            var opts = OptionGroupSingleton<RetributionistOptions>.Instance;
            return opts.StallGame && (!opts.TurnIntoSoulOnce ||
                                      !RetributionistManager.UsedRevenge.Contains(Player.PlayerId));
        }
    }

    public string RoleName => "Retributionist";
    public string RoleDescription => "Seek revenge on your killer!";
    public string RoleLongDescription => "When you die, you get to seek revenge on your killer";
    public Color RoleColor => RetributionistColor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateKilling;

    public DoomableType DoomHintType => DoomableType.Death;

    public string GetAdvancedDescription() =>
        "When you get killed, you spawn on a random vent as the Vengeful Soul and you get a " +
        "limited time to find and kill your killer. If you succeed, you get to live again. " +
        "If you fail, you become a normal ghost. Your killer cannot vent or use their ability " +
        "if they're an Impostor Concealing role." +
        MiscUtils.AppendOptionsText(GetType());

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = MiraAPI.Utilities.Assets.TmpSpriteUtils.CreateSpriteAsset(DivaniAssets.RetributionistIcon.LoadAsset(), "DivaniMod.Role.Crewmate.Retributionist", 1.45f),
        OptionsScreenshot = DivaniAssets.RetributionistBanner,
        Icon = DivaniAssets.RetributionistIcon,
        IntroSound = DivaniAssets.RetributionistIntroSound,
        MaxRoleCount = 1,
    };

    private static bool RevengeIsLimited => OptionGroupSingleton<RetributionistOptions>.Instance.TurnIntoSoulOnce;

    public string GetRevengeTally()
    {
        var available = Player != null && !RetributionistManager.UsedRevenge.Contains(Player.PlayerId);
        return $"{RoleColor.ToTextColor()}({(available ? "☐" : "✓")})</color>";
    }

    public bool ProgressOnName(bool localDead, bool inMeeting, bool amOwner, out string progress)
    {
        if (!RevengeIsLimited || !(amOwner || (localDead && PlayerControl.LocalPlayer.DiedOtherRound() &&
                                               OptionGroupSingleton<GeneralOptions>.Instance.TheDeadKnow)))
        {
            progress = string.Empty;
            return false;
        }

        var showTasks = amOwner || (localDead && OptionGroupSingleton<PostmortemOptions>.Instance.ShowTaskDead);
        progress = showTasks ? $"{GetRevengeTally()} {Player.TaskInfo()}" : GetRevengeTally();
        return true;
    }

    public string ProgressOnSummaryNormal => Player.TaskInfo();

    public string ProgressOnSummaryDetailed =>
        TouLocale.GetParsed("StatsTaskCount")
            .Replace("<count>", Player.TaskInfo().Replace("(", "").Replace(")", ""));
}
