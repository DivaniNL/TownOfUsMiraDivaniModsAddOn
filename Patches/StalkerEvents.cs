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
/// EjectionEvent: fires after the exile animation. If a correct guess was
/// marked, fire the win RPC now - this routes through CustomGameOver just like
/// Plague Doctor's solo win.
/// </summary>
public static class StalkerEvents
{
    // lower number = higher priority; run before Swapper (10) and Tiebreaker (default 0)
    [RegisterEvent(5)]
    public static void ProcessVotesEventHandler(ProcessVotesEvent @event)
    {
        foreach (var stalker in StalkerRole.GetActiveStalkers().ToList())
        {
            ResolveStalkerGuess(@event, stalker);
        }
    }

    [RegisterEvent]
    public static void EjectionEventHandler(EjectionEvent @event)
    {
        var winner = StalkerRole.PendingWinStalker;
        StalkerRole.PendingWinStalker = null;

        if (winner == null || winner.Data == null || winner.Data.IsDead)
        {
            return;
        }

        DivaniPlugin.Instance.Log.LogInfo(
            $"Stalker '{winner.Data.PlayerName}' guessed both Lovers correctly. " +
            "Triggering post-meeting Stalker win.");
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

        if (stalker.Guess1.TargetPlayerId == stalker.Guess2.TargetPlayerId)
        {
            return;
        }

        var p1 = GetPlayerById(stalker.Guess1.TargetPlayerId);
        var p2 = GetPlayerById(stalker.Guess2.TargetPlayerId);
        if (p1 == null || p2 == null)
        {
            return;
        }

        stalker.ResolvedThisMeeting = true;

        var bothLovers = p1.HasModifier<LoverModifier>() && p2.HasModifier<LoverModifier>();

        if (bothLovers)
        {
            // Stalker wins - but don't trigger the game-end until the meeting
            // actually wraps up (EjectionEventHandler above picks this up).
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
