using System;
using System.Collections.Generic;
using System.Linq;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Meeting.Voting;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using MiraAPI.Voting;
using DivaniMods.Modifiers.Game.Universal;
using DivaniMods.Options;
using TownOfUs.Events.Misc;
using TownOfUs.Modifiers;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;

namespace DivaniMods.Patches;

public static class MisvoteVotePatches
{

    [RegisterEvent(100)]
    public static void HandleVoteEventHandler(HandleVoteEvent @event)
    {
        try
        {
            var voter = @event.Player;
            if (voter == null || voter.Data == null || voter.Data.IsDead)
            {
                return;
            }

            if (!voter.HasModifier<MisvoteModifier>())
            {
                return;
            }

            var voteCount = GetVoteCountForVoter(voter);

            var voteData = @event.VoteData;

            voteData.Votes.Clear();
            voteData.SetRemainingVotes(0);

            for (var i = 0; i < voteCount; i++)
            {
                var targetId = PickRandomAliveTargetId();
                if (targetId == byte.MaxValue)
                {
                    break;
                }
                voteData.VoteForPlayer(targetId);
            }

            @event.Cancel();

        }
        catch (Exception ex)
        {
            DivaniPlugin.Instance.Log.LogWarning("MisvoteVotePatches.HandleVote failed: " + ex.Message);
        }
    }


    [RegisterEvent(100)]
    public static void ProcessVotesEventHandler(ProcessVotesEvent @event)
    {
        try
        {

            if (IsActiveProsecutionRound())
            {
                return;
            }

            var anyChanged = AddRandomVotesForNonVotingMisvotedPlayers(@event.Votes);
            for (var i = 0; i < KnightedEvents.ExtraKnightVotes.Count; i++)
            {
                var extra = KnightedEvents.ExtraKnightVotes[i];
                var voter = GetPlayer(extra.Voter);
                if (voter == null || !voter.HasModifier<MisvoteModifier>())
                {
                    continue;
                }

                var newTarget = PickRandomAliveTargetId();
                if (newTarget == byte.MaxValue)
                {
                    continue;
                }

                KnightedEvents.ExtraKnightVotes[i] = new CustomVote(extra.Voter, newTarget);
                anyChanged = true;
            }

            if (!anyChanged)
            {
                return;
            }

            var fullVotes = new List<CustomVote>(@event.Votes);
            fullVotes.AddRange(KnightedEvents.ExtraKnightVotes);
            @event.ExiledPlayer = VotingUtils.GetExiled(fullVotes, out _);

        }
        catch (Exception ex)
        {
            DivaniPlugin.Instance.Log.LogWarning("MisvoteVotePatches.ProcessVotes failed: " + ex.Message);
        }
    }

    [RegisterEvent(100)]
    public static void CheckForEndVotingEventHandler(CheckForEndVotingEvent @event)
    {
        try
        {
            if (!@event.IsVotingComplete)
            {
                return;
            }

            if (!OptionGroupSingleton<MisvoteOptions>.Instance.ProsecutorVotesRandom.Value)
            {
                return;
            }

            var prosecutor = CustomRoleUtils.GetActiveRolesOfType<ProsecutorRole>()
                .FirstOrDefault(x =>
                    x != null && x.Player != null && !x.Player.HasDied() &&
                    x.HasProsecuted && x.ProsecuteVictim != byte.MaxValue);

            if (prosecutor == null)
            {
                return;
            }

            if (prosecutor.ProsecutionsCompleted >=
                OptionGroupSingleton<ProsecutorOptions>.Instance.MaxProsecutions)
            {
                return;
            }

            if (!prosecutor.Player.HasModifier<MisvoteModifier>())
            {
                return;
            }

            var prosData = prosecutor.Player.GetVoteData();
            if (prosData == null)
            {
                return;
            }

            prosData.Votes.Clear();
            prosData.VotesRemaining = 0;

            for (var i = 0; i < 5; i++)
            {
                var targetId = PickRandomAliveTargetId();
                if (targetId == byte.MaxValue)
                {
                    break;
                }
                prosData.VoteForPlayer(targetId);
            }

        }
        catch (Exception ex)
        {
            DivaniPlugin.Instance.Log.LogWarning("MisvoteVotePatches.CheckEndVoting failed: " + ex.Message);
        }
    }

    private static int GetVoteCountForVoter(PlayerControl voter)
    {
        if (voter.Data.Role is MayorRole mayor && mayor.Revealed)
        {
            return 3;
        }

        if (KnightedEvents.ShowVotes && voter.HasModifier<KnightedModifier>())
        {
            var knightCount = voter.GetModifiers<KnightedModifier>()?.Count() ?? 0;
            if (knightCount <= 0)
            {
                knightCount = 1;
            }
            var votesPerKnight = (int)OptionGroupSingleton<MonarchOptions>.Instance.VotesPerKnight;
            return 1 + (knightCount * votesPerKnight);
        }

        return 1;
    }

    private static int GetVoteCountForNonVotingVoter(PlayerControl voter)
    {
        if (voter.Data.Role is MayorRole mayor && mayor.Revealed)
        {
            return 3;
        }

        if (voter.HasModifier<KnightedModifier>())
        {
            var knightCount = voter.GetModifiers<KnightedModifier>()?.Count() ?? 0;
            if (knightCount <= 0)
            {
                knightCount = 1;
            }

            var votesPerKnight = (int)OptionGroupSingleton<MonarchOptions>.Instance.VotesPerKnight;
            return 1 + (knightCount * votesPerKnight);
        }

        return 1;
    }

    private static bool AddRandomVotesForNonVotingMisvotedPlayers(List<CustomVote> votes)
    {
        var anyChanged = false;
        var votedPlayerIds = votes
            .Select(vote => vote.Voter)
            .Concat(KnightedEvents.ExtraKnightVotes.Select(vote => vote.Voter))
            .ToHashSet();

        foreach (var voter in PlayerControl.AllPlayerControls)
        {
            if (voter == null || voter.Data == null || voter.Data.IsDead || voter.Data.Disconnected)
            {
                continue;
            }

            if (!voter.HasModifier<MisvoteModifier>() || votedPlayerIds.Contains(voter.PlayerId))
            {
                continue;
            }

            var voteCount = GetVoteCountForNonVotingVoter(voter);
            for (var i = 0; i < voteCount; i++)
            {
                var targetId = PickRandomAliveTargetId();
                if (targetId == byte.MaxValue)
                {
                    break;
                }

                votes.Add(new CustomVote(voter.PlayerId, targetId));
                anyChanged = true;
            }

        }

        return anyChanged;
    }

    private static bool IsActiveProsecutionRound()
    {
        var prosecutor = CustomRoleUtils.GetActiveRolesOfType<ProsecutorRole>()
            .FirstOrDefault(x =>
                x != null && x.Player != null && !x.Player.HasDied() &&
                x.HasProsecuted && x.ProsecuteVictim != byte.MaxValue);

        if (prosecutor == null)
        {
            return false;
        }

        return prosecutor.ProsecutionsCompleted <
            OptionGroupSingleton<ProsecutorOptions>.Instance.MaxProsecutions;
    }

    private static PlayerControl? GetPlayer(byte playerId)
    {
        foreach (var p in PlayerControl.AllPlayerControls)
        {
            if (p != null && p.PlayerId == playerId)
            {
                return p;
            }
        }

        return null;
    }

    private static byte PickRandomAliveTargetId()
    {

        var candidates = new List<byte>();
        foreach (var p in PlayerControl.AllPlayerControls)
        {
            if (p == null || p.Data == null || p.Data.IsDead || p.Data.Disconnected)
            {
                continue;
            }

            candidates.Add(p.PlayerId);
        }

        if (candidates.Count == 0)
        {
            return byte.MaxValue;
        }

        var idx = UnityEngine.Random.Range(0, candidates.Count);
        return candidates[idx];
    }
}
