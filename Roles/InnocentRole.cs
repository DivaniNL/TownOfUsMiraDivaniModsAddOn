using System;
using AmongUs.GameOptions;
using MiraAPI.GameOptions;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using DivaniMods.Assets;
using DivaniMods.Options;
using TownOfUs;
using TownOfUs.Extensions;
using TownOfUs.Interfaces;
using TownOfUs.Modules.Localization;
using TownOfUs.Modules.Wiki;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Roles;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;

namespace DivaniMods.Roles;

public sealed class InnocentRole(IntPtr cppPtr)
    : NeutralRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable, ICrewVariant, IGuessable
{
    public static readonly Color InnocentColor = new Color32(255, 141, 168, 255);
    public static Dictionary<byte, InnocentRole> ActiveInnocents { get; } = new();

    public byte? PendingTauntKillerId { get; set; }
    public byte? TauntedKillerId { get; set; }
    public bool ShowTauntedTargetSymbol { get; set; }
    public bool TargetVoted { get; set; }
    public bool AboutToWin { get; set; }
    public bool AwaitingNextMeetingExile { get; set; }
    public bool WinWindowExpired { get; set; }

    public DoomableType DoomHintType => DoomableType.Trickster;
    public RoleBehaviour CrewVariant => RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<EngineerTouRole>());
    public bool CanBeGuessed => true;
    public string RoleName => "Innocent";
    public string RoleDescription => "Taunt someone into killing you!";
    public string RoleLongDescription =>
        "Use Taunt on another player to make them immediately kill you.\n" +
        "If that player is voted out in the next meeting, you win.";
    public Color RoleColor => InnocentColor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleAlignment RoleAlignment => RoleAlignment.NeutralEvil;
    public bool HasImpostorVision => false;

    public string GetAdvancedDescription() => RoleLongDescription + MiscUtils.AppendOptionsText(GetType());

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = DivaniAssets.InnocentIcon,
        IntroSound = TownOfUs.Assets.TouAudio.NoisemakerIntroSound,
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>(),
        MaxRoleCount = 1,
    };

    public override void SpawnTaskHeader(PlayerControl playerControl)
    {
        if (playerControl != PlayerControl.LocalPlayer)
        {
            return;
        }

        var task = PlayerTask.GetOrCreateTask<ImportantTextTask>(playerControl, 0);
        task.Text = $"{TownOfUsColors.Neutral.ToTextColor()}{TouLocale.GetParsed("NeutralEvilTaskHeader")}</color>";
        task.name = "NeutralRoleText";
    }

    public override void Initialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Initialize(this, targetPlayer);
        ActiveInnocents[targetPlayer.PlayerId] = this;
        PendingTauntKillerId = null;
        TauntedKillerId = null;
        ShowTauntedTargetSymbol = false;
        TargetVoted = false;
        AboutToWin = false;
        AwaitingNextMeetingExile = false;
        WinWindowExpired = false;
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);
        TouRoleUtils.ClearTaskHeader(Player);
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

    public bool WinConditionMet()
    {
        return TargetVoted || AboutToWin;
    }

    public override bool DidWin(GameOverReason gameOverReason) => TargetVoted;

    public static void ClearAndReload()
    {
        ActiveInnocents.Clear();
    }
}
