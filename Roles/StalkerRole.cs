using System;
using System.Collections.Generic;
using System.Linq;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using DivaniMods.Assets;
using DivaniMods.Buttons;
using DivaniMods.Options;
using DivaniMods.Patches;
using TownOfUs.Interfaces;
using TownOfUs.Modifiers.Game.Alliance;
using TownOfUs.Modules;
using TownOfUs.Modules.Wiki;
using TownOfUs.Roles;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;

namespace DivaniMods.Roles;

public sealed class StalkerRole(IntPtr cppPtr) : NeutralRole(cppPtr), ITownOfUsRole, IUnlovable, IWikiDiscoverable
{
    public static readonly Color StalkerColor = new Color32(255, 0, 149, 255);
    public bool IsUnlovable => true;

    public string RoleName => MiraLocaleManager.Get("DivaniMods.Role.Stalker", "Stalker");
    public string RoleDescription => MiraLocaleManager.Get("DivaniMods.Role.Stalker.Description");
    public string RoleMedDescription => MiraLocaleManager.Get("DivaniMods.Role.Stalker.MedDescription");
    public string RoleLongDescription => MiraLocaleManager.Get("DivaniMods.Role.Stalker.LongDescription");
    public Color RoleColor => StalkerColor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleAlignment RoleAlignment => RoleAlignment.NeutralEvil;

    public string GetAdvancedDescription() => RoleLongDescription + MiscUtils.AppendOptionsText(GetType());

    [HideFromIl2Cpp] public List<CustomButtonWikiDescription> Abilities { get; } =
    [
        new(
            MiraLocaleManager.Get("DivaniMods.Role.Stalker.Ability.Guess"),
            MiraLocaleManager.Get("DivaniMods.Role.Stalker.Ability.Guess.Description"),
            DivaniAssets.StalkerIcon
        ),
        new(
            MiraLocaleManager.Get("DivaniMods.Role.Stalker.Ability.LoverChat"),
            MiraLocaleManager.Get("DivaniMods.Role.Stalker.Ability.LoverChat.Description"),
            DivaniAssets.StalkerIcon
        )
    ];

    public CustomRoleConfiguration Configuration => new(this)
    {
        TasksCountForProgress = false,
        MaxRoleCount = 1,
        Icon = DivaniAssets.StalkerIcon,
        IconTmp = MiraAPI.Utilities.Assets.TmpSpriteUtils.CreateSpriteAsset(
            DivaniAssets.StalkerIcon.LoadAsset(), "DivaniMod.Role.Neutral.Stalker", 1.45f),
        IntroSound = DivaniAssets.StalkerIntroSound,
    };

    [HideFromIl2Cpp] public PlayerVoteArea? Guess1 { get; set; }
    [HideFromIl2Cpp] public PlayerVoteArea? Guess2 { get; set; }
    public bool ResolvedThisMeeting { get; set; }
    private MeetingMenu? _meetingMenu;
    public static bool TriggerStalkerWin { get; set; }
    public static PlayerControl? WinningStalker { get; set; }
    public static PlayerControl? PendingWinStalker { get; set; }

    public static void ClearAndReload()
    {
        TriggerStalkerWin = false;
        WinningStalker = null;
        PendingWinStalker = null;
    }

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);

        Guess1 = null;
        Guess2 = null;
        ResolvedThisMeeting = false;

        if (Player.AmOwner)
        {
            _meetingMenu = new MeetingMenu(
                this,
                OnHeartClicked,
                MeetingAbilityType.Toggle,
                DivaniAssets.StalkerIntuitButton,
                DivaniAssets.StalkerGazeButton,
                IsExempt,
                activeColor: Color.white,
                disabledColor: Color.white,
                hoverColor: Color.white)
            {
                Position = new Vector3(-0.40f, 0f, -3f),
            };

            StalkerChatVisibility.ApplyLoverChatUi();
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

    public override void OnMeetingStart()
    {
        RoleBehaviourStubs.OnMeetingStart(this);

        Guess1 = null;
        Guess2 = null;
        ResolvedThisMeeting = false;

        if (Player.AmOwner)
        {
            _meetingMenu?.GenButtons(MeetingHud.Instance, !Player.HasDied());

            StalkerChatVisibility.ApplyNormalChatUi();
        }
    }

    public override void OnVotingComplete()
    {
        RoleBehaviourStubs.OnVotingComplete(this);

        if (Player.AmOwner)
        {
            _meetingMenu?.HideButtons();
        }
    }

    private static bool IsExempt(PlayerVoteArea voteArea)
    {
        var player = GameData.Instance.GetPlayerById(voteArea.PlayerId)?.Object;
        return !player || player!.Data == null || player.Data.Disconnected || player.Data.IsDead;
    }

    private void OnHeartClicked(PlayerVoteArea voteArea, MeetingHud meeting)
    {
        if (meeting.state == MeetingHud.MeetingStates.Discussion || IsExempt(voteArea))
        {
            return;
        }

        if (!Guess1)
        {
            Guess1 = voteArea;
            _meetingMenu!.Actives[voteArea.PlayerId] = true;
        }
        else if (!Guess2)
        {
            Guess2 = voteArea;
            _meetingMenu!.Actives[voteArea.PlayerId] = true;
        }
        else if (Guess1 == voteArea)
        {
            _meetingMenu!.Actives[Guess1!.PlayerId] = false;
            Guess1 = null;
        }
        else if (Guess2 == voteArea)
        {
            _meetingMenu!.Actives[Guess2.PlayerId] = false;
            Guess2 = null;
        }
        else
        {
            _meetingMenu!.Actives[Guess1!.PlayerId] = false;
            Guess1 = Guess2;
            Guess2 = voteArea;
            _meetingMenu!.Actives[voteArea.PlayerId] = true;
        }

        RpcSyncGuesses(Player, Guess1?.PlayerId ?? byte.MaxValue, Guess2?.PlayerId ?? byte.MaxValue);
    }

    [MethodRpc((uint)DivaniRpcCalls.StalkerSyncGuesses)]
    public static void RpcSyncGuesses(PlayerControl stalker, byte guess1, byte guess2)
    {
        if (MeetingHud.Instance == null)
        {
            return;
        }

        if (stalker?.Data?.Role is not StalkerRole role)
        {
            return;
        }

        var areas = MeetingHud.Instance.playerStates.ToArray();
        role.Guess1 = areas.FirstOrDefault(x => x.PlayerId == guess1);
        role.Guess2 = areas.FirstOrDefault(x => x.PlayerId == guess2);
    }

    [MethodRpc((uint)DivaniRpcCalls.StalkerTriggerWin)]
    public static void RpcTriggerStalkerWin(PlayerControl stalker)
    {
        if (stalker == null)
        {
            return;
        }

        DivaniPlugin.Instance.Log.LogInfo($"Stalker: win RPC received for {stalker.Data?.PlayerName}. AmHost={AmongUsClient.Instance.AmHost}");

        TriggerStalkerWin = true;
        WinningStalker = stalker;

        if (AmongUsClient.Instance.AmHost && stalker.Data != null)
        {
            var winners = new[] { stalker.Data };
            MiraAPI.GameEnd.CustomGameOver.Trigger<DivaniMods.GameOver.StalkerGameOver>(winners);
        }
    }

    public static IEnumerable<StalkerRole> GetActiveStalkers()
    {
        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player == null || player.Data == null || player.Data.IsDead)
            {
                continue;
            }

            if (player.Data.Role is StalkerRole stalker)
            {
                yield return stalker;
            }
        }
    }

    public override bool DidWin(GameOverReason gameOverReason)
    {
        return MiraAPI.GameEnd.CustomGameOver.Instance is DivaniMods.GameOver.StalkerGameOver;
    }
}
