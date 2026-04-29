using DivaniMods.Roles;
using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Events.Vanilla.Meeting.Voting;
using MiraAPI.GameOptions;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using MiraAPI.Voting;
using Reactor.Networking.Attributes;
using DivaniMods.Buttons;
using TownOfUs;
using TownOfUs.Assets;
using TownOfUs.Events.Misc;
using TownOfUs.Modules.Localization;
using TownOfUs.Patches;
using TownOfUs.Roles;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using DivaniMods.Options;
using UnityEngine;

namespace DivaniMods.Patches;

[HarmonyPatch]
public static class OpportunistPatch
{
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CoStartGame))]
    [HarmonyPostfix]
    public static void ResetOnGameStart()
    {
        OpportunistRole.ClearAndReload();
    }

    [RegisterEvent]
    public static void OnMeetingStart(StartMeetingEvent _)
    {
        foreach (var opp in OpportunistRole.ActiveOpportunists.Values)
        {
            opp.CurrentMeetingTargetId = null;
            opp.VotedThisMeeting = false;
        }
    }

    // Capture the Opportunist's target the moment they vote, BEFORE Prosecutor's
    // CheckForEndVoting handler wipes everyone's VoteData and re-casts 5 votes for the
    // ProsecuteVictim. After that wipe, the Opportunist's own vote is gone from
    // MeetingHud.VoterState[], so we can't rely on States to identify their target.
    [RegisterEvent]
    public static void OnHandleVote(HandleVoteEvent evt)
    {
        if (evt.Player == null || evt.TargetPlayerInfo == null)
        {
            return;
        }

        if (!OpportunistRole.ActiveOpportunists.TryGetValue(evt.Player.PlayerId, out var opp))
        {
            return;
        }

        // Only the first vote per meeting counts as the Opportunist's chosen target.
        if (opp.VotedThisMeeting)
        {
            return;
        }

        opp.VotedThisMeeting = true;
        opp.CurrentMeetingTargetId = evt.TargetPlayerInfo.PlayerId;
    }

    // Priority 200 = runs AFTER SwapperEvents.ProcessVotesEventHandler (10) and
    // MisvoteVotePatches.ProcessVotesEventHandler (100). The Misvote handler ADDS to
    // @event.Votes and rewrites KnightedEvents.ExtraKnightVotes in place, but the
    // Swapper handler builds its rewritten vote list LOCALLY and only assigns
    // @event.ExiledPlayer - it never mutates @event.Votes. So at priority 200,
    // @event.Votes still holds the pre-swap Suspect ids.
    //
    // To keep the Opportunist's tally consistent with the actual exile (which IS
    // post-swap), we mirror Swapper's swap ourselves: if the Opportunist's saved
    // target id equals one of any active SwapperRole's Swap1/Swap2 ids, we swap
    // it for the partner id and tally votes against the partner. That way the
    // Opportunist gets credit for every vote that effectively lands on their
    // chosen target's exile slot - including the votes Swapper redirected there.
    //
    // We tally during ProcessVotes (not VotingComplete) and DO NOT flip MetThreshold
    // here: ExileController has not spawned yet, so flagging the win would let
    // LogicGameFlowPatches.CheckEndCriteriaPatch end the game before the exile
    // screen plays. Lock-in still happens in EjectionEvent / RoundStart, matching
    // Jester/Innocent.
    [RegisterEvent(200)]
    public static void OnProcessVotes(ProcessVotesEvent @event)
    {
        if (@event?.Votes == null)
        {
            return;
        }

        foreach (var opp in OpportunistRole.ActiveOpportunists.Values)
        {
            if (opp.MetThreshold || opp.Player == null)
            {
                continue;
            }

            if (!opp.VotedThisMeeting || !opp.CurrentMeetingTargetId.HasValue)
            {
                continue;
            }

            var oppId = opp.Player.PlayerId;
            var oppTarget = ApplyActiveSwaps(opp.CurrentMeetingTargetId.Value);

            var votesAdded = 0;

            votesAdded += CountVotesOnTarget(@event.Votes, oppId, oppTarget);

            // Knighted bonus votes (when "Show Knighted Votes" is off) live in a
            // separate list that TownOfUs only merges in for the final exile calc.
            // Count those too so Knighted bonus votes onto the Opportunist's
            // target still tick the tally up.
            votesAdded += CountVotesOnTarget(KnightedEvents.ExtraKnightVotes, oppId, oppTarget);

            if (votesAdded == 0)
            {
                continue;
            }

            opp.VotesCollected += votesAdded;
        }
    }

    // `target` here is ALREADY swap-adjusted: it is the pre-swap suspect id whose
    // votes will end up on the Opportunist's chosen exile slot after Swapper rewrites.
    // The vote.Suspect values in @event.Votes are pre-swap, so we compare directly -
    // swap-adjusting them too would just undo the target's swap.
    private static int CountVotesOnTarget(System.Collections.Generic.IEnumerable<CustomVote> votes, byte oppId, byte target)
    {
        var count = 0;
        foreach (var vote in votes)
        {
            if (vote.Voter == oppId)
            {
                continue;
            }

            if (vote.Suspect == byte.MaxValue)
            {
                continue;
            }

            if (vote.Suspect != target)
            {
                continue;
            }

            count++;
        }
        return count;
    }

    // Mirrors TownOfUs.Events.Crewmate.SwapperEvents.SwapVotes: for every active
    // SwapperRole whose Swap1 and Swap2 PlayerVoteAreas are both set and whose
    // owner is alive, votes targeting Swap1.TargetPlayerId are treated as if they
    // targeted Swap2.TargetPlayerId, and vice versa. We chain them in iteration
    // order to be safe, even though vanilla SwapVotes does not actually compose
    // multiple swaps (each swap recomputes exile from the original votes,
    // overwriting the previous swapper's result - so chaining matches the
    // single-swapper case, which is by far the most common scenario).
    private static byte ApplyActiveSwaps(byte playerId)
    {
        foreach (var swapper in CustomRoleUtils.GetActiveRolesOfType<SwapperRole>())
        {
            if (swapper == null || swapper.Player == null)
            {
                continue;
            }

            if (swapper.Player.HasDied())
            {
                continue;
            }

            var swap1 = swapper.Swap1;
            var swap2 = swapper.Swap2;
            if (swap1 == null || swap2 == null)
            {
                continue;
            }

            var s1 = swap1.TargetPlayerId;
            var s2 = swap2.TargetPlayerId;

            if (playerId == s1)
            {
                playerId = s2;
            }
            else if (playerId == s2)
            {
                playerId = s1;
            }
        }
        return playerId;
    }

    [RegisterEvent]
    public static void OnEjection(EjectionEvent _)
    {
        // Lock in the win during the exile screen. ExileController.Instance is alive here,
        // so CheckEndCriteria will be suppressed until the exile screen finishes - then the
        // win triggers cleanly via NeutralRoleWinCondition, matching Jester/Innocent.
        TryLockInWin();
    }

    [RegisterEvent]
    public static void OnRoundStart(RoundStartEvent evt)
    {
        if (evt.TriggeredByIntro)
        {
            return;
        }

        // Fallback: a meeting may end without an exile (skip vote / tie). If the threshold
        // was reached anyway, lock in the win at round start so it triggers on the next
        // CheckEndCriteria tick.
        TryLockInWin();

        foreach (var opp in OpportunistRole.ActiveOpportunists.Values)
        {
            opp.CurrentMeetingTargetId = null;
            opp.VotedThisMeeting = false;
        }
    }

    private static void TryLockInWin()
    {
        if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost)
        {
            return;
        }

        var needed = (int)OptionGroupSingleton<OpportunistOptions>.Instance.VotesNeeded;

        foreach (var opp in OpportunistRole.ActiveOpportunists.Values)
        {
            if (opp.MetThreshold || opp.Player == null)
            {
                continue;
            }

            if (opp.VotesCollected < needed)
            {
                continue;
            }

            opp.MetThreshold = true;
            opp.AboutToWin = true;

            if (PlayerControl.LocalPlayer != null)
            {
                RpcSyncOpportunistWin(PlayerControl.LocalPlayer, opp.Player.PlayerId, opp.VotesCollected);
            }
        }
    }

    [MethodRpc((uint)DivaniRpcCalls.OpportunistWin)]
    public static void RpcSyncOpportunistWin(PlayerControl sender, byte opportunistId, int votesCollected)
    {
        if (!OpportunistRole.ActiveOpportunists.TryGetValue(opportunistId, out var opp))
        {
            return;
        }

        opp.VotesCollected = votesCollected;
        opp.MetThreshold = true;
        opp.AboutToWin = true;
    }
}
