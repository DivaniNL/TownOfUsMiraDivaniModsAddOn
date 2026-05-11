using HarmonyLib;
using AmongUs.GameOptions;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Meeting;
using DivaniMods.Roles;

namespace DivaniMods.Patches;

/// <summary>
/// First-meeting recruitment: host applies vanilla Impostor at the end of the first meeting.
/// </summary>
[HarmonyPatch]
public static class RecruiterPatch
{
    /// <summary>Number of meetings that have fully ended this game (incremented in <see cref="OnEndMeeting"/>).</summary>
    internal static int MeetingsEnded { get; private set; }

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CoStartGame))]
    [HarmonyPostfix]
    public static void ResetOnGameStart()
    {
        MeetingsEnded = 0;
    }

    [RegisterEvent]
    public static void OnEndMeeting(EndMeetingEvent _)
    {
        var wasFirstMeeting = MeetingsEnded == 0;
        MeetingsEnded++;

        if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost || !wasFirstMeeting)
        {
            return;
        }

        foreach (var pc in PlayerControl.AllPlayerControls)
        {
            if (pc == null || pc.Data == null || pc.Data.Role is not RecruiterRole recruiter)
            {
                continue;
            }

            if (recruiter.Player.Data == null || recruiter.Player.Data.IsDead)
            {
                continue;
            }

            var id = recruiter.PendingRecruitTargetId;
            if (id == byte.MaxValue)
            {
                continue;
            }

            var target = GameData.Instance.GetPlayerById(id)?.Object;
            if (!RecruiterRole.IsValidRecruitTarget(target, recruiter.Player))
            {
                continue;
            }

            target!.RpcSetRole(RoleTypes.Impostor, true);
        }
    }
}
