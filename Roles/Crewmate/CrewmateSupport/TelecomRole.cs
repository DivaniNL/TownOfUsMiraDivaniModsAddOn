using System;
using System.Linq;
using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Utilities.Extensions;
using DivaniMods.Assets;
using DivaniMods.Modifiers.Crewmate.CrewmateSupport;
using DivaniMods.Options;
using TownOfUs;
using TownOfUs.Assets;
using TownOfUs.Extensions;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modules;
using TownOfUs.Modules.Wiki;
using TownOfUs.Options;
using TownOfUs.Patches.Options;
using TownOfUs.Roles;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace DivaniMods.Roles.Crewmate.CrewmateSupport;

public sealed class TelecomRole(IntPtr cppPtr)
    : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public static readonly Color TelecomColor = new Color32(0x8E, 0xEF, 0xFF, 255);

    public string RoleName => "Telecom";
    public string RoleDescription => "Spead the word!";
    public string RoleLongDescription =>
        "Transmit to a player to open a private chat!";
    public Color RoleColor => TelecomColor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateSupport;

    public DoomableType DoomHintType => DoomableType.Trickster;

    public string GetAdvancedDescription() => RoleLongDescription + MiscUtils.AppendOptionsText(GetType());

    [HideFromIl2Cpp] public List<CustomButtonWikiDescription> Abilities { get; } =
    [
        new("Transmission", "Open a private chat with a chosen player.", DivaniAssets.TelecomTransmissionButton)
    ];

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = DivaniAssets.TelecomIcon,
        IntroSound = DivaniAssets.TelecomIntroSound,
        MaxRoleCount = 1,
    };

    public byte TargetId { get; set; } = byte.MaxValue;
    public byte PendingMeetingTargetId { get; set; } = byte.MaxValue;
    public bool TransmittedThisRound { get; set; }

    private MeetingMenu? _meetingMenu;
    private byte _localSelectedId = byte.MaxValue;

    private static bool MeetingMode =>
        OptionGroupSingleton<TelecomOptions>.Instance.TargetSelection.Value ==
        (int)TelecomTargetSelectionOptions.Meeting;

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var sb = ITownOfUsRole.SetNewTabText(this);
        var target = TargetId != byte.MaxValue ? GameData.Instance.GetPlayerById(TargetId)?.Object : null;
        if (target != null && target.Data != null)
        {
            sb.AppendLine($"{RoleColor.ToTextColor()}<b>Transmitting: {target.Data.PlayerName}</b></color>");
        }
        else
        {
            sb.AppendLine($"{RoleColor.ToTextColor()}<b>No active transmission</b></color>");
        }

        return sb;
    }

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);

        TargetId = byte.MaxValue;
        PendingMeetingTargetId = byte.MaxValue;
        TransmittedThisRound = false;
        _localSelectedId = byte.MaxValue;

        if (Player.AmOwner && MeetingMode)
        {
            _meetingMenu = new MeetingMenu(
                this,
                OnMeetingToggle,
                MeetingAbilityType.Toggle,
                DivaniAssets.TelecomTransmissionMeeting,
                DivaniAssets.TelecomTransmissionMeeting,
                IsExempt,
                activeColor: TelecomColor,
                disabledColor: Color.gray,
                hoverColor: Color.white)
            {
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

        _localSelectedId = byte.MaxValue;

        var usable = !Player.HasDied() && !Player.HasModifier<JailedModifier>() && !TransmittedThisRound;
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
               target.PlayerId == TargetId ||
               target.HasModifier<JailedModifier>();
    }

    private void OnMeetingToggle(PlayerVoteArea voteArea, MeetingHud hud)
    {
        if (hud.state == MeetingHud.VoteStates.Discussion || IsExempt(voteArea) || _meetingMenu == null)
        {
            return;
        }

        if (_localSelectedId == voteArea.TargetPlayerId)
        {
            _meetingMenu.Actives[voteArea.TargetPlayerId] = false;
            _localSelectedId = byte.MaxValue;
            RpcSetPendingMeetingTarget(Player, byte.MaxValue);
            return;
        }

        if (_localSelectedId != byte.MaxValue)
        {
            _meetingMenu.Actives[_localSelectedId] = false;
        }

        _localSelectedId = voteArea.TargetPlayerId;
        _meetingMenu.Actives[voteArea.TargetPlayerId] = true;
        SoundManager.Instance.PlaySound(DivaniAssets.TelecomTransmissionSound.LoadAsset(), false);
        RpcSetPendingMeetingTarget(Player, voteArea.TargetPlayerId);
    }

    public static bool IsValidTarget(PlayerControl? target, PlayerControl telecom)
    {
        if (target == null || telecom == null || target.Data == null)
        {
            return false;
        }

        if (target.Data.IsDead || target.Data.Disconnected || target.PlayerId == telecom.PlayerId)
        {
            return false;
        }

        return true;
    }

    private static void ClearLink(PlayerControl telecom)
    {
        foreach (var pc in PlayerControl.AllPlayerControls.ToArray())
        {
            if (pc == null || !pc.HasModifier<TelecomChatModifier>())
            {
                continue;
            }

            var mod = pc.GetModifier<TelecomChatModifier>();
            if (mod == null)
            {
                continue;
            }

            if (pc.PlayerId == telecom.PlayerId || (mod.Partner != null && mod.Partner.PlayerId == telecom.PlayerId))
            {
                pc.RemoveModifier<TelecomChatModifier>();
            }
        }
    }

    [MethodRpc((uint)DivaniRpcCalls.TelecomSetTransmission)]
    public static void RpcSetTransmission(PlayerControl telecom, byte targetId)
    {
        if (telecom?.Data?.Role is not TelecomRole role)
        {
            return;
        }

        var existing = telecom.GetModifier<TelecomChatModifier>();
        var oldPartner = existing?.Partner;
        if (oldPartner != null && oldPartner.PlayerId != targetId)
        {
            oldPartner.RemoveModifier<TelecomChatModifier>();
        }

        var target = targetId != byte.MaxValue ? GameData.Instance.GetPlayerById(targetId)?.Object : null;
        if (!IsValidTarget(target, telecom))
        {
            role.TargetId = byte.MaxValue;
            if (existing != null)
            {
                telecom.RemoveModifier<TelecomChatModifier>();
            }

            return;
        }

        role.TargetId = targetId;

        var telMod = existing ?? telecom.AddModifier<TelecomChatModifier>();
        if (telMod != null)
        {
            telMod.AmTelecom = true;
            telMod.Partner = target;
        }

        var tgtMod = target!.GetModifier<TelecomChatModifier>() ?? target!.AddModifier<TelecomChatModifier>();
        if (tgtMod != null)
        {
            tgtMod.AmTelecom = false;
            tgtMod.Partner = telecom;
        }
    }

    [MethodRpc((uint)DivaniRpcCalls.TelecomClearTransmission)]
    public static void RpcClearTransmission(PlayerControl telecom)
    {
        if (telecom?.Data?.Role is TelecomRole role)
        {
            role.TargetId = byte.MaxValue;
        }

        if (telecom != null)
        {
            ClearLink(telecom);
        }
    }

    [MethodRpc((uint)DivaniRpcCalls.TelecomSetPendingMeetingTarget)]
    public static void RpcSetPendingMeetingTarget(PlayerControl telecom, byte targetId)
    {
        if (telecom?.Data?.Role is TelecomRole role)
        {
            role.PendingMeetingTargetId = targetId;
        }
    }

    [MethodRpc((uint)DivaniRpcCalls.TelecomSendChat)]
    public static void RpcSendTelecomChat(PlayerControl sender, string text)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(sender);
            return;
        }

        if (sender == null || !sender.HasModifier<TelecomChatModifier>())
        {
            return;
        }

        var mod = sender.GetModifier<TelecomChatModifier>();
        if (mod == null)
        {
            return;
        }

        var partner = mod.Partner;
        var local = PlayerControl.LocalPlayer;
        var localInvolved = local == sender || (partner != null && local == partner);
        var deadKnows = DeathHandlerModifier.IsFullyDead(local) &&
                        OptionGroupSingleton<PostmortemOptions>.Instance.TheDeadKnow;

        if (!localInvolved && !deadKnows)
        {
            return;
        }

        var anonymous = OptionGroupSingleton<TelecomOptions>.Instance.Anonymous.Value;

        var hideSender = anonymous && mod.AmTelecom && local != sender;
        var basePlayer = hideSender ? local.Data : sender.Data;
        var displayName = hideSender ? "Telecom" : sender.Data.PlayerName;

        var chat = HudManager.Instance.Chat;
        var originalSound = chat.messageSound;
        chat.messageSound = SilentClip;

        MiscUtils.AddTeamChat(basePlayer,
            $"<color=#{TelecomColor.ToHtmlStringRGBA()}>{displayName} (Telecom chat)</color>",
            text, blackoutText: false, bubbleType: BubbleType.None, onLeft: !sender.AmOwner);

        chat.messageSound = originalSound;

        if (TeamChatPatches.PrivateChatDot != null)
        {
            TeamChatPatches.PrivateChatDot.sprite = DivaniAssets.TelecomChatBubble.LoadAsset();
        }

        if (local != sender)
        {
            SoundManager.Instance.PlaySound(DivaniAssets.TelecomMessageSound.LoadAsset(), false);
        }
    }

    private static AudioClip? _silentClip;

    private static AudioClip SilentClip => _silentClip ??= AudioClip.Create("TelecomSilent", 1, 1, 44100, false);
}
