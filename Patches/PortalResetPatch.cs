using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Meeting;
using DivaniMods.Buttons.Crewmate.CrewmateSupport;
using DivaniMods.Roles.Crewmate.CrewmateSupport;

namespace DivaniMods.Patches;

internal static class PortalUnlockOnEjection
{
    [RegisterEvent]
    public static void OnEjection(EjectionEvent _)
    {
        if (PortalManager.BothPortalsPlaced)
        {
            PortalManager.PortalsUnlocked = true;
        }
    }
}

[HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.CoBegin))]
internal static class PortalResetOnGameStart
{
    private static void Postfix()
    {
        PortalManager.Reset();
    }
}

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
internal static class PortalResetOnGameEnd
{
    private static void Postfix()
    {
        PortalManager.Reset();
    }
}

[HarmonyPatch(typeof(LobbyBehaviour), nameof(LobbyBehaviour.Start))]
internal static class PortalResetOnLobby
{
    private static void Postfix()
    {
        PortalManager.Reset();
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
