using System;
using System.Text;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using Reactor.Utilities.Extensions;
using TMPro;
using DivaniMods.Interfaces;
using DivaniMods.Options;
using TownOfUs;
using TownOfUs.Assets;
using TownOfUs.Extensions;
using TownOfUs.Interfaces;
using TownOfUs.Modules.Localization;
using TownOfUs.Modules.Wiki;
using TownOfUs.Options;
using TownOfUs.Roles;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;
using DivaniMods.Assets;
using MiraAPI.Utilities.Assets;

namespace DivaniMods.Roles.Neutral.NeutralEvil;

public sealed class OpportunistRole(IntPtr cppPtr)
    : NeutralRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable, IGuessable, IProgressTally, INeutralEvilWinOutcomeRole
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
    public bool CanBeGuessed => true;

    public string RoleName => "Opportunist";
    public string RoleDescription => "Benefit from others!";
    public string RoleLongDescription =>
        "After you vote a target, every other vote cast on that same target during the meeting counts toward your goal.\n" +
        "Reach the required number of collected votes to win alone.\n" +
        "If enabled by host, use wildcard to make skip votes count towards your tally once.";
    public Color RoleColor => OpportunistColor;

    public LoadableAsset<Sprite> WinIcon => DivaniAssets.OpportunistIcon;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleAlignment RoleAlignment => RoleAlignment.NeutralEvil;
    public bool HasImpostorVision => false;

    public string GetAdvancedDescription() =>
        $"After you vote a target, every other vote cast on that same target during the meeting counts toward your goal (max: {(int)OptionGroupSingleton<OpportunistOptions>.Instance.MaxVotesPerMeeting.Value}).\n" +
        "Reach the required number of collected votes to win alone.\n" +
        "If enabled by host, use wildcard to make skip votes count towards your tally once." +
        MiscUtils.AppendOptionsText(GetType());

    [HideFromIl2Cpp] public List<CustomButtonWikiDescription> Abilities { get; } =
    [
        new("Wildcard", "One-time meeting button that counts Skip votes toward your win goal.", DivaniAssets.OpportunistIcon)
    ];

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = MiraAPI.Utilities.Assets.TmpSpriteUtils.CreateSpriteAsset(DivaniAssets.OpportunistIcon.LoadAsset(), "DivaniMod.Role.Neutral.Opportunist", 1.45f),
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
        task.Text = $"{TownOfUsColors.Neutral.ToTextColor()}{TouLocale.GetParsed("NeutralEvilTaskHeader")}</color>";
        task.name = "NeutralRoleText";
    }

    public string GetVoteTally()
    {
        var needed = (int)OptionGroupSingleton<OpportunistOptions>.Instance.VotesNeeded.Value;
        var capped = Math.Min(VotesCollected, needed);
        return $"{RoleColor.ToTextColor()}({capped}/{needed})</color>";
    }

    public bool ProgressOnName(bool localDead, bool inMeeting, bool amOwner, out string progress)
    {
        if (amOwner || (localDead && OptionGroupSingleton<GeneralOptions>.Instance.TheDeadKnow))
        {
            progress = GetVoteTally();
            return true;
        }

        progress = string.Empty;
        return false;
    }

    public string ProgressOnSummaryNormal => GetVoteTally();

    public string ProgressOnSummaryDetailed =>
        $"Votes collected: {Math.Min(VotesCollected, (int)OptionGroupSingleton<OpportunistOptions>.Instance.VotesNeeded.Value)}/{(int)OptionGroupSingleton<OpportunistOptions>.Instance.VotesNeeded.Value}";

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var stringB = ITownOfUsRole.SetNewTabText(this);
        var needed = (int)OptionGroupSingleton<OpportunistOptions>.Instance.VotesNeeded.Value;
        var maxPerMeeting = (int)OptionGroupSingleton<OpportunistOptions>.Instance.MaxVotesPerMeeting.Value;
        var capped = Math.Min(VotesCollected, needed);
        stringB.AppendLine(TownOfUsPlugin.Culture, $"<b>Votes collected: {capped}/{needed}</b>");
        stringB.AppendLine(TownOfUsPlugin.Culture, $"<b>Max votes per meeting: {maxPerMeeting}</b>");

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
        AboutToTorment = false;
        HasKilled = false;
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
        WildcardButton.SetPlayerId(250);
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

        if (PendingWildcardSkip && meeting.state == MeetingHud.MeetingStates.NotVoted)
        {
            PendingWildcardSkip = false;
            meeting.SkipVoteButton.gameObject.GetComponentInChildren<PassiveButton>()?.OnClick.Invoke();
        }

        WildcardButton.gameObject.SetActive(!WildcardUsed && meeting.state == MeetingHud.MeetingStates.NotVoted);

        if (!WildcardButton.gameObject.active)
        {
            return;
        }

        if (meeting.state == MeetingHud.MeetingStates.Discussion &&
            meeting.discussionTimer < GameOptionsManager.Instance.currentNormalGameOptions.DiscussionTime)
        {
            WildcardButton.SetDisabled();
        }
        else
        {
            WildcardButton.SetEnabled();
        }

        WildcardButton.VoteComplete = meeting.SkipVoteButton.VoteComplete;
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

    public bool ReachedWinCondition => MetThreshold;

    public NeutralEvilWinOutcome WinOutcome => OptionGroupSingleton<OpportunistOptions>.Instance.WinOutcome;

    public NeutralEvilWinOutcome EffectiveWinOutcome => WinOutcome;

    public bool AboutToTorment { get; set; }

    public bool HasKilled { get; set; }

    public bool WinConditionMet()
    {
        return WinOutcome is NeutralEvilWinOutcome.EndsGame && MetThreshold;
    }

    public override bool DidWin(GameOverReason gameOverReason) => MetThreshold;

    public static void ClearAndReload()
    {
        ActiveOpportunists.Clear();
    }
}
