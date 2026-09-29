using AmongUs.GameOptions;
using System.Collections;
using System.Linq;
using DivaniMods.Assets;
using DivaniMods.Modifiers.Crewmate.CrewmatePower;
using DivaniMods.Options;
using Il2CppInterop.Runtime.Attributes;
using Il2CppSystem.Runtime.InteropServices;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using TownOfUs;
using TownOfUs.Assets;
using TownOfUs.Extensions;
using TownOfUs.Modules;
using TownOfUs.Modules.Components;
using TownOfUs.Modules.Wiki;
using TownOfUs.Roles;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using TownOfUs.Modifiers;
using System.Text;
using Reactor.Utilities.Extensions;

namespace DivaniMods.Roles.Crewmate.CrewmatePower;

public sealed class MentorRole(IntPtr cppPtr)
    : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    private MeetingMenu? meetingMenu;
    private static readonly Vector3 TeachButtonPosition = new(-0.35f, 0f, -3f);
    private AmbassadorSelectionMinigame? teachMenu;
    private int meetingCount;
    private int tasksCompletedAtLastLesson = -1;
    private readonly HashSet<byte> taughtPlayers = new();
    public static readonly Color MentorColor = new Color32(150, 150, 200, 255);
    public string RoleName => "Mentor";
    public string RoleDescription => "Teach the Crewmates!";
    public string RoleLongDescription => "Teach players to learn skills of roles for one round.\nYour lesson fails if it targets a Non-Crewmate.";
    public Color RoleColor => MentorColor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmatePower;

    public override bool IsAffectedByComms => false;

    public DoomableType DoomHintType => DoomableType.Perception;

    public string GetAdvancedDescription() => RoleLongDescription + MiscUtils.AppendOptionsText(GetType());

    public byte StudentId { get; set; } = byte.MaxValue;
    public ushort LessonRoleId { get; set; } = default;

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = DivaniAssets.MentorIcon,
        IconTmp = MiraAPI.Utilities.Assets.TmpSpriteUtils.CreateSpriteAsset(DivaniAssets.MentorIcon.LoadAsset(), "DivaniMod.Role.Crewmate.Mentor", 1.45f),
        IntroSound = DivaniAssets.MentorIntroSound,
        MaxRoleCount = 1
    };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
       var stringB = ITownOfUsRole.SetNewTabText(this);

        stringB.AppendLine($"Current Lessons:");

        if (ShowChosenLesson())
        {
            var targetName = GameData.Instance.GetPlayerById(StudentId)?.Object?.Data?.PlayerName;
            var roleObj = RoleManager.Instance.GetRole((RoleTypes)LessonRoleId) as ITownOfUsRole;
            var roleName = roleObj?.RoleName;
            var roleColor = roleObj != null ? ColorUtility.ToHtmlStringRGB(roleObj.RoleColor) : "9999FF";
            stringB.AppendLine($"<b>Student: {targetName}. Role Taught: <color=#{roleColor}>{roleName}</color></b>");
            return stringB;
        }

        return stringB;
    }

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);

        meetingCount = 0;
        tasksCompletedAtLastLesson = -1;
        taughtPlayers.Clear();
        ClearLesson();

        if (Player.AmOwner)
        {
            StudentId = byte.MaxValue;

            meetingMenu = new MeetingMenu(
                this,
                OpenLessonMenu,
                "Teach",
                MeetingAbilityType.Click,
                DivaniAssets.MentorTeachButton,
                exemption: IsExempt,
                position: TeachButtonPosition);
        }
    }

    public override void OnMeetingStart()
    {
        RoleBehaviourStubs.OnMeetingStart(this);

        meetingCount++;
        ClearLesson();

        var meeting = MeetingHud.Instance;
        if (Player.AmOwner && meeting != null && !Player.HasDied())
        {
            meetingMenu?.GenButtons(meeting, true);
        }
    }

    public override void OnVotingComplete()
    {
        RoleBehaviourStubs.OnVotingComplete(this);

        if (Player.AmOwner)
        {
            meetingMenu?.HideButtons();

            if (teachMenu != null)
            {
                teachMenu.Close();
                teachMenu = null;
            }
        }
    }

    [HideFromIl2Cpp]
    private int CompletedTasks()
    {
        var tasks = Player?.Data?.Tasks;
        return tasks == null ? 0 : tasks.ToArray().Count(t => t.Complete);
    }

    [HideFromIl2Cpp]
    private bool CanTeachNow()
    {
        var options = OptionGroupSingleton<MentorOptions>.Instance;

        if (!options.CanTeachRoundOne.Value && meetingCount <= 1)
        {
            return false;
        }

        var tasksNeeded = (int)options.TasksNeededToTeachAgain.Value;
        if (tasksNeeded > 0 && tasksCompletedAtLastLesson >= 0 && CompletedTasks() - tasksCompletedAtLastLesson < tasksNeeded)
        {
            return false;
        }

        return true;
    }

    [HideFromIl2Cpp]
    public void RecordLesson(byte targetId)
    {
        taughtPlayers.Add(targetId);
        tasksCompletedAtLastLesson = CompletedTasks();
    }

    [HideFromIl2Cpp]
    public bool IsExempt(PlayerVoteArea voteArea)
    {
        if (voteArea == null || voteArea.PlayerId == Player.PlayerId || !CanTeachNow())
        {
            return true;
        }

        if (!OptionGroupSingleton<MentorOptions>.Instance.CanTeachOnSamePlayerAgain.Value && taughtPlayers.Contains(voteArea.PlayerId))
        {
            return true;
        }

        var target = GameData.Instance.GetPlayerById(voteArea.PlayerId)?.Object;

        if (target == null || target.HasDied() || target.HasModifier<MentorTaughtModifier>() || target.HasModifier<MentorInsomniaModifier>() || target.HasModifier<BaseRevealModifier>())
        {
            return true;
        }

        return false;
    }

    [HideFromIl2Cpp]
    public void OpenLessonMenu(PlayerVoteArea voteArea, MeetingHud meeting)
    {
        if (meeting.state == MeetingHud.MeetingStates.Discussion || IsExempt(voteArea))
        {
            return;
        }

        if (Minigame.Instance)
        {
            return;
        }

        var student = GameData.Instance.GetPlayerById(voteArea.PlayerId)?.Object;
        if (student == null)
        {
            return;
        }

        var roles = MiscUtils.GetPotentialRoles().Where(IsRoleValid).ToList();
        if (roles.Count == 0)
        {
            return;
        }

        var targetId = student.PlayerId;
        var menu = AmbassadorSelectionMinigame.Create();
        teachMenu = menu;
        menu.Open(roles, role =>
        {
            if (role != null)
            {
                OnRoleSelected(role, targetId);
            }
        });

        Coroutines.Start(CoStyleLessonMenu(menu, roles.Count));
    }

    [HideFromIl2Cpp]
    private static IEnumerator CoStyleLessonMenu(AmbassadorSelectionMinigame menu, int roleCount)
    {
        while (menu != null && (menu.RolesHolder == null || menu.RolesHolder.childCount < roleCount + 1))
        {
            yield return null;
        }

        if (menu == null)
        {
            yield break;
        }

        menu.StatusText.text = "Choose a role";
        menu.RoleName.text = "Mentor";
        menu.RoleTeam.text = "Hover over a role";
        menu.RoleIcon.sprite = DivaniAssets.MentorIcon.LoadAsset();

        foreach (var ring in new[] { menu.RedRing, menu.WarpRing })
        {
            var ringRenderer = ring != null ? ring.GetComponent<SpriteRenderer>() : null;
            if (ringRenderer != null)
            {
                ringRenderer.color = MentorColor;
            }
        }

        var randomCard = menu.RolesHolder.GetChild(menu.RolesHolder.childCount - 1);
        var actualCard = randomCard.GetChild(0);
        var crewColor = (Color)Palette.CrewmateBlue;
        var crewIcon = TouRoleIcons.RandomCrew.LoadAsset();
        const string randomTeam = "Random Crewmate";

        var cardName = actualCard.GetChild(0).GetComponent<TextMeshPro>();
        var cardIcon = actualCard.GetChild(1).GetComponent<SpriteRenderer>();
        var cardTeam = actualCard.GetChild(2).GetComponent<TextMeshPro>();
        var rollover = actualCard.GetComponent<ButtonRolloverHandler>();
        var button = actualCard.GetComponent<PassiveButton>();

        cardName.color = crewColor;
        cardTeam.text = randomTeam;
        cardTeam.color = crewColor;
        cardIcon.sprite = crewIcon;
        rollover.OverColor = crewColor;

        button.OnMouseOver.AddListener((UnityAction)(() =>
        {
            menu.RoleName.text = "Random";
            menu.RoleTeam.text = randomTeam;
            menu.RoleIcon.sprite = crewIcon;
        }));
    }

    [HideFromIl2Cpp]
    public static bool IsRoleValid(RoleBehaviour role)
    {
        if (role is not ITownOfUsRole { Team: ModdedRoleTeams.Crewmate } touRole || role is MentorRole)
        {
            return false;
        }

        if (role.GetRoleAlignment() is RoleAlignment.CrewmateAfterlife || role.GetRoleAlignment() is RoleAlignment.CrewmateHider)
        {
            return false;
        }

        if (role is MayorRole or PoliticianRole or MonarchRole or TimeLordRole)
        {
            return false;
        }

        var options = OptionGroupSingleton<MentorOptions>.Instance;
        return touRole.RoleAlignment switch
        {
            RoleAlignment.CrewmateKilling => options.CanTeachCrewKilling.Value,
            RoleAlignment.CrewmatePower => options.CanTeachCrewPower.Value,
            RoleAlignment.CrewmateInvestigative => options.CanTeachCrewInves.Value,
            _ => true,
        };
    }

    [HideFromIl2Cpp]
    public void OnRoleSelected(RoleBehaviour role, byte targetId)
    {
        var target = GameData.Instance.GetPlayerById(targetId)?.Object;

        if (target == null)
        {
            return;
        }

        teachMenu?.Close();
        teachMenu = null;
        RpcSetLessonTarget(Player, targetId, RoleId.Get(role.GetType()));

        // One lesson per meeting: hide the Teach buttons once a student has been chosen.
        meetingMenu?.HideButtons();
        ShowTaughtIndicator(targetId);
    }

    [HideFromIl2Cpp]
    private static void ShowTaughtIndicator(byte targetId)
    {
        var meeting = MeetingHud.Instance;
        if (meeting == null)
        {
            return;
        }

        var voteArea = meeting.playerStates.FirstOrDefault(state => state.PlayerId == targetId);
        if (voteArea == null)
        {
            return;
        }

        var indicator = new GameObject("MentorTaughtIndicator");
        indicator.layer = voteArea.gameObject.layer;
        indicator.transform.SetParent(voteArea.transform, false);
        indicator.transform.localPosition = TeachButtonPosition;
        indicator.AddComponent<SpriteRenderer>().sprite = DivaniAssets.MentorTaughtMeeting.LoadAsset();
    }

    [MethodRpc((uint)DivaniRpcCalls.MentorSetLessonTarget)]
    public static void RpcSetLessonTarget(PlayerControl Mentor, byte targetId, ushort roleId)
    {
        if (Mentor?.Data?.Role is not MentorRole mentorRole)
        {
            return;
        }

        mentorRole.StudentId = targetId;
        mentorRole.LessonRoleId = roleId;
        mentorRole.RecordLesson(targetId);


        if (Mentor.AmOwner)
        {
            var targetName = GameData.Instance.GetPlayerById(targetId)?.Object?.Data?.PlayerName ?? "them";
            var roleObj = RoleManager.Instance.GetRole((RoleTypes)roleId) as ITownOfUsRole;
            var lessonRole = roleObj?.RoleName ?? "a new role";
            var lessonRoleHex = roleObj != null ? ColorUtility.ToHtmlStringRGB(roleObj.RoleColor) : "9999FF";

            Helpers.CreateAndShowNotification(
                $"<b>You will teach <color=white>{targetName}</color> to be the <color=#{lessonRoleHex}>{lessonRole}</color> next round!</b>",
                MentorColor, spr: DivaniAssets.MentorIcon.LoadAsset()
            );
        }
    }

    [MethodRpc((uint)DivaniRpcCalls.MentorNotifyLessonFailed)]
    public static void RpcNotifyLessonFailed(PlayerControl Mentor, PlayerControl target)
    {
        var options = OptionGroupSingleton<MentorOptions>.Instance;

        if (target != null && target.AmOwner && options.NotifyTargetOnAttempt.Value)
        {
            Helpers.CreateAndShowNotification(
                $"<b>The <color=\"#{MentorColor.ToHtmlStringRGBA()}\">Mentor</color> tried to teach you a lesson but failed!</b>",
                Color.white, spr: DivaniAssets.MentorIcon.LoadAsset());
        }

        if (Mentor != null && Mentor.AmOwner && options.NotifyMentorOnFail.Value)
        {
            Helpers.CreateAndShowNotification(
                $"<b>Your lesson for {target?.Data?.PlayerName ?? "them"} failed!</b>",
                MentorColor, spr: DivaniAssets.MentorIcon.LoadAsset());
        }
    }

    [MethodRpc((uint)DivaniRpcCalls.MentorNotifyLessonRedirected)]
    public static void RpcNotifyLessonRedirected(PlayerControl Mentor, ushort newRoleId)
    {
        if (Mentor == null || !Mentor.AmOwner || !OptionGroupSingleton<MentorOptions>.Instance.NotifyMentorOnFail.Value)
        {
            return;
        }

        var roleObj = RoleManager.Instance.GetRole((RoleTypes)newRoleId) as ITownOfUsRole;
        var roleName = roleObj?.RoleName ?? "a new role";
        var roleHex = roleObj != null ? ColorUtility.ToHtmlStringRGB(roleObj.RoleColor) : "9999FF";

        Helpers.CreateAndShowNotification(
            $"<b>The role you chose was unavailable- your student was taught the <color=#{roleHex}>{roleName}</color> instead!</b>",
            MentorColor, spr: DivaniAssets.MentorIcon.LoadAsset());
    }

    public static bool IsValidLessonTarget(PlayerControl? target, PlayerControl Mentor)
    {
        if (target == null || Mentor == null)
        {
            return false;
        }

        if (target.Data == null || target.Data.Disconnected)
        {
            return false;
        }

        if (target.HasDied() || target.PlayerId == Mentor.PlayerId)
        {
            return false;
        }

        return true;
    }

    public void ClearLesson()
    {
        StudentId = byte.MaxValue;
        LessonRoleId = default;
    }

    [HideFromIl2Cpp]
    public static bool IsBreakingMaxRoleCount(RoleBehaviour role, PlayerControl target)
    {
        if (role is not ICustomRole customRole || customRole.GetCount() is not int cap || cap == 0)
        {
            return false;
        }

        var aliveWithRole = PlayerControl.AllPlayerControls.ToArray()
            .Count(p => p != null && p.Data?.Role != null && p.Data.Role.Role == role.Role && !p.HasDied() && p != target);

        return aliveWithRole >= cap;
    }

    [HideFromIl2Cpp]
    public static RoleBehaviour? GetRandomValidRole(PlayerControl target)
    {
        var pool = MiscUtils.GetPotentialRoles()
            .Where(r => IsRoleValid(r) && !IsBreakingMaxRoleCount(r, target))
            .ToList();

        return pool.Count == 0 ? null : pool[UnityEngine.Random.Range(0, pool.Count)];
    }

    public bool ShowChosenLesson()
    {
        return !(LessonRoleId == default) || !(StudentId == byte.MaxValue);
    }
}
