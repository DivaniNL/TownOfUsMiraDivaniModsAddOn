using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using DivaniMods.Modifiers.Crewmate.CrewmateSupport;
using DivaniMods.Options;
using DivaniMods.Patches;
using DivaniMods.Roles.Crewmate.CrewmateSupport;
using TownOfUs.Patches.Options;
using TownOfUs.Utilities;

namespace DivaniMods.Events.Crewmate.CrewmateSupport;

public static class TelecomEvents
{
    private static bool MeetingMode =>
        OptionGroupSingleton<TelecomOptions>.Instance.TargetSelection.Value ==
        (int)TelecomTargetSelectionOptions.Meeting;

    [RegisterEvent]
    public static void OnRoundStart(RoundStartEvent evt)
    {
        if (!evt.TriggeredByIntro && AmongUsClient.Instance && AmongUsClient.Instance.AmHost)
        {
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (pc == null || pc.Data == null || pc.Data.Role is not TelecomRole role || role.TargetId == byte.MaxValue)
                {
                    continue;
                }

                var target = GameData.Instance.GetPlayerById(role.TargetId)?.Object;
                var targetGone = target == null || target.Data == null || target.Data.IsDead || target.Data.Disconnected;
                if (pc.HasDied() || targetGone)
                {
                    TelecomRole.RpcClearTransmission(pc);
                }
            }
        }

        var local = PlayerControl.LocalPlayer;
        if (local == null || local.Data == null)
        {
            return;
        }

        if (local.Data.Role is TelecomRole localRole)
        {
            localRole.TransmittedThisRound = false;
        }

        if (!local.Data.IsDead && local.HasModifier<TelecomChatModifier>())
        {
            TelecomChatModifier.ShowTelecomChat();
        }
    }

    [RegisterEvent]
    public static void OnMeetingStart(StartMeetingEvent _)
    {
        RoundChatManager.HideSwitchButton();

        if (TeamChatPatches.PrivateChatDot != null)
        {
            TeamChatPatches.PrivateChatDot.enabled = false;
        }

        var local = PlayerControl.LocalPlayer;
        if (local != null && local.Data != null && !local.Data.IsDead && local.HasModifier<TelecomChatModifier>())
        {
            TeamChatPatches.ForceNormalChat();
        }
    }

    [RegisterEvent]
    public static void OnEndMeeting(EndMeetingEvent _)
    {
        if (!AmongUsClient.Instance || !AmongUsClient.Instance.AmHost)
        {
            return;
        }

        if (!MeetingMode)
        {
            return;
        }

        foreach (var pc in PlayerControl.AllPlayerControls)
        {
            if (pc == null || pc.Data == null || pc.Data.Role is not TelecomRole role || pc.HasDied())
            {
                continue;
            }

            if (role.PendingMeetingTargetId != byte.MaxValue)
            {
                var pendingId = role.PendingMeetingTargetId;
                role.PendingMeetingTargetId = byte.MaxValue;

                var pending = GameData.Instance.GetPlayerById(pendingId)?.Object;
                if (TelecomRole.IsValidTarget(pending, pc))
                {
                    TelecomRole.RpcSetTransmission(pc, pendingId);
                }
            }
        }
    }
}
