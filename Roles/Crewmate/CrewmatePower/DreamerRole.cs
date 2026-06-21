using DivaniMods.Assets;
using DivaniMods.Modifiers.Crewmate.CrewmatePower;
using DivaniMods.Options;
using Il2CppInterop.Runtime.Attributes;
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
using TownOfUs.Utilities;
using UnityEngine;

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

        return target == null
            || target.HasDied()
            || target.HasModifier<DreamerTargetDreamingModifier>()
            || target.HasModifier<DreamerInsomniaModifier>();
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
    }

    [MethodRpc((uint)DivaniRpcCalls.DreamerNotifyDreamFailed)]
    public static void RpcNotifyDreamFailed(PlayerControl dreamer, PlayerControl target)
    {
        var options = OptionGroupSingleton<DreamerOptions>.Instance;

        if (target != null && target.AmOwner && options.NotifyNonCrewOnAttempt.Value)
        {
            Helpers.CreateAndShowNotification(
                "<b>The Dreamer tried to <color=#804D19>reimagine</color> you but failed!</b>",
                Color.white, spr: DivaniAssets.DreamerIcon.LoadAsset());
        }

        if (dreamer != null && dreamer.AmOwner && options.NotifyDreamerOnFail.Value)
        {
            Helpers.CreateAndShowNotification(
                $"<b>Your dream on {target?.Data?.PlayerName ?? "them"} failed! They are not a Crew Role!</b>",
                Color.white, spr: DivaniAssets.DreamerIcon.LoadAsset());
        }
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
}
