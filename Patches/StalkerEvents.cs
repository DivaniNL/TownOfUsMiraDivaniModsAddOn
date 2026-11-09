using System.Linq;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Events.Vanilla.Meeting.Voting;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using DivaniMods.Options;
using DivaniMods.Roles;
using TownOfUs.Modifiers.Game.Alliance;

namespace DivaniMods.Patches;

/// <summary>
/// Resolves the Stalker's meeting guess in two stages so the win screen shows
/// up *after* the meeting wraps up (mirroring PlagueDoctor).
///
/// ProcessVotesEvent: decide what happens to the Stalker's guess - if both
/// picks are Lovers, remember that we should trigger a Stalker win once the
/// meeting ends. If wrong (and WrongGuessExiles is on), rewrite ExiledPlayer
/// so the Stalker gets ejected with the rest of the meeting. No picks / only
/// one heart filled / same-player picks / non-players all no-op so the meeting
/// continues as normal.
///
/// EndMeetingEvent: fires after the vote and exile sequence, including ties and
/// skips. If a correct guess was marked, fire the win RPC now.
/// </summary>
public static class StalkerEvents
{
    // lower number = higher priority; run before Swapper (10) and Tiebreaker (default 0)
    [RegisterEvent(5)]
    public static void ProcessVotesEventHandler(ProcessVotesEvent @event)
    {
        if (!AmongUsClient.Instance || !AmongUsClient.Instance.AmHost)
        {
            return;
        }

        foreach (var stalker in StalkerRole.GetActiveStalkers().ToList())
        {
            ResolveStalkerGuess(@event, stalker);
        }
    }

    [RegisterEvent]
    public static void EndMeetingEventHandler(EndMeetingEvent @event)
    {
        if (!AmongUsClient.Instance || !AmongUsClient.Instance.AmHost)
        {
            return;
        }

        var winner = StalkerRole.PendingWinStalker;
        StalkerRole.PendingWinStalker = null;

        if (winner == null || winner.Data == null)
        {
            return;
        }

        DivaniPlugin.Instance.Log.LogInfo(
            $"Stalker '{winner.Data.PlayerName}' guessed both Lovers correctly. " +
            "Triggering Stalker win after the meeting.");
        StalkerRole.RpcTriggerStalkerWin(winner);
    }

    private static void ResolveStalkerGuess(ProcessVotesEvent @event, StalkerRole stalker)
    {
        if (stalker == null || stalker.Player == null || stalker.ResolvedThisMeeting)
        {
            return;
        }

        // No guess or only one heart filled -> nothing happens, meeting continues.
        if (stalker.Guess1 == null || stalker.Guess2 == null)
        {
            return;
        }

        if (stalker.Guess1.PlayerId == stalker.Guess2.PlayerId)
        {
            return;
        }

        var p1 = GetPlayerById(stalker.Guess1.PlayerId);
        var p2 = GetPlayerById(stalker.Guess2.PlayerId);
        if (p1 == null || p2 == null)
        {
            return;
        }

        stalker.ResolvedThisMeeting = true;

        var bothLovers = p1.HasModifier<LoverModifier>() && p2.HasModifier<LoverModifier>();

        if (bothLovers)
        {
            // Defer ending the game until the meeting has fully resolved.
            StalkerRole.PendingWinStalker = stalker.Player;
            return;
        }

        if (!OptionGroupSingleton<StalkerOptions>.Instance.WrongGuessExiles)
        {
            DivaniPlugin.Instance.Log.LogInfo(
                $"Stalker '{stalker.Player.Data.PlayerName}' guessed wrong but WrongGuessExiles is disabled.");
            return;
        }

        DivaniPlugin.Instance.Log.LogInfo(
            $"Stalker '{stalker.Player.Data.PlayerName}' guessed wrong. Overriding exile.");
        @event.ExiledPlayer = stalker.Player.Data;
    }

    private static PlayerControl? GetPlayerById(byte id)
    {
        foreach (var p in PlayerControl.AllPlayerControls)
        {
            if (p != null && p.PlayerId == id)
            {
                return p;
            }
        }
        return null;
    }
}
