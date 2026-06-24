using System;
using System.Text;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using Reactor.Utilities.Extensions;
using TMPro;
using DivaniMods.Options;
using TownOfUs;
using TownOfUs.Assets;
using TownOfUs.Extensions;
using TownOfUs.Interfaces;
using TownOfUs.Modules.Localization;
using TownOfUs.Modules.Wiki;
using TownOfUs.Roles;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;
using DivaniMods.Assets;

namespace DivaniMods.Roles.Neutral.NeutralOutlier;

public sealed class OpportunistRole(IntPtr cppPtr)
    : NeutralRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable, ICrewVariant, IGuessable
{
    public static readonly Color OpportunistColor = new Color32(216, 184, 90, 255); // gold
    public static Dictionary<byte, OpportunistRole> ActiveOpportunists { get; } = new();

    // Per-meeting state
    public byte? CurrentMeetingTargetId { get; set; }
    public bool VotedThisMeeting { get; set; }
    public bool WildcardActiveThisMeeting { get; set; }
    public bool PendingWildcardSkip { get; set; }

    // Cumulative state
    public int VotesCollected { get; set; }
    public bool MetThreshold { get; set; }
    public bool AboutToWin { get; set; }
    public bool WildcardUsed { get; set; }

    [HideFromIl2Cpp] public PlayerVoteArea? WildcardButton { get; set; }

    public DoomableType DoomHintType => DoomableType.Trickster;
    public RoleBehaviour CrewVariant => RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<EngineerTouRole>());
    public bool CanBeGuessed => true;

    public string RoleName => "Opportunist";
    public string RoleDescription => "Benefit from others!";
    public string RoleLongDescription =>
        "After you vote a target, every other vote cast on that same target during the meeting counts toward your goal.\n" +
        "Reach the required number of collected votes to win alone.\n" +
        "If enabled by host, use wildcard to make skip votes count towards your tally once.";
    public Color RoleColor => OpportunistColor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleAlignment RoleAlignment => RoleAlignment.NeutralOutlier;
    public bool HasImpostorVision => false;

    public string GetAdvancedDescription() => RoleLongDescription + MiscUtils.AppendOptionsText(GetType());

    [HideFromIl2Cpp] public List<CustomButtonWikiDescription> Abilities { get; } =
    [
        new("Wildcard", "One-time meeting button that counts Skip votes toward your win goal.", DivaniAssets.OpportunistIcon)
    ];

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = DivaniAssets.OpportunistIcon,
        IntroSound = DivaniAssets.OpportunistIntroSound,
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
        task.Text = $"{TownOfUsColors.Neutral.ToTextColor()}{TouLocale.GetParsed("NeutralOutlierTaskHeader")}</color>";
        task.name = "NeutralRoleText";
    }

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var stringB = ITownOfUsRole.SetNewTabText(this);
        var needed = (int)OptionGroupSingleton<OpportunistOptions>.Instance.VotesNeeded.Value;
        var capped = Math.Min(VotesCollected, needed);
        stringB.AppendLine(TownOfUsPlugin.Culture, $"<b>Votes collected: {capped}/{needed}</b>");

        if (OptionGroupSingleton<OpportunistOptions>.Instance.CanUseWildcard.Value)
        {
            stringB.AppendLine(TownOfUsPlugin.Culture, $"<b>{(WildcardUsed ? "Wildcard Used" : "Wildcard Available")}</b>");
        }

        return stringB;
    }

    public override void Initialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Initialize(this, targetPlayer);
        ActiveOpportunists[targetPlayer.PlayerId] = this;
        CurrentMeetingTargetId = null;
        VotedThisMeeting = false;
        WildcardActiveThisMeeting = false;
        PendingWildcardSkip = false;
        VotesCollected = 0;
        MetThreshold = false;
        AboutToWin = false;
        WildcardUsed = false;
        WildcardButton = null;
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);
        TouRoleUtils.ClearTaskHeader(Player);
    }

    public override void OnMeetingStart()
    {
        RoleBehaviourStubs.OnMeetingStart(this);

        WildcardActiveThisMeeting = false;
        PendingWildcardSkip = false;
        WildcardButton = null;

        var meeting = MeetingHud.Instance;
        if (Player == null || !Player.AmOwner || meeting == null || WildcardUsed ||
            !OptionGroupSingleton<OpportunistOptions>.Instance.CanUseWildcard.Value)
        {
            return;
        }

        var skip = meeting.SkipVoteButton;
        WildcardButton = Instantiate(skip, skip.transform.parent);
        WildcardButton.Parent = meeting;
        WildcardButton.SetTargetPlayerId(250);
        WildcardButton.transform.localPosition = skip.transform.localPosition + new Vector3(0f, -0.17f, 0f);

        WildcardButton.gameObject.GetComponentInChildren<TextTranslatorTMP>().Destroy();
        WildcardButton.gameObject.GetComponentInChildren<TextMeshPro>().text = "WILDCARD";
        WildcardButton.gameObject.name = "button_wildcardButton";

        skip.transform.localPosition += new Vector3(0f, 0.20f, 0f);
    }

    public void FixedUpdate()
    {
        if (Player == null || Player.Data.Role is not OpportunistRole)
        {
            return;
        }

        var meeting = MeetingHud.Instance;
        if (!Player.AmOwner || meeting == null || WildcardButton == null)
        {
            return;
        }

        if (PendingWildcardSkip && meeting.state == MeetingHud.VoteStates.NotVoted)
        {
            PendingWildcardSkip = false;
            meeting.SkipVoteButton.gameObject.GetComponentInChildren<PassiveButton>()?.OnClick.Invoke();
        }

        WildcardButton.gameObject.SetActive(!WildcardUsed && meeting.state == MeetingHud.VoteStates.NotVoted);

        if (!WildcardButton.gameObject.active)
        {
            return;
        }

        if (meeting.state == MeetingHud.VoteStates.Discussion &&
            meeting.discussionTimer < GameOptionsManager.Instance.currentNormalGameOptions.DiscussionTime)
        {
            WildcardButton.SetDisabled();
        }
        else
        {
            WildcardButton.SetEnabled();
        }

        WildcardButton.voteComplete = meeting.SkipVoteButton.voteComplete;
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
        return MetThreshold || AboutToWin;
    }

    public override bool DidWin(GameOverReason gameOverReason) => MetThreshold;

    public static void ClearAndReload()
    {
        ActiveOpportunists.Clear();
    }
}
