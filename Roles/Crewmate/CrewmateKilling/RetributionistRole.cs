using System;
using MiraAPI.GameOptions;
using MiraAPI.Roles;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using DivaniMods.Assets;
using DivaniMods.Events.Crewmate.CrewmateKilling;
using DivaniMods.Options;
using DivaniMods.Interfaces;
using TownOfUs.Extensions;
using TownOfUs.Interfaces;
using TownOfUs.Modules.Wiki;
using TownOfUs.Options;
using TownOfUs.Roles;
using TownOfUs.Utilities;
using UnityEngine;

namespace DivaniMods.Roles.Crewmate.CrewmateKilling;

public sealed class RetributionistRole(IntPtr cppPtr)
    : CrewmateRole(cppPtr), IDivaniCrewRole, IWikiDiscoverable, IDoomable, IProgressTally
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

    public string RoleName => MiraLocaleManager.Get("DivaniMods.Role.Retributionist", "Retributionist");
    public string RoleDescription => MiraLocaleManager.Get("DivaniMods.Role.Retributionist.Description");
    public string RoleMedDescription => MiraLocaleManager.Get("DivaniMods.Role.Retributionist.MedDescription");
    public string RoleLongDescription => MiraLocaleManager.Get("DivaniMods.Role.Retributionist.LongDescription");
    public Color RoleColor => RetributionistColor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateKilling;

    public DoomableType DoomHintType => DoomableType.Death;

    public string GetAdvancedDescription() =>
        MiraLocaleManager.Get("DivaniMods.Role.Retributionist.AdvancedDescription") +
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
        return $"{RoleColor.ToTextColor()}{(available ? $"<color=#FF0000>☐</color>" : $"<color=#00FF00>✓</color>")}";
    }

    public bool ProgressOnName(bool localDead, bool inMeeting, bool amOwner, out string progress)
    {
        if (!RevengeIsLimited || !(amOwner || (localDead && OptionGroupSingleton<GeneralOptions>.Instance.TheDeadKnow)))
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
       MiraLocaleManager.Get("StatsTaskCount")
            .Replace("<count>", Player.TaskInfo().Replace("(", "").Replace(")", ""));
}
