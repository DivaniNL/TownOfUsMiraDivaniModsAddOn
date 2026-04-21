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
using DivaniMods.Modifiers;
using TownOfUs.Events.Misc;
using TownOfUs.Modifiers;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;

namespace DivaniMods.Patches;

/// <summary>
/// Redirects every vote a Misvoted player casts to an independently-chosen
/// random alive target. Covers:
///   - normal single votes (via HandleVoteEvent),
///   - revealed Mayor's 3 votes (via HandleVoteEvent, replacing MayorEvents),
///   - Knighted bonus votes when "Show Knighted Votes" is on (via HandleVoteEvent,
///     replacing KnightedEvents),
///   - Knighted bonus votes when "Show Knighted Votes" is off (via ProcessVotesEvent,
///     rewriting the extras KnightedEvents duplicates),
///   - Prosecutor's 5 prosecute votes (via CheckForEndVotingEvent, replacing the
///     votes ProsecutorEvents casts).
///
/// Prosecutor punishment still relies on <c>ProsecutorEvents.WrapUpEvent</c>,
/// which checks the player who actually got exiled - not the intended victim -
/// so a misvoted prosecution that exiles a crewmate still punishes the
/// Prosecutor, and one that exiles an evil does not.
/// </summary>
public static class MisvoteVotePatches
{
    // Priority 100 = runs AFTER MayorEvents/KnightedEvents.HandleVoteEvent (default 0).
    // MiraEventManager.InvokeEvent runs every handler regardless of @event.Cancel(),
    // and cancellation only blocks the default action at the end of VotingUtils.HandleVote.
    // If we ran earlier (negative priority), Mayor's handler would still execute after us
    // and append its 3 votes on the original TargetId on top of our random picks, producing
    // 3 Mayor votes + 3 random votes (most visible with Skip). Running last lets us wipe
    // whatever Mayor/Knighted wrote and replace it with the random picks, then cancel to
    // suppress the default single-vote add.
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
            // Drop any votes that earlier handlers (Mayor's 3x VoteForPlayer, Knighted's
            // 1+N VoteForPlayer when ShowKnightedVotes is on, etc.) already appended.
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

            DivaniPlugin.Instance.Log.LogInfo(
                "Misvote: " + voter.Data.PlayerName + " -> redirected " +
                voteCount + " vote(s) to random target(s).");
        }
        catch (Exception ex)
        {
            DivaniPlugin.Instance.Log.LogWarning("MisvoteVotePatches.HandleVote failed: " + ex.Message);
        }
    }

    // Priority 100 = runs AFTER KnightedEvents.ProcessVotesEventHandler (default 0).
    // When "Show Knighted Votes" is OFF, KnightedEvents adds bonus CustomVotes to
    // KnightedEvents.ExtraKnightVotes, all duplicating the first vote's Suspect.
    // We re-roll each bonus vote cast by a Misvoted player so every extra ends up
    // on an independently-chosen random target, then recompute the exiled player.
    [RegisterEvent(100)]
    public static void ProcessVotesEventHandler(ProcessVotesEvent @event)
    {
        try
        {
            if (KnightedEvents.ExtraKnightVotes.Count == 0)
            {
                return;
            }

            var anyChanged = false;
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

            DivaniPlugin.Instance.Log.LogInfo(
                "Misvote: re-rolled Knighted bonus votes; exiled=" +
                (@event.ExiledPlayer != null ? @event.ExiledPlayer.PlayerName : "<none>") + ".");
        }
        catch (Exception ex)
        {
            DivaniPlugin.Instance.Log.LogWarning("MisvoteVotePatches.ProcessVotes failed: " + ex.Message);
        }
    }

    // Priority 100 = runs AFTER ProsecutorEvents.VoteEvent (default 0). That handler
    // clears every player's VoteData and then casts 5 votes on the Prosecutor's
    // chosen victim. If the Prosecutor is Misvoted, we wipe those 5 votes and
    // re-cast 5 independently-random votes so the actual exile is chaotic.
    // ProsecutorEvents.WrapUpEvent already punishes the Prosecutor based on the
    // player who actually ends up exiled, so the punishment path still works.
    [RegisterEvent(100)]
    public static void CheckForEndVotingEventHandler(CheckForEndVotingEvent @event)
    {
        try
        {
            if (!@event.IsVotingComplete)
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

            DivaniPlugin.Instance.Log.LogInfo(
                "Misvote: Prosecutor " + prosecutor.Player.Data.PlayerName +
                " -> redirected 5 prosecute vote(s) to random target(s).");
        }
        catch (Exception ex)
        {
            DivaniPlugin.Instance.Log.LogWarning("MisvoteVotePatches.CheckEndVoting failed: " + ex.Message);
        }
    }

    // Matches the number of votes the voter would cast in their *normal* turn
    // through HandleVoteEvent. Mayor (Revealed) casts 3, Knighted casts
    // 1 + knights * VotesPerKnight when ShowVotes is enabled, everyone else 1.
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

    private static PlayerControl? GetPlayer(byte playerId)
    {
        return PlayerControl.AllPlayerControls.ToArray()
            .FirstOrDefault(p => p != null && p.PlayerId == playerId);
    }

    private static byte PickRandomAliveTargetId()
    {
        var candidates = PlayerControl.AllPlayerControls.ToArray()
            .Where(p => p != null && p.Data != null
                        && !p.Data.IsDead && !p.Data.Disconnected)
            .Select(p => p.PlayerId)
            .ToList();

        if (candidates.Count == 0)
        {
            return byte.MaxValue;
        }

        var idx = UnityEngine.Random.Range(0, candidates.Count);
        return candidates[idx];
    }
}
