using System.Linq;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Meeting.Voting;
using MiraAPI.Modifiers;
using DivaniMods.Modifiers;

namespace DivaniMods.Patches;

/// <summary>
/// Redirects a Misvote's meeting vote (including Skip) to a random alive
/// player. Runs as a HandleVoteEvent handler - the voter still gets the
/// regular voting UI, but the final tallied vote ends up on a random target.
/// </summary>
public static class MisvoteVotePatches
{
    // Higher priority (lower number) than Mayor/Knighted so the random pick
    // is the one that actually gets recorded in VoteData.
    [RegisterEvent(-10)]
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

            var randomTargetId = PickRandomAliveTargetId(voter);
            if (randomTargetId == byte.MaxValue)
            {
                return;
            }

            // SetRemainingVotes(0) matches the Mayor/Knighted pattern - it guards
            // against additional votes leaking through after we cast ours.
            @event.VoteData.SetRemainingVotes(0);
            @event.VoteData.VoteForPlayer(randomTargetId);
            @event.Cancel();

            DivaniPlugin.Instance.Log.LogInfo(
                "Misvote: " + voter.Data.PlayerName +
                " -> redirected vote to player id " + randomTargetId + ".");
        }
        catch (System.Exception ex)
        {
            DivaniPlugin.Instance.Log.LogWarning("MisvoteVotePatches failed: " + ex.Message);
        }
    }

    private static byte PickRandomAliveTargetId(PlayerControl voter)
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
