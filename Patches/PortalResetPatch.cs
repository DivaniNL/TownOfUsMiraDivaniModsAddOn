using HarmonyLib;
using DivaniMods.Buttons;
using DivaniMods.Roles;

namespace DivaniMods.Patches;

[HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.CoBegin))]
internal static class PortalResetOnGameStart
{
    private static void Postfix()
    {
        PortalManager.Reset();
        DivaniPlugin.Instance.Log.LogInfo("Portals reset on game start");
    }
}

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
internal static class PortalResetOnGameEnd
{
    private static void Postfix()
    {
        PortalManager.Reset();
        DivaniPlugin.Instance.Log.LogInfo("Portals reset on game end");
    }
}

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
internal static class PortalReportOnMeetingStart
{
    private static void Postfix()
    {
        var localPlayer = PlayerControl.LocalPlayer;
        if (localPlayer == null || localPlayer.Data == null || localPlayer.Data.IsDead) return;
        
        if (localPlayer.Data.Role is not PortalmakerRole) return;
        
        PortalManager.ReportPortalUsage(localPlayer);
    }
}
