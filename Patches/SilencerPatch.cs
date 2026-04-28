using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using DivaniMods.Options;
using DivaniMods.Roles;
using UnityEngine;

namespace DivaniMods.Patches;

/// <summary>
/// Silencer: every kill the Silencer makes shaves seconds off the voting
/// phase of every meeting for the rest of the game. The cut per kill and the
/// voting-time floor are configurable; total kill seconds are clamped against
/// the floor each meeting independently, so a longer base voting time yields
/// more headroom and the floor is always respected.
///
/// Concrete example - voting time 150s, seconds per kill 40s, 2 kills made:
/// every meeting voting time = 150 - 40 - 40 = 70s, discussion untouched.
///
/// All clients run the same logic deterministically:
/// <see cref="AfterMurderEvent"/> fires on every client when a player is
/// killed, so each client can independently track Silencer kills and apply
/// the same reduction in <see cref="MeetingHud.Update"/>.
/// </summary>
public static class SilencerPatch
{
    /// <summary>Total seconds accumulated from Silencer kills since the game started.</summary>
    private static float _totalKillSeconds;

    /// <summary>Reduction (seconds) chosen at meeting start, clamped against the voting-time floor.</summary>
    private static float _cachedReduction;

    /// <summary>Whether the cached reduction has already been applied to this meeting's timer.</summary>
    private static bool _appliedThisMeeting;

    /// <summary>
    /// Track every kill made by a Silencer. Runs on all clients because
    /// <see cref="AfterMurderEvent"/> follows the murder RPC. Persists for
    /// the rest of the game so every future meeting gets the cut.
    /// </summary>
    [RegisterEvent]
    public static void OnAfterMurder(AfterMurderEvent evt)
    {
        var source = evt.Source;
        if (source == null || source.Data == null) return;
        if (source.Data.Role is not SilencerRole) return;

        _totalKillSeconds += OptionGroupSingleton<SilencerOptions>.Instance.SecondsPerKill;
    }

    /// <summary>
    /// Reset all tracking when a fresh game starts so kills from a previous
    /// match don't leak into the new one.
    /// </summary>
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
    private static class OnGameEndPatch
    {
        private static void Postfix()
        {
            _totalKillSeconds = 0f;
            _cachedReduction = 0f;
            _appliedThisMeeting = false;
        }
    }

    /// <summary>
    /// Cache the reduction for this meeting from the running total. Clamping
    /// happens here so the floor is checked against whatever voting time is
    /// currently configured. The total is NOT reset between meetings - every
    /// future meeting also gets the full accumulated cut.
    /// </summary>
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
    private static class StartPatch
    {
        private static void Postfix()
        {
            _appliedThisMeeting = false;

            var opts = OptionGroupSingleton<SilencerOptions>.Instance;
            var votingTime = GameOptionsManager.Instance != null
                ? GameOptionsManager.Instance.currentNormalGameOptions.VotingTime
                : 0;

            var headroom = Mathf.Max(0f, votingTime - opts.MinimumVotingTime);
            _cachedReduction = Mathf.Clamp(_totalKillSeconds, 0f, headroom);
        }
    }

    /// <summary>
    /// Apply the cached reduction the first frame the meeting enters the
    /// voting phase. <see cref="MeetingHud.VoteStates"/> goes
    /// Animating(0) -> Discussion(1) -> NotVoted(2)/Voted(3) -> Results(4)
    /// -> Proceeding(5); NotVoted and Voted are the real voting phase.
    /// Bumping <see cref="MeetingHud.discussionTimer"/> forward only here
    /// shortens voting without touching the intro cutscene or discussion.
    /// </summary>
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
    private static class UpdatePatch
    {
        private static void Postfix(MeetingHud __instance)
        {
            if (_appliedThisMeeting) return;
            if (_cachedReduction <= 0f) return;
            if (__instance == null) return;

            var state = __instance.state;
            if (state != MeetingHud.VoteStates.NotVoted &&
                state != MeetingHud.VoteStates.Voted)
            {
                return;
            }

            __instance.discussionTimer += _cachedReduction;
            _appliedThisMeeting = true;
        }
    }
}
