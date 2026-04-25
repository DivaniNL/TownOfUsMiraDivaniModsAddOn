using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using DivaniMods.Options;
using DivaniMods.Roles;
using UnityEngine;

namespace DivaniMods.Patches;

/// <summary>
/// Silencer: every kill the Silencer makes shaves seconds off the next
/// meeting's voting phase. The cut is configurable per kill, and the voting
/// time can never drop below a configurable minimum.
///
/// All clients run the same logic deterministically:
/// <see cref="AfterMurderEvent"/> fires on every client when a player is
/// killed, so each client can independently track Silencer kills since the
/// last meeting and apply the same reduction in <see cref="MeetingHud.Update"/>.
/// </summary>
public static class SilencerPatch
{
    /// <summary>Seconds accumulated from Silencer kills since the last meeting.</summary>
    private static float _pendingSeconds;

    /// <summary>Reduction (seconds) chosen at meeting start, clamped against the voting-time floor.</summary>
    private static float _cachedReduction;

    /// <summary>Whether the cached reduction has already been applied to this meeting's timer.</summary>
    private static bool _appliedThisMeeting;

    /// <summary>
    /// Track every kill made by a Silencer. Runs on all clients because
    /// <see cref="AfterMurderEvent"/> follows the murder RPC.
    /// </summary>
    [RegisterEvent]
    public static void OnAfterMurder(AfterMurderEvent evt)
    {
        var source = evt.Source;
        if (source == null || source.Data == null) return;
        if (source.Data.Role is not SilencerRole) return;

        _pendingSeconds += OptionGroupSingleton<SilencerOptions>.Instance.SecondsPerKill;
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
            _pendingSeconds = 0f;
            _cachedReduction = 0f;
            _appliedThisMeeting = false;
        }
    }

    /// <summary>
    /// Cache the reduction for this meeting and reset the kill accumulator.
    /// Clamping happens here so the floor is checked against whatever voting
    /// time is currently configured.
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

            // Headroom = how many seconds we are allowed to take off the voting timer.
            var headroom = Mathf.Max(0f, votingTime - opts.MinimumVotingTime);
            _cachedReduction = Mathf.Clamp(_pendingSeconds, 0f, headroom);
            _pendingSeconds = 0f;
        }
    }

    /// <summary>
    /// Apply the cached reduction the first frame the meeting leaves the
    /// discussion phase. Bumping <see cref="MeetingHud.discussionTimer"/>
    /// forward shortens the voting window without touching discussion time.
    /// </summary>
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
    private static class UpdatePatch
    {
        private static void Postfix(MeetingHud __instance)
        {
            if (_appliedThisMeeting) return;
            if (_cachedReduction <= 0f) return;
            if (__instance == null) return;

            // Wait until vanilla flips the state out of Discussion - that's the
            // moment the voting timer starts ticking, so any bump we add here
            // comes off the voting phase only.
            if (__instance.state == MeetingHud.VoteStates.Discussion) return;

            __instance.discussionTimer += _cachedReduction;
            _appliedThisMeeting = true;
        }
    }
}
