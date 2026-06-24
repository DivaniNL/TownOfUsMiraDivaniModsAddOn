using System;
using System.Text;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using DivaniMods.Assets;
using DivaniMods.Modules.Duelist;
using DivaniMods.Options;
using TownOfUs;
using TownOfUs.Modules.Localization;
using TownOfUs.Modules.Wiki;
using TownOfUs.Roles;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;
using TownOfUs.Extensions;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Interfaces;

namespace DivaniMods.Roles.Neutral.NeutralOutlier;

public sealed class DuelistRole(IntPtr cppPtr)
    : NeutralRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable, ICrewVariant, IContinuesGame, IUnlovable
{
    public static readonly Color DuelistColor = new Color32(125, 112, 95, 255);

    public string RoleName => "Duelist";
    public string RoleDescription => "ITS TIME TO D-D-D-DUEL";
    public string RoleLongDescription =>
        "Challenge a player to a duel.\n" +
        "Win enough duels to claim victory.";
    public Color RoleColor => DuelistColor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleAlignment RoleAlignment => RoleAlignment.NeutralOutlier;

    public DoomableType DoomHintType => DoomableType.Relentless;

    public bool HasImpostorVision => true;

    public string GetAdvancedDescription() => RoleLongDescription + MiscUtils.AppendOptionsText(GetType());

    [HideFromIl2Cpp] public List<CustomButtonWikiDescription> Abilities { get; } =
    [
        new("Duel", "Challenge a shipmate to a duel.", DivaniAssets.DuelistDuelButton),
        new("Strike", "Attack your opponent during the duel.", DivaniAssets.DuelStrikeButton)
    ];

    public CustomRoleConfiguration Configuration => new(this)
    {
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

    public RoleBehaviour CrewVariant =>
        RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<SheriffRole>());

    public bool IsUnlovable => true;

    public bool ContinuesGame => !Player.HasDied() && WinType == DuelistWinType.WinAlone && Helpers.GetAlivePlayers().Count <= 3 && (WinsNeeded - DuelWins) <= 2;

    public override void SpawnTaskHeader(PlayerControl playerControl)
    {
        if (playerControl != PlayerControl.LocalPlayer)
        {
            return;
        }
        ImportantTextTask orCreateTask = PlayerTask.GetOrCreateTask<ImportantTextTask>(playerControl, 0);
        orCreateTask.Text =
            $"{TownOfUsColors.Neutral.ToTextColor()}{TouLocale.GetParsed("NeutralOutlierTaskHeader")}</color>";
        orCreateTask.name = "NeutralRoleText";
    }

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var stringB = ITownOfUsRole.SetNewTabText(this);
        stringB.AppendLine(TownOfUsPlugin.Culture, $"<b>Duels won ({Math.Min(DuelWins, WinsNeeded)}/{WinsNeeded})</b>");
        stringB.AppendLine(TownOfUsPlugin.Culture, $"<b>Duels lost ({Math.Min(DuelLosses, LossesToDie)}/{LossesToDie})</b>");
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
        if (WinType == DuelistWinType.WinAlone)
        {
            return true;
        }
        return MiscUtils.RealKillersAliveCount == 0;
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
