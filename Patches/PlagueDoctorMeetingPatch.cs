using HarmonyLib;
using DivaniMods.Assets;
using DivaniMods.Roles;
using UnityEngine;

namespace DivaniMods.Patches;

/// <summary>
/// Shows a biohazard indicator next to fully-infected players during meetings.
/// Only the Plague Doctor sees these icons. Icons are positioned so they don't
/// overlap with other meeting indicators (e.g. medic shield plus).
/// </summary>
[HarmonyPatch]
public static class PlagueDoctorMeetingPatch
{
    /// <summary>Name of the child GameObject we create on each vote area.</summary>
    private const string IndicatorName = "DivaniMods_InfectedIndicator";

    /// <summary>
    /// Local position of the biohazard icon relative to the PlayerVoteArea.
    /// Placed to the right of the name text, outside the slot where the medic
    /// plus icon normally appears.
    /// </summary>
    private static readonly Vector3 IndicatorLocalPos = new(1.05f, 0.03f, -2f);

    /// <summary>
    /// Scale of the biohazard icon. Tuned so it appears roughly the same size
    /// as the medic plus icon shown in meetings (~0.35 world units wide).
    /// The source image is 960x960 @ 300 ppu, so scale 0.11 gives ~0.35 units.
    /// </summary>
    private static readonly Vector3 IndicatorScale = new(0.11f, 0.11f, 1f);

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
    [HarmonyPostfix]
    public static void MeetingHud_Start_Postfix(MeetingHud __instance)
    {
        RefreshAllInfectedIndicators(__instance);
    }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
    [HarmonyPostfix]
    public static void MeetingHud_Update_Postfix(MeetingHud __instance)
    {
        // Cheap per-frame keep-alive in case something removed our icon (e.g.
        // vote area rebuild) or if infections update during the meeting.
        RefreshAllInfectedIndicators(__instance);
    }

    private static void RefreshAllInfectedIndicators(MeetingHud meetingHud)
    {
        if (meetingHud == null) return;
        if (meetingHud.playerStates == null) return;

        var localPlayer = PlayerControl.LocalPlayer;
        if (localPlayer == null) return;

        // Only the Plague Doctor should see these icons.
        bool isLocalPD = PlagueDoctorRole.PlagueDoctorPlayer != null &&
                         localPlayer == PlagueDoctorRole.PlagueDoctorPlayer;

        foreach (var voteArea in meetingHud.playerStates)
        {
            if (voteArea == null) continue;

            bool infected = PlagueDoctorRole.InfectedPlayers.ContainsKey(voteArea.TargetPlayerId);
            bool shouldShow = isLocalPD && infected;

            var existing = FindIndicator(voteArea);
            if (shouldShow)
            {
                if (existing == null)
                {
                    CreateIndicator(voteArea);
                }
                else if (!existing.activeSelf)
                {
                    existing.SetActive(true);
                }
            }
            else if (existing != null)
            {
                existing.SetActive(false);
            }
        }
    }

    private static GameObject? FindIndicator(PlayerVoteArea voteArea)
    {
        var t = voteArea.transform.Find(IndicatorName);
        return t != null ? t.gameObject : null;
    }

    private static void CreateIndicator(PlayerVoteArea voteArea)
    {
        var go = new GameObject(IndicatorName);
        go.transform.SetParent(voteArea.transform, worldPositionStays: false);
        go.transform.localPosition = IndicatorLocalPos;
        go.transform.localScale = IndicatorScale;
        go.layer = voteArea.gameObject.layer;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = DivaniAssets.InfectedIcon.LoadAsset();

        // Render above the nameplate/background so the icon is actually visible.
        var nameText = voteArea.NameText;
        if (nameText != null)
        {
            sr.sortingLayerID = nameText.sortingLayerID;
            sr.sortingOrder = nameText.sortingOrder + 1;
        }
        else
        {
            sr.sortingOrder = 10;
        }
    }
}
