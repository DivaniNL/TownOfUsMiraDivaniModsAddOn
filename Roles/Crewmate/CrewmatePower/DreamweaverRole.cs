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
using TownOfUs.Extensions;
using TownOfUs.Modules;
using TownOfUs.Modules.Components;
using TownOfUs.Modules.Wiki;
using TownOfUs.Roles;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;
using TownOfUs.Modifiers;
using System.Text;
using Reactor.Utilities.Extensions;

namespace DivaniMods.Roles.Crewmate.CrewmatePower;

public sealed class DreamweaverRole(IntPtr cppPtr)
    : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    private MeetingMenu? meetingMenu;

    private AmbassadorSelectionMinigame? dreamMenu;
    public static readonly Color DreamweaverColor = new Color32(51, 51, 153, 255);
    public string RoleName => "Dreamweaver";
    public string RoleDescription => "Cast Dreams up fellow Crewmates!";
    public string RoleLongDescription => "Cast dreams upon other players to become the roles you desire.\nYour dream fails if it targets a Non-Crewmate.";
    public Color RoleColor => DreamweaverColor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmatePower;

    public override bool IsAffectedByComms => false;

    public DoomableType DoomHintType => DoomableType.Perception;

    public string GetAdvancedDescription() => RoleLongDescription + MiscUtils.AppendOptionsText(GetType());

    public byte DreamTargetId { get; set; } = byte.MaxValue;
    public ushort DreamRoleId { get; set; } = default;

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = DivaniAssets.DreamweaverIcon,
        IconTmp = MiraAPI.Utilities.Assets.TmpSpriteUtils.CreateSpriteAsset(DivaniAssets.DreamweaverIcon.LoadAsset(), "DivaniMod.Role.Crewmate.Dreamweaver", 1.45f),
        IntroSound = DivaniAssets.DreamweaverIntroSound,
        MaxRoleCount = 1
    };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
       var stringB = ITownOfUsRole.SetNewTabText(this);

        stringB.AppendLine($"Current Dreams:");

        if (ShowChosenDream())
        {
            var targetName = GameData.Instance.GetPlayerById(DreamTargetId)?.Object?.Data?.PlayerName;
            var roleObj = RoleManager.Instance.GetRole((RoleTypes)DreamRoleId) as ITownOfUsRole;
            var roleName = roleObj?.RoleName;
            var roleColor = roleObj != null ? ColorUtility.ToHtmlStringRGB(roleObj.RoleColor) : "9999FF";
            stringB.AppendLine($"<b>Dream Target: {targetName}. Dream Role: <color=#{roleColor}>{roleName}</color></b>");
            return stringB;
        }

        return stringB;
    }

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);

        ClearDream();

        if (Player.AmOwner)
        {
            DreamTargetId = byte.MaxValue;

            meetingMenu = new MeetingMenu(
                this,
                OpenDreamMenu,
                "Dream",
                MeetingAbilityType.Click,
                DivaniAssets.DreamweaverMeetingDream,
                exemption: IsExempt,
                position: new Vector3(-0.35f, 0f, -3f));
        }
    }

    public override void OnMeetingStart()
    {
        RoleBehaviourStubs.OnMeetingStart(this);

        ClearDream();

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

            if (dreamMenu != null)
            {
                dreamMenu.Close();
                dreamMenu = null;
            }
        }
    }

    [HideFromIl2Cpp]
    public bool IsExempt(PlayerVoteArea voteArea)
    {
        if (voteArea == null || voteArea.PlayerId == Player.PlayerId)
        {
            return true;
        }

        var target = GameData.Instance.GetPlayerById(voteArea.PlayerId)?.Object;

        if (target == null || target.HasDied() || target.HasModifier<DreamweaverTargetDreamingModifier>() || target.HasModifier<DreamweaverInsomniaModifier>() || target.HasModifier<BaseRevealModifier>())
        {
            return true;
        }

        return false;
    }

    [HideFromIl2Cpp]
    public void OpenDreamMenu(PlayerVoteArea voteArea, MeetingHud meeting)
    {
        if (meeting.state == MeetingHud.MeetingStates.Discussion || IsExempt(voteArea))
        {
            return;
        }

        if (Minigame.Instance)
        {
            return;
        }

        var dreamTarget = GameData.Instance.GetPlayerById(voteArea.PlayerId)?.Object;
        if (dreamTarget == null)
        {
            return;
        }

        var roles = MiscUtils.GetPotentialRoles().Where(IsRoleValid).ToList();
        if (roles.Count == 0)
        {
            return;
        }

        var targetId = dreamTarget.PlayerId;
        var menu = AmbassadorSelectionMinigame.Create();
        dreamMenu = menu;
        menu.Open(roles, role =>
        {
            if (role != null)
            {
                OnRoleSelected(role, targetId);
            }
        });

        Coroutines.Start(CoStyleDreamMenu(menu, roles.Count));
    }

    [HideFromIl2Cpp]
    private static IEnumerator CoStyleDreamMenu(AmbassadorSelectionMinigame menu, int roleCount)
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
        menu.RoleName.text = "Dreamweaver";
        menu.RoleTeam.text = "Hover over a role";
        menu.RoleIcon.sprite = DivaniAssets.DreamweaverIcon.LoadAsset();

        foreach (var ring in new[] { menu.RedRing, menu.WarpRing })
        {
            var ringRenderer = ring != null ? ring.GetComponent<SpriteRenderer>() : null;
            if (ringRenderer != null)
            {
                ringRenderer.color = DreamweaverColor;
            }
        }
        menu.RolesHolder.GetChild(menu.RolesHolder.childCount - 1).gameObject.SetActive(false);
    }

    [HideFromIl2Cpp]
    public static bool IsRoleValid(RoleBehaviour role)
    {
        if (role is not ITownOfUsRole { Team: ModdedRoleTeams.Crewmate } touRole || role is DreamweaverRole)
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

        var options = OptionGroupSingleton<DreamweaverOptions>.Instance;
        return touRole.RoleAlignment switch
        {
            RoleAlignment.CrewmateKilling => options.CanCastCrewKilling.Value,
            RoleAlignment.CrewmatePower => options.CanCastCrewPower.Value,
            RoleAlignment.CrewmateInvestigative => options.CanCastCrewInves.Value,
            _ => true,
        };
    }

    [HideFromIl2Cpp]
    public void OnRoleSelected(RoleBehaviour role, byte targetId)
    {
        var options = OptionGroupSingleton<DreamweaverOptions>.Instance;

        var target = GameData.Instance.GetPlayerById(targetId)?.Object;

        if (target == null)
        {
            return;
        }

        dreamMenu?.Close();
        dreamMenu = null;
        RpcSetReimagineTarget(Player, targetId, RoleId.Get(role.GetType()));
    }

    [MethodRpc((uint)DivaniRpcCalls.DreamweaverSetReimagineTarget)]
    public static void RpcSetReimagineTarget(PlayerControl Dreamweaver, byte targetId, ushort roleId)
    {
        if (Dreamweaver?.Data?.Role is not DreamweaverRole DreamweaverRole)
        {
            return;
        }

        DreamweaverRole.DreamTargetId = targetId;
        DreamweaverRole.DreamRoleId = roleId;


        if (Dreamweaver.AmOwner)
        {
            var targetName = GameData.Instance.GetPlayerById(targetId)?.Object?.Data?.PlayerName ?? "them";
            var roleObj = RoleManager.Instance.GetRole((RoleTypes)roleId) as ITownOfUsRole;
            var dreamRole = roleObj?.RoleName ?? "a new role";
            var dreamRoleHex = roleObj != null ? ColorUtility.ToHtmlStringRGB(roleObj.RoleColor) : "9999FF";

            Helpers.CreateAndShowNotification(
                $"<b>You will cast a dream upon <color=white>{targetName}</color> to be the <color=#{dreamRoleHex}>{dreamRole}</color> next round!</b>",
                DreamweaverColor, spr: DivaniAssets.DreamweaverIcon.LoadAsset()
            );
        }
    }

    [MethodRpc((uint)DivaniRpcCalls.DreamweaverNotifyDreamFailed)]
    public static void RpcNotifyDreamFailed(PlayerControl Dreamweaver, PlayerControl target)
    {
        var options = OptionGroupSingleton<DreamweaverOptions>.Instance;

        if (target != null && target.AmOwner && options.NotifyTargetOnAttempt.Value)
        {
            Helpers.CreateAndShowNotification(
                $"<b>The <color=\"#{DreamweaverColor.ToHtmlStringRGBA()}\">Dreamweaver</color> tried to Cast a spell on you but failed!</b>",
                Color.white, spr: DivaniAssets.DreamweaverIcon.LoadAsset());
        }

        if (Dreamweaver != null && Dreamweaver.AmOwner && options.NotifyDreamweaverOnFail.Value)
        {
            Helpers.CreateAndShowNotification(
                $"<b>Your casted dream on {target?.Data?.PlayerName ?? "them"} failed!</b>",
                DreamweaverColor, spr: DivaniAssets.DreamweaverIcon.LoadAsset());
        }
    }

    [MethodRpc((uint)DivaniRpcCalls.DreamweaverNotifyDreamRedirected)]
    public static void RpcNotifyDreamRedirected(PlayerControl Dreamweaver, ushort newRoleId)
    {
        if (Dreamweaver == null || !Dreamweaver.AmOwner || !OptionGroupSingleton<DreamweaverOptions>.Instance.NotifyDreamweaverOnFail.Value)
        {
            return;
        }

        var roleObj = RoleManager.Instance.GetRole((RoleTypes)newRoleId) as ITownOfUsRole;
        var roleName = roleObj?.RoleName ?? "a new role";
        var roleHex = roleObj != null ? ColorUtility.ToHtmlStringRGB(roleObj.RoleColor) : "9999FF";

        Helpers.CreateAndShowNotification(
            $"<b>The role you chose was unavailable- your dreamt role became the <color=#{roleHex}>{roleName}</color>!</b>",
            DreamweaverColor, spr: DivaniAssets.DreamweaverIcon.LoadAsset());
    }

    public static bool IsValidDreamTarget(PlayerControl? target, PlayerControl Dreamweaver)
    {
        if (target == null || Dreamweaver == null)
        {
            return false;
        }

        if (target.Data == null || target.Data.Disconnected)
        {
            return false;
        }

        if (target.HasDied() || target.PlayerId == Dreamweaver.PlayerId)
        {
            return false;
        }

        return true;
    }

    public void ClearDream()
    {
        DreamTargetId = byte.MaxValue;
        DreamRoleId = default;
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

    public bool ShowChosenDream()
    {
        return !(DreamRoleId == default) || !(DreamTargetId == byte.MaxValue);
    }
}
