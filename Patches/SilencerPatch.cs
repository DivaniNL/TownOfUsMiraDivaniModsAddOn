using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Roles;
using DivaniMods.Options;
using DivaniMods.Roles.Impostor.ImpostorKilling;
using TownOfUs.Utilities;
using UnityEngine;

namespace DivaniMods.Patches;


public static class SilencerPatch
{
    /// <summary>Total seconds accumulated from Silencer kills since the game started.</summary>
    private static float _totalKillSeconds;

    /// <summary>Reduction (seconds) chosen at meeting start, clamped against the voting-time floor.</summary>
    private static float _cachedReduction;

    /// <summary>Whether the cached reduction has already been applied to this meeting's timer.</summary>
    private static bool _appliedThisMeeting;

    [RegisterEvent]
    public static void OnAfterMurder(AfterMurderEvent evt)
    {
        var source = evt.Source;
        if (source == null || source.Data == null) return;
        if (source.Data.Role is not SilencerRole) return;

        _totalKillSeconds += OptionGroupSingleton<SilencerOptions>.Instance.SecondsPerKill;
    }

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

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
    private static class StartPatch
    {
        private static void Postfix()
        {
            _appliedThisMeeting = false;

            var opts = OptionGroupSingleton<SilencerOptions>.Instance;
            if (opts.NormalVotingTimeWhenDead && !HasLivingSilencer())
            {
                _totalKillSeconds = 0f;
                _cachedReduction = 0f;
                return;
            }

            var votingTime = GameOptionsManager.Instance != null
                ? GameOptionsManager.Instance.currentNormalGameOptions.VotingTime
                : 0;

            var headroom = Mathf.Max(0f, votingTime - opts.MinimumVotingTime);
            _cachedReduction = Mathf.Clamp(_totalKillSeconds, 0f, headroom);
        }
    }

    private static bool HasLivingSilencer()
    {
        return CustomRoleUtils.GetActiveRolesOfType<SilencerRole>()
            .Any(role => role.Player != null && !role.Player.HasDied());
    }

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
