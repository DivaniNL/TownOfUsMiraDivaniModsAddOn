using HarmonyLib;
using MiraAPI.GameOptions;
using DivaniMods.Buttons;
using DivaniMods.Options;
using DivaniMods.Roles;

namespace DivaniMods.Patches;

/// <summary>
/// Reports beacon activity in chat at the start of each meeting (if option enabled).
/// </summary>
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
