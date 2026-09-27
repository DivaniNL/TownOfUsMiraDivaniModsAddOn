using System;
using System.Text;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using DivaniMods.Assets;
using DivaniMods.Modules.Duelist;
using DivaniMods.Options;
using TownOfUs;
using TownOfUs.Modules.Wiki;
using TownOfUs.Options;
using TownOfUs.Roles;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;
using TownOfUs.Extensions;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Interfaces;
using DivaniMods.Interfaces;

namespace DivaniMods.Roles.Neutral.NeutralOutlier;

public sealed class DuelistRole(IntPtr cppPtr)
    : NeutralRole(cppPtr), IDivaniRole, IWikiDiscoverable, IDoomable, ICrewVariant, IContinuesGame, IUnlovable,
        IProgressTally
{
    public static readonly Color DuelistColor = new Color32(244, 237, 90, 255);

    public string RoleName => MiraLocaleManager.Get("DivaniMods.Role.Duelist", "Duelist");
    public string RoleDescription => MiraLocaleManager.Get("DivaniMods.Role.Duelist.Description");
    public string RoleMedDescription => MiraLocaleManager.Get("DivaniMods.Role.Duelist.MedDescription");
    public string RoleLongDescription => MiraLocaleManager.Get("DivaniMods.Role.Duelist.LongDescription");
    public Color RoleColor => DuelistColor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleAlignment RoleAlignment => RoleAlignment.NeutralOutlier;

    public DoomableType DoomHintType => DoomableType.Relentless;

    public RoleBehaviour CrewVariant =>
        RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<SheriffRole>());

    public bool HasImpostorVision => true;

    public string GetAdvancedDescription() => RoleLongDescription + MiscUtils.AppendOptionsText(GetType());

    [HideFromIl2Cpp] public List<CustomButtonWikiDescription> Abilities { get; } =
    [
        new(
            MiraLocaleManager.Get("DivaniMods.Role.Duelist.Ability.Duel"),
            MiraLocaleManager.Get("DivaniMods.Role.Duelist.Ability.Duel.Description"),
            DivaniAssets.DuelistDuelButton
        ),
        new(
            MiraLocaleManager.Get("DivaniMods.Role.Duelist.Ability.Strike"),
            MiraLocaleManager.Get("DivaniMods.Role.Duelist.Ability.Strike.Description"),
            DivaniAssets.DuelStrikeButton
        )
    ];

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = MiraAPI.Utilities.Assets.TmpSpriteUtils.CreateSpriteAsset(DivaniAssets.DuelistIcon.LoadAsset(), "DivaniMod.Role.Neutral.Duelist", 1.45f),
        OptionsScreenshot = DivaniAssets.DuelistBanner,
        Icon = DivaniAssets.DuelistIcon,
        IntroSound = DivaniAssets.DuelistIntroSound,
        MaxRoleCount = 1,
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>(),
    };

    public int DuelWins => DuelManager.GetWins(Player.PlayerId);
    public int DuelLosses => DuelManager.GetLosses(Player.PlayerId);

    private static int WinsNeeded => (int)OptionGroupSingleton<DuelistOptions>.Instance.DuelsToWin.Value;
    private static int LossesToDie => (int)OptionGroupSingleton<DuelistOptions>.Instance.DuelsLostToDie.Value;
    private static DuelistWinType WinType => (DuelistWinType)OptionGroupSingleton<DuelistOptions>.Instance.WinType.Value;

    public bool HasMetWinGoal => DuelWins >= WinsNeeded;

    public bool VictoryPending => WinType == DuelistWinType.LeaveInVictory && HasMetWinGoal;

    public bool IsUnlovable => true;

    public bool ContinuesGame => !Player.HasDied() && Helpers.GetAlivePlayers().Count <= 3 &&
        (WinsNeeded - DuelWins) <= 2 && (!VictoryPending || DuelManager.IsDuelUnresolved);

    public override void SpawnTaskHeader(PlayerControl playerControl)
    {
        if (playerControl != PlayerControl.LocalPlayer)
        {
            return;
        }
        ImportantTextTask orCreateTask = PlayerTask.GetOrCreateTask<ImportantTextTask>(playerControl, 0);
        orCreateTask.Text =
            $"{TownOfUsColors.Neutral.ToTextColor()}{MiraLocaleManager.Get("NeutralOutlierTaskHeader")}</color>";
        orCreateTask.name = "NeutralRoleText";
    }

    public string GetDuelTally()
    {
        var wins = Math.Min(DuelWins, WinsNeeded);
        var losses = Math.Min(DuelLosses, LossesToDie);
        return
            $"{Color.green.ToTextColor()}({wins}/{WinsNeeded})</color> {Color.red.ToTextColor()}({losses}/{LossesToDie})</color>";
    }

    public bool ProgressOnName(bool localDead, bool inMeeting, bool amOwner, out string progress)
    {
        if (amOwner || (localDead && OptionGroupSingleton<GeneralOptions>.Instance.TheDeadKnow))
        {
            progress = GetDuelTally();
            return true;
        }

        progress = string.Empty;
        return false;
    }

    public string ProgressOnSummaryNormal => GetDuelTally();

    public string ProgressOnSummaryDetailed
        {
        get
        {
            var wins = Math.Min(DuelWins, WinsNeeded);
            var losses = Math.Min(DuelLosses, LossesToDie);

            return MiraLocaleManager.Get("DivaniMods.Role.Duelist.Progress.Detailed")
                .Replace("<wins>", wins.ToString(TownOfUsPlugin.Culture))
                .Replace("<winsNeeded>", WinsNeeded.ToString(TownOfUsPlugin.Culture))
                .Replace("<losses>", losses.ToString(TownOfUsPlugin.Culture))
                .Replace("<lossesNeeded>", LossesToDie.ToString(TownOfUsPlugin.Culture));
        }
    }

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var stringB = ITownOfUsRole.SetNewTabText(this);

        var wins = Math.Min(DuelWins, WinsNeeded);
        var losses = Math.Min(DuelLosses, LossesToDie);

        var winsText = MiraLocaleManager.Get("DivaniMods.Role.Duelist.Tab.DuelsWon")
            .Replace("<wins>", wins.ToString(TownOfUsPlugin.Culture))
            .Replace("<needed>", WinsNeeded.ToString(TownOfUsPlugin.Culture));

        var lossesText = MiraLocaleManager.Get("DivaniMods.Role.Duelist.Tab.DuelsLost")
            .Replace("<losses>", losses.ToString(TownOfUsPlugin.Culture))
            .Replace("<needed>", LossesToDie.ToString(TownOfUsPlugin.Culture));

        stringB.AppendLine($"<b>{winsText}</b>");
        stringB.AppendLine($"<b>{lossesText}</b>");

        return stringB;
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);
        TouRoleUtils.ClearTaskHeader(Player);
    }

    public bool WinConditionMet()
    {
        if (Player == null || Player.HasDied())
        {
            return false;
        }
        if (!HasMetWinGoal)
        {
            return false;
        }

        return WinType == DuelistWinType.WinAlone || Helpers.GetAlivePlayers().Count <= 1;
    }
    public override bool CanUse(IUsable usable)
    {
        if (!GameManager.Instance.LogicUsables.CanUse(usable, Player))
        {
            return false;
        }

        var console = usable.TryCast<Console>()!;
        return console == null || console.AllowImpostor;
    }
    public override bool DidWin(GameOverReason gameOverReason)
    {
        return HasMetWinGoal;
    }

}
