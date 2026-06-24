using HarmonyLib;
using DivaniMods.Events.Impostor.ImpostorSupport;
using DivaniMods.Roles.Impostor.ImpostorSupport;
using TownOfUs.Extensions;
using TownOfUs.Utilities;

namespace DivaniMods.Patches;

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.UpdateTimerText))]
public static class CouncillorMeetingTimerPatch
{
    [HarmonyPostfix]
    public static void Postfix(MeetingHud __instance)
    {
        var local = PlayerControl.LocalPlayer;
        if (!local || !local.Data || local.HasDied())
        {
            return;
        }

        if (local.Data.Role is not CouncillorRole)
        {
            return;
        }

        var total = 1 + CouncillorEvents.GetExtraVotes(local.PlayerId);
        __instance.TimerText.text += $"<color=#FFFFFF>\nCurrent votes this meeting: {total}</color>";
    }
}
