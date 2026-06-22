using AmongUs.GameOptions;
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

namespace DivaniMods.Roles.Crewmate.CrewmatePower;

public sealed class DreamerRole(IntPtr cppPtr)
    : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    private MeetingMenu? meetingMenu;

    private GuesserMenu? dreamMenu;

    public string RoleName => "Dreamer";
    public string RoleDescription => "Reimagine fellow Crewmates!";
    public string RoleLongDescription => "Dream other players to become the roles you desire. Your dream fails if it targets a Non-Crewmate.";
    public Color RoleColor => new Color32(51, 51, 153, 255);
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmatePower;

    public override bool IsAffectedByComms => false;

    public DoomableType DoomHintType => DoomableType.Perception;

    public string GetAdvancedDescription() => RoleLongDescription + MiscUtils.AppendOptionsText(GetType());

    public byte DreamTargetId { get; set; } = byte.MaxValue;
    public ushort DreamRole { get; set; }

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = DivaniAssets.DreamerIcon,
        IntroSound = DivaniAssets.DreamerIntroSound,
    };

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);

        if (Player.AmOwner)
        {
            DreamTargetId = byte.MaxValue;

            meetingMenu = new MeetingMenu(
                this,
                OpenDreamMenu,
                "Dream",
                MeetingAbilityType.Click,
                DivaniAssets.DreamerMeetingDream,
                exemption: IsExempt,
                position: new Vector3(-0.35f, 0f, -3f));
        }
    }

    public override void OnMeetingStart()
    {
        RoleBehaviourStubs.OnMeetingStart(this);

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
        }
    }

    [HideFromIl2Cpp]
    public bool IsExempt(PlayerVoteArea voteArea)
    {
        if (voteArea == null || voteArea.TargetPlayerId == Player.PlayerId)
        {
            return true;
        }

        var target = GameData.Instance.GetPlayerById(voteArea.TargetPlayerId)?.Object;

        if (target == null || target.HasDied() || target.HasModifier<DreamerTargetDreamingModifier>() || target.HasModifier<DreamerInsomniaModifier>() || target.HasModifier<BaseRevealModifier>())
        {
            return true;
        }

        return false;
    }

    [HideFromIl2Cpp]
    public void OpenDreamMenu(PlayerVoteArea voteArea, MeetingHud meeting)
    {
        if (meeting.state == MeetingHud.VoteStates.Discussion || IsExempt(voteArea))
        {
            return;
        }

        if (Minigame.Instance)
        {
            return;
        }

        var dreamTarget = GameData.Instance.GetPlayerById(voteArea.TargetPlayerId)?.Object;
        if (dreamTarget == null)
        {
            return;
        }

        dreamMenu = GuesserMenu.Create();
        dreamMenu.Begin(IsRoleValid, role => OnRoleSelected(role, dreamTarget.PlayerId));
    }

    [HideFromIl2Cpp]
    public static bool IsRoleValid(RoleBehaviour role)
    {
        if (role is not ITownOfUsRole { Team: ModdedRoleTeams.Crewmate } touRole || role is DreamerRole)
        {
            return false;
        }

        if (role is MayorRole or PoliticianRole or MonarchRole or TimeLordRole)
        {
            return false;
        }

        var restriction = (DreamerReimagineRestriction)OptionGroupSingleton<DreamerOptions>.Instance.CannotReimagineInto.Value;
        return restriction switch
        {
            DreamerReimagineRestriction.CrewmateKilling => touRole.RoleAlignment != RoleAlignment.CrewmateKilling,
            DreamerReimagineRestriction.CrewmatePower => touRole.RoleAlignment != RoleAlignment.CrewmatePower,
            _ => true,
        };
    }

    [HideFromIl2Cpp]
    public void OnRoleSelected(RoleBehaviour role, byte targetId)
    {
        var options = OptionGroupSingleton<DreamerOptions>.Instance;

        if (options.RespectMaxRoleCount.Value
            && (DreamerOnDreamBreakMaxRoleCount)options.OnMaxRoleCountBroken.Value == DreamerOnDreamBreakMaxRoleCount.DreamRedo
            && IsBreakingMaxRoleCount(role))
        {
            Helpers.CreateAndShowNotification(
                "<b>That role is already maxed out — <color=#9999FF>choose another role!</color></b>",
                new Color32(51, 51, 153, 255), spr: DivaniAssets.DreamerIcon.LoadAsset());

            dreamMenu?.Close();
            dreamMenu = GuesserMenu.Create();
            dreamMenu.Begin(IsRoleValid, newRole => OnRoleSelected(newRole, targetId));
            return;
        }

        dreamMenu?.Close();
        RpcSetReimagineTarget(Player, targetId, RoleId.Get(role.GetType()));
    }

    [MethodRpc((uint)DivaniRpcCalls.DreamerSetReimagineTarget)]
    public static void RpcSetReimagineTarget(PlayerControl dreamer, byte targetId, ushort roleId)
    {
        if (dreamer?.Data?.Role is not DreamerRole dreamerRole)
        {
            return;
        }

        dreamerRole.DreamTargetId = targetId;
        dreamerRole.DreamRole = roleId;


        if (dreamer.AmOwner)
        {
            var targetName = GameData.Instance.GetPlayerById(targetId)?.Object?.Data?.PlayerName ?? "them";
            var roleObj = RoleManager.Instance.GetRole((RoleTypes)roleId) as ITownOfUsRole;
            var dreamRole = roleObj?.RoleName ?? "a new role";
            var dreamRoleHex = roleObj != null ? ColorUtility.ToHtmlStringRGB(roleObj.RoleColor) : "9999FF";

            Helpers.CreateAndShowNotification(
                $"<b>You will reimagine <color=white>{targetName}</color> as the <color=#{dreamRoleHex}>{dreamRole}</color> in the next meeting!</b>",
                new Color32(51, 51, 153, 255), spr: DivaniAssets.DreamerIcon.LoadAsset()
            );
        }
    }

    [MethodRpc((uint)DivaniRpcCalls.DreamerNotifyDreamFailed)]
    public static void RpcNotifyDreamFailed(PlayerControl dreamer, PlayerControl target)
    {
        var options = OptionGroupSingleton<DreamerOptions>.Instance;

        if (target != null && target.AmOwner && options.NotifyNonCrewOnAttempt.Value)
        {
            Helpers.CreateAndShowNotification(
                "<b>The Dreamer tried to <color=white>reimagine</color> you but failed!</b>",
                new Color32(51, 51, 153, 255), spr: DivaniAssets.DreamerIcon.LoadAsset());
        }

        if (dreamer != null && dreamer.AmOwner && options.NotifyDreamerOnFail.Value)
        {
            Helpers.CreateAndShowNotification(
                $"<b>Your dream on {target?.Data?.PlayerName ?? "them"} failed!</b>",
                new Color32(51, 51, 153, 255), spr: DivaniAssets.DreamerIcon.LoadAsset());
        }
    }

    [MethodRpc((uint)DivaniRpcCalls.DreamerNotifyDreamRedirected)]
    public static void RpcNotifyDreamRedirected(PlayerControl dreamer, ushort newRoleId)
    {
        if (dreamer == null || !dreamer.AmOwner || !OptionGroupSingleton<DreamerOptions>.Instance.NotifyDreamerOnFail.Value)
        {
            return;
        }

        var roleObj = RoleManager.Instance.GetRole((RoleTypes)newRoleId) as ITownOfUsRole;
        var roleName = roleObj?.RoleName ?? "a new role";
        var roleHex = roleObj != null ? ColorUtility.ToHtmlStringRGB(roleObj.RoleColor) : "9999FF";

        Helpers.CreateAndShowNotification(
            $"<b>The role you choose was unavailable- your dream role became the <color=#{roleHex}>{roleName}</color>!</b>",
            new Color32(51, 51, 153, 255), spr: DivaniAssets.DreamerIcon.LoadAsset());
    }

    public static bool IsValidDreamTarget(PlayerControl? target, PlayerControl dreamer)
    {
        if (target == null || dreamer == null)
        {
            return false;
        }

        if (target.Data == null || target.Data.Disconnected)
        {
            return false;
        }

        if (target.HasDied() || target.PlayerId == dreamer.PlayerId)
        {
            return false;
        }

        return true;
    }

    public void ClearDream()
    {
        DreamTargetId = byte.MaxValue;
        DreamRole = default;
    }

    [HideFromIl2Cpp]
    public static bool IsBreakingMaxRoleCount(RoleBehaviour role)
    {
        if (role is not ICustomRole customRole || customRole.GetCount() is not int cap || cap == 0)
        {
            return false;
        }

        var aliveWithRole = PlayerControl.AllPlayerControls.ToArray()
            .Count(p => p != null && p.Data?.Role != null && p.Data.Role.Role == role.Role && !p.HasDied());

        return aliveWithRole >= cap;
    }

    [HideFromIl2Cpp]
    public static RoleBehaviour? GetRandomValidRole()
    {
        var pool = MiscUtils.GetPotentialRoles()
            .Where(r => IsRoleValid(r) && !IsBreakingMaxRoleCount(r))
            .ToList();

        return pool.Count == 0 ? null : pool[UnityEngine.Random.Range(0, pool.Count)];
    }
}
