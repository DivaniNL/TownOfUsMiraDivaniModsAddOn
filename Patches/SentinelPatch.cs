using HarmonyLib;
using Reactor.Utilities;
using DivaniMods.Assets;
using DivaniMods.Buttons;
using DivaniMods.Roles;
using System.Collections;
using UnityEngine;

namespace DivaniMods.Patches;

/// <summary>
/// Handles Sentinel beacon tracking each frame and triggers flashes on new room entries.
/// Also handles button visibility (only show when in a valid room).
/// </summary>
[HarmonyPatch]
public static class SentinelPatch
{
    /// <summary>
    /// Reset beacons at game start.
    /// </summary>
    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.CoBegin))]
    [HarmonyPostfix]
    public static void ResetOnGameStart()
    {
        BeaconManager.Reset();
    }

    /// <summary>
    /// Reset beacons at game end.
    /// </summary>
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
    [HarmonyPostfix]
    public static void ResetOnGameEnd()
    {
        BeaconManager.Reset();
    }

    /// <summary>
    /// Main update loop: track player room entries and trigger flashes for the Sentinel.
    /// Also controls button visibility.
    /// </summary>
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    [HarmonyPostfix]
    public static void HudManagerUpdate(HudManager __instance)
    {
        var localPlayer = PlayerControl.LocalPlayer;
        if (localPlayer == null || localPlayer.Data == null) return;

        bool isSentinel = localPlayer.Data.Role is SentinelRole;

        // Button visibility: only show when in a valid room
        UpdateButtonVisibility(localPlayer, isSentinel);

        // Only Sentinel tracks beacons
        if (!isSentinel) return;
        if (localPlayer.Data.IsDead) return;

        // Don't track during meetings or comms sabotage
        if (MeetingHud.Instance != null || ExileController.Instance != null) return;
        if (PlayerTask.PlayerHasTaskOfType<IHudOverrideTask>(localPlayer)) return;
        if (BeaconManager.BeaconsPlaced == 0) return;

        var newEntries = BeaconManager.UpdatePlayerTracking();

        // Flash + notification for each new room entry
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

    /// <summary>
    /// Show/hide the Place Beacon button based on whether the player is in a valid room.
    /// </summary>
    private static void UpdateButtonVisibility(PlayerControl player, bool isSentinel)
    {
        var buttonInstance = PlaceBeaconButton.Instance;
        if (buttonInstance?.Button == null) return;

        if (!isSentinel || player.Data.IsDead)
        {
            buttonInstance.Button.gameObject.SetActive(false);
            return;
        }

        // Hide during meetings
        if (MeetingHud.Instance || ExileController.Instance)
        {
            buttonInstance.Button.gameObject.SetActive(false);
            return;
        }

        var position = player.GetTruePosition();
        bool inRoom = BeaconManager.IsInRoom(position);
        
        if (inRoom)
        {
            buttonInstance.Button.gameObject.SetActive(true);
            
            // Show disabled state during comms sabotage
            if (PlayerTask.PlayerHasTaskOfType<IHudOverrideTask>(player))
            {
                buttonInstance.Button.SetDisabled();
            }
        }
        else
        {
            buttonInstance.Button.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Full-screen flash in Sentinel color. Uses same pattern as Medic flash (MiscUtils.CoFlash).
    /// </summary>
    private static IEnumerator CoFlashSentinel()
    {
        if (HudManager.Instance == null) yield break;

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
