using System;
using System.Collections.Generic;
using System.Linq;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using DivaniMods.Assets;
using DivaniMods.Buttons;
using DivaniMods.Options;
using DivaniMods.Patches;
using TownOfUs.Interfaces;
using TownOfUs.Modifiers.Game.Alliance;
using TownOfUs.Modules;
using TownOfUs.Roles;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;

namespace DivaniMods.Roles;

/// <summary>
/// Neutral Evil that only exists when Lovers are present in the game. In meetings,
/// the Stalker can pick two players they believe are the Lovers. If both picks are
/// Lovers, Stalker instantly wins; otherwise, Stalker is exiled. During the round
/// the Stalker can also read Lover chat with identities scrubbed.
/// </summary>
public sealed class StalkerRole(IntPtr cppPtr) : NeutralRole(cppPtr), ITownOfUsRole, IUnlovable
{
    public static readonly Color StalkerColor = new Color32(80, 0, 120, 255);

    // Stalker is built around the Lovers existing; getting the Lover modifier itself would
    // completely break the guess mechanic (and create a bizarre neutral-lover pair), so
    // the Lover assignment pipeline skips anyone flagged IsUnlovable.
    public bool IsUnlovable => true;

    public string RoleName => "Stalker";
    public string RoleDescription => "Unmask the Lovers!";
    public string RoleLongDescription =>
        "You are a Stalker. Lovers are in the game and you want\n" +
        "to expose them. In meetings, mark two players with hearts.\n" +
        "Guess both Lovers correctly and you win instantly.\n" +
        "Guess wrong and you are exiled.";
    public Color RoleColor => StalkerColor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleAlignment RoleAlignment => RoleAlignment.NeutralEvil;

    public CustomRoleConfiguration Configuration => new(this)
    {
        TasksCountForProgress = false,
        Icon = DivaniAssets.StalkerIcon,
        IntroSound = DivaniAssets.StalkerIntroSound,
    };

    // Meeting-state pick tracking (mirror of Swapper's PlayerVoteArea pattern).
    [HideFromIl2Cpp] public PlayerVoteArea? Guess1 { get; set; }
    [HideFromIl2Cpp] public PlayerVoteArea? Guess2 { get; set; }

    // Marks the current meeting's resolution so StalkerEvents only reacts once.
    public bool ResolvedThisMeeting { get; set; }

    private MeetingMenu? _meetingMenu;

    // Global state for game-end handling (single winner; similar to Plague Doctor).
    public static bool TriggerStalkerWin { get; set; }
    public static PlayerControl? WinningStalker { get; set; }

    // Set during ProcessVotesEvent when the Stalker correctly guessed both
    // Lovers; cleared to null on any failure. We only fire RpcTriggerStalkerWin
    // once the meeting's EjectionEvent runs so the game-end screen appears
    // *after* the meeting wraps up (matching the Plague Doctor flow).
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
            // Heart sprites already encode their colors (grey = not picked, pink = picked);
            // override MeetingMenu's default green/red/white tints with white so the sprite
            // pixels pass through untouched.
            _meetingMenu = new MeetingMenu(
                this,
                OnHeartClicked,
                MeetingAbilityType.Toggle,
                DivaniAssets.HeartFilled,
                DivaniAssets.HeartNotFilled,
                IsExempt,
                activeColor: Color.white,
                disabledColor: Color.white,
                hoverColor: Color.white)
            {
                // Same side/position as Swapper so the pickers match visually.
                Position = new Vector3(-0.40f, 0f, -3f),
            };

            // Force-enable the chat button + swap to the Lover heart sprites so the
            // Stalker can actually open the chat and see mirrored Lover messages.
            // Mirrors LoverModifier.OnActivate.
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

            // Meetings use the normal chat button sprites (everyone can chat,
            // no private Lover channel). Love sprites come back on round start.
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
        var player = GameData.Instance.GetPlayerById(voteArea.TargetPlayerId)?.Object;
        return !player || player!.Data == null || player.Data.Disconnected || player.Data.IsDead;
    }

    private void OnHeartClicked(PlayerVoteArea voteArea, MeetingHud meeting)
    {
        if (meeting.state == MeetingHud.VoteStates.Discussion || IsExempt(voteArea))
        {
            return;
        }

        // Toggle selection with the same semantics as SwapperRole.SetActive:
        //   empty + empty  -> fill Guess1
        //   filled + empty -> fill Guess2
        //   click same     -> clear that slot
        //   both full, new -> drop Guess1, shift Guess2 -> Guess1, set Guess2
        if (!Guess1)
        {
            Guess1 = voteArea;
            _meetingMenu!.Actives[voteArea.TargetPlayerId] = true;
        }
        else if (!Guess2)
        {
            Guess2 = voteArea;
            _meetingMenu!.Actives[voteArea.TargetPlayerId] = true;
        }
        else if (Guess1 == voteArea)
        {
            _meetingMenu!.Actives[Guess1.TargetPlayerId] = false;
            Guess1 = null;
        }
        else if (Guess2 == voteArea)
        {
            _meetingMenu!.Actives[Guess2.TargetPlayerId] = false;
            Guess2 = null;
        }
        else
        {
            _meetingMenu!.Actives[Guess1.TargetPlayerId] = false;
            Guess1 = Guess2;
            Guess2 = voteArea;
            _meetingMenu.Actives[voteArea.TargetPlayerId] = true;
        }

        RpcSyncGuesses(Player, Guess1?.TargetPlayerId ?? byte.MaxValue, Guess2?.TargetPlayerId ?? byte.MaxValue);
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
        role.Guess1 = areas.FirstOrDefault(x => x.TargetPlayerId == guess1);
        role.Guess2 = areas.FirstOrDefault(x => x.TargetPlayerId == guess2);
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

    /// <summary>
    /// Enumerates all alive Stalkers currently in game.
    /// </summary>
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
