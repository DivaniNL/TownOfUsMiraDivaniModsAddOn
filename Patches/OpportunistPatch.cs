using DivaniMods.Roles.Neutral.NeutralOutlier;
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
            opp.WildcardActiveThisMeeting = false;
            opp.PendingWildcardSkip = false;
        }
    }

    [RegisterEvent]
    public static void OnBeforeVote(BeforeVoteEvent evt)
    {
        var local = PlayerControl.LocalPlayer;
        if (local == null)
        {
            return;
        }

        if (!OpportunistRole.ActiveOpportunists.TryGetValue(local.PlayerId, out var opp))
        {
            return;
        }

        if (opp.WildcardButton == null || evt.VoteArea != opp.WildcardButton)
        {
            return;
        }

        evt.Cancel();

        if (opp.WildcardUsed || opp.WildcardActiveThisMeeting ||
            !OptionGroupSingleton<OpportunistOptions>.Instance.CanUseWildcard.Value)
        {
            return;
        }

        RpcOpportunistWildcard(local, local.PlayerId);
        opp.PendingWildcardSkip = true;
    }

    [MethodRpc((uint)DivaniRpcCalls.OpportunistWildcard)]
    public static void RpcOpportunistWildcard(PlayerControl sender, byte opportunistId)
    {
        if (!OpportunistRole.ActiveOpportunists.TryGetValue(opportunistId, out var opp))
        {
            return;
        }

        opp.WildcardActiveThisMeeting = true;
        opp.WildcardUsed = true;
    }


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

            var oppId = opp.Player.PlayerId;
            var votesAdded = 0;

            if (opp.WildcardActiveThisMeeting && OpportunistSkipped(@event.Votes, oppId))
            {
                votesAdded += CountSkipVotes(@event.Votes, oppId);
                votesAdded += CountSkipVotes(KnightedEvents.ExtraKnightVotes, oppId);
            }
            else
            {
                if (opp.WildcardActiveThisMeeting)
                {
                    RpcRefundOpportunistWildcard(PlayerControl.LocalPlayer, oppId);
                }

                if (!opp.VotedThisMeeting || !opp.CurrentMeetingTargetId.HasValue)
                {
                    continue;
                }

                var oppTarget = ApplyActiveSwaps(opp.CurrentMeetingTargetId.Value);

                votesAdded += CountVotesOnTarget(@event.Votes, oppId, oppTarget);
                votesAdded += CountVotesOnTarget(KnightedEvents.ExtraKnightVotes, oppId, oppTarget);
            }

            if (votesAdded == 0)
            {
                continue;
            }

            opp.VotesCollected += votesAdded;

            if (PlayerControl.LocalPlayer != null)
            {
                RpcSyncOpportunistVotes(PlayerControl.LocalPlayer, oppId, opp.VotesCollected);
            }
        }
    }

    [MethodRpc((uint)DivaniRpcCalls.OpportunistVotesSync)]
    public static void RpcSyncOpportunistVotes(PlayerControl sender, byte opportunistId, int votesCollected)
    {
        if (!OpportunistRole.ActiveOpportunists.TryGetValue(opportunistId, out var opp))
        {
            return;
        }

        opp.VotesCollected = votesCollected;
    }


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


    private const byte SkipVoteId = 253;

    private static bool OpportunistSkipped(System.Collections.Generic.IEnumerable<CustomVote> votes, byte oppId)
    {
        foreach (var vote in votes)
        {
            if (vote.Voter == oppId && vote.Suspect == SkipVoteId)
            {
                return true;
            }
        }
        return false;
    }

    [MethodRpc((uint)DivaniRpcCalls.OpportunistWildcardRefund)]
    public static void RpcRefundOpportunistWildcard(PlayerControl sender, byte opportunistId)
    {
        if (!OpportunistRole.ActiveOpportunists.TryGetValue(opportunistId, out var opp))
        {
            return;
        }

        opp.WildcardActiveThisMeeting = false;
        opp.WildcardUsed = false;
    }

    private static int CountSkipVotes(System.Collections.Generic.IEnumerable<CustomVote> votes, byte oppId)
    {
        var count = 0;
        foreach (var vote in votes)
        {
            if (vote.Voter == oppId)
            {
                continue;
            }

            if (vote.Suspect != SkipVoteId)
            {
                continue;
            }

            count++;
        }
        return count;
    }


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

        TryLockInWin();
    }

    [RegisterEvent]
    public static void OnRoundStart(RoundStartEvent evt)
    {
        if (evt.TriggeredByIntro)
        {
            return;
        }


        TryLockInWin();

        foreach (var opp in OpportunistRole.ActiveOpportunists.Values)
        {
            opp.CurrentMeetingTargetId = null;
            opp.VotedThisMeeting = false;
            opp.WildcardActiveThisMeeting = false;
            opp.PendingWildcardSkip = false;
            opp.WildcardButton = null;
        }
    }

    private static void TryLockInWin()
    {
        if (!AmongUsClient.Instance || !AmongUsClient.Instance.AmHost)
        {
            return;
        }

        var needed = (int)OptionGroupSingleton<OpportunistOptions>.Instance.VotesNeeded.Value;

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
