using HarmonyLib;
using Reactor.Utilities;
using DivaniMods.Assets;
using DivaniMods.Buttons.Crewmate.CrewmateInvestigative;
using DivaniMods.Roles.Crewmate.CrewmateInvestigative;
using System.Collections;
using UnityEngine;

namespace DivaniMods.Patches;

[HarmonyPatch]
public static class SentinelPatch
{
    private static bool _wasInMeeting;

    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.CoBegin))]
    [HarmonyPostfix]
    public static void ResetOnGameStart()
    {
        BeaconManager.Reset();
    }

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
    [HarmonyPostfix]
    public static void ResetOnGameEnd()
    {
        BeaconManager.Reset();
    }

    [HarmonyPatch(typeof(LobbyBehaviour), nameof(LobbyBehaviour.Start))]
    [HarmonyPostfix]
    public static void ResetOnLobby()
    {
        BeaconManager.Reset();
    }

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    [HarmonyPostfix]
    public static void HudManagerUpdate(HudManager __instance)
    {
        var localPlayer = PlayerControl.LocalPlayer;
        if (localPlayer == null || localPlayer.Data == null) return;

        bool isSentinel = localPlayer.Data.Role is SentinelRole;

        if (!isSentinel) return;
        if (localPlayer.Data.IsDead) return;

        if (MeetingHud.Instance || ExileController.Instance)
        {
            _wasInMeeting = true;
            return;
        }

        if (_wasInMeeting)
        {
            _wasInMeeting = false;
            BeaconManager.ReseedAllBeacons();
            return;
        }

        if (PlayerTask.PlayerHasTaskOfType<IHudOverrideTask>(localPlayer)) return;
        if (BeaconManager.BeaconsPlaced == 0) return;

        var newEntries = BeaconManager.UpdatePlayerTracking();

        foreach (var (beacon, playerName) in newEntries)
        {
            Coroutines.Start(CoFlashSentinel());

            char label = (char)('A' + BeaconManager.Beacons.IndexOf(beacon));
            var colorHex = ColorUtility.ToHtmlStringRGB(SentinelRole.SentinelColor);
            MiraAPI.Utilities.Helpers.CreateAndShowNotification(
                $"<b><color=#{colorHex}>Someone walked through Beacon {label} ({beacon.RoomName})</color></b>",
                Color.white,
                new Vector3(0f, 1f, -20f),
                spr: DivaniAssets.SentinelIcon.LoadAsset());
        }
    }

    private static IEnumerator CoFlashSentinel()
    {
        if (!HudManager.Instance) yield break;

        var overlay = UnityEngine.Object.Instantiate(HudManager.Instance.FullScreen, HudManager.Instance.transform);
        overlay.transform.localScale = Vector3.one * 10f;
        overlay.color = new Color(
            SentinelRole.SentinelColor.r,
            SentinelRole.SentinelColor.g,
            SentinelRole.SentinelColor.b,
            0.3f);
        overlay.gameObject.SetActive(true);
        overlay.enabled = true;

        yield return new WaitForSeconds(0.5f);

        if (overlay != null)
        {
            UnityEngine.Object.Destroy(overlay.gameObject);
        }
    }
}
