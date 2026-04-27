using DivaniMods.Roles;
using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Events.Vanilla.Meeting.Voting;
using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using TownOfUs;
using TownOfUs.Assets;
using TownOfUs.Modules.Localization;
using TownOfUs.Patches;
using TownOfUs.Roles;
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

    [RegisterEvent]
    public static void OnVotingComplete(VotingCompleteEvent _)
    {
        // Tally votes only - DO NOT flip MetThreshold here. MeetingHud.VotingComplete runs
        // before ExileController spawns, so flagging the win at this point would cause
        // LogicGameFlowPatches.CheckEndCriteriaPatch to end the game before the exile screen
        // plays. Mirror Jester/Innocent: lock in the win during EjectionEvent instead.
        if (MeetingHud.Instance == null)
        {
            return;
        }

        var states = MeetingHudGetVotesPatch.States;
        if (states == null || states.Length == 0)
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

            var oppTarget = opp.CurrentMeetingTargetId.Value;

            // Count every vote cast onto the Opportunist's saved target by anyone other
            // than themselves. Each VoterState entry is one vote, so:
            //   - Mayor's extra votes count individually.
            //   - Prosecutor's 5 votes appear as 5 separate entries; if their ProsecuteVictim
            //     equals oppTarget, all 5 count toward the tally.
            //   - Swapper does not modify VoterState[] (it only adjusts the exile result),
            //     so original cast votes - which is what the user wants tracked - are used.
            var votesAdded = 0;
            foreach (var state in states)
            {
                if (state.VoterId == opp.Player.PlayerId)
                {
                    continue;
                }

                if (state.SkippedVote || state.VotedForId == byte.MaxValue)
                {
                    continue;
                }

                if (state.VotedForId != oppTarget)
                {
                    continue;
                }

                votesAdded++;
            }

            if (votesAdded == 0)
            {
                continue;
            }

            opp.VotesCollected += votesAdded;
        }
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
        }
    }
}
