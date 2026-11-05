using HarmonyLib;
using MiraAPI.GameOptions;
using DivaniMods.Buttons.Crewmate.CrewmateInvestigative;
using DivaniMods.Options;
using DivaniMods.Roles.Crewmate.CrewmateInvestigative;

namespace DivaniMods.Patches;

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
internal static class SentinelMeetingPatch
{
    private static void Postfix()
    {
        var localPlayer = PlayerControl.LocalPlayer;
        if (localPlayer == null || localPlayer.Data == null || localPlayer.Data.IsDead) return;

        if (localPlayer.Data.Role is not SentinelRole) return;

        if (!OptionGroupSingleton<SentinelOptions>.Instance.ShowChatReport) return;

        BeaconManager.ReportBeaconActivity(localPlayer);
    }
}
