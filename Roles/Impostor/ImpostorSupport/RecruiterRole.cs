using System;
using AmongUs.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using DivaniMods.Assets;
using DivaniMods.Patches;
using TownOfUs.Extensions;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modules;
using TownOfUs.Modules.Wiki;
using TownOfUs.Roles;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace DivaniMods.Roles.Impostor.ImpostorSupport;

public sealed class RecruiterRole(IntPtr cppPtr)
    : ImpostorRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable, ICrewVariant
{
    public string RoleName => "Recruiter";
    public string RoleDescription => "Recruit a non-Impostor during the first meeting!";
    public string RoleLongDescription =>
        "During the first meeting only, tap the crewmate icon on each nameplate to mark who you recruit.\n" +
        "Your choice shows as the red Impostor icon. Only one player can be marked.\n" +
        "When the meeting ends, they become a vanilla Impostor.";
    public Color RoleColor => Palette.ImpostorRed;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorSupport;

    public DoomableType DoomHintType => DoomableType.Insight;

    public RoleBehaviour CrewVariant =>
        RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<EngineerTouRole>());

    public string GetAdvancedDescription() => RoleLongDescription + MiscUtils.AppendOptionsText(GetType());

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = DivaniAssets.RecruiterIcon,
        IntroSound = DivaniAssets.SilencerIntroSound,
        MaxRoleCount = 1,
    };

    /// <summary>Synced selection for the host to read when the first meeting closes (255 = none).</summary>
    public byte PendingRecruitTargetId { get; set; } = 255;

    private MeetingMenu? _meetingMenu;
    private byte _localSelectedId = 255;

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        PendingRecruitTargetId = 255;
        _localSelectedId = 255;

        if (Player.AmOwner)
        {
            // MeetingMenu defaults activeColor to green for Toggle — pass white so the imp
            // sprite is not tinted. Same for hover (default red) and disabled.
            _meetingMenu = new MeetingMenu(
                this,
                OnMeetingToggle,
                MeetingAbilityType.Toggle,
                DivaniAssets.RecruitMeetingImpostor,
                DivaniAssets.RecruitMeetingCrewmate,
                IsExempt,
                activeColor: Color.white,
                disabledColor: Color.white,
                hoverColor: Color.white)
            {
                // Offset only — on-screen size comes from sprite PPU/bounds, not this vector.
                Position = new Vector3(-0.40f, 0f, -3f),
            };
        }
    }

    public override void OnMeetingStart()
    {
        RoleBehaviourStubs.OnMeetingStart(this);

        if (!Player.AmOwner || _meetingMenu == null)
        {
            return;
        }

        var firstMeeting = RecruiterPatch.MeetingsEnded == 0;
        var usable = firstMeeting &&
                       !Player.HasDied() &&
                       !Player.HasModifier<JailedModifier>();
        var hud = MeetingHud.Instance;
        if (hud != null)
        {
            _meetingMenu.GenButtons(hud, usable);
        }
    }

    public override void OnVotingComplete()
    {
        RoleBehaviourStubs.OnVotingComplete(this);

        if (Player.AmOwner && _meetingMenu != null)
        {
            _meetingMenu.HideButtons();
        }
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);

        if (Player.AmOwner)
        {
            _meetingMenu?.Dispose();
            _meetingMenu = null;
        }
    }

    private bool IsExempt(PlayerVoteArea voteArea)
    {
        var target = GameData.Instance.GetPlayerById(voteArea.TargetPlayerId)?.Object;
        return target == null ||
               target.Data == null ||
               target.Data.Disconnected ||
               target.Data.IsDead ||
               target.PlayerId == Player.PlayerId ||
               target.Data.Role is ImpostorRole ||
               target.HasModifier<JailedModifier>();
    }

    private void OnMeetingToggle(PlayerVoteArea voteArea, MeetingHud hud)
    {
        if (hud.state == MeetingHud.VoteStates.Discussion || IsExempt(voteArea))
        {
            return;
        }

        if (_meetingMenu == null)
        {
            return;
        }

        if (_localSelectedId == voteArea.TargetPlayerId)
        {
            _meetingMenu.Actives[voteArea.TargetPlayerId] = false;
            _localSelectedId = 255;
            RpcSetPendingTarget(Player, 255);
            return;
        }

        if (_localSelectedId != 255)
        {
            _meetingMenu.Actives[_localSelectedId] = false;
        }

        _localSelectedId = voteArea.TargetPlayerId;
        _meetingMenu.Actives[voteArea.TargetPlayerId] = true;
        RpcSetPendingTarget(Player, voteArea.TargetPlayerId);
    }

    [MethodRpc((uint)DivaniRpcCalls.RecruiterSetPendingTarget)]
    public static void RpcSetPendingTarget(PlayerControl recruiter, byte targetPlayerId)
    {
        if (recruiter?.Data?.Role is not RecruiterRole role)
        {
            return;
        }

        role.PendingRecruitTargetId = targetPlayerId;
    }

    internal static bool IsValidRecruitTarget(PlayerControl? target, PlayerControl recruiter)
    {
        if (target == null || recruiter == null)
        {
            return false;
        }

        if (target.Data == null || target.Data.IsDead || target.Data.Disconnected)
        {
            return false;
        }

        if (target.PlayerId == recruiter.PlayerId)
        {
            return false;
        }

        return target.Data.Role is not ImpostorRole;
    }
}
