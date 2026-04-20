using HarmonyLib;
using DivaniMods.Roles;
using TownOfUs.Utilities;
using UnityEngine;

namespace DivaniMods.Patches;

/// <summary>
/// Appends a biohazard symbol (☣) next to fully-infected players' names during
/// meetings and on the HUD. Uses the same text-symbol approach as the medic
/// shield (+), executioner target (X), etc., so the indicator renders below the
/// minimap overlay instead of on top of it.
/// </summary>
[HarmonyPatch(typeof(PlayerRoleTextExtensions), nameof(PlayerRoleTextExtensions.UpdateTargetSymbols))]
public static class PlagueDoctorMeetingPatch
{
    private const string InfectedSymbol = "µ";

    [HarmonyPostfix]
    public static void Postfix(ref string __result, PlayerControl player, bool hidden = false)
    {
        if (PlayerControl.LocalPlayer == null) return;

        var localPlayer = PlayerControl.LocalPlayer;

        // Only the Plague Doctor should see infected symbols.
        bool isLocalPD = localPlayer.Data.Role is PlagueDoctorRole ||
                         (PlagueDoctorRole.PlagueDoctorPlayer != null &&
                          localPlayer.PlayerId == PlagueDoctorRole.PlagueDoctorPlayer.PlayerId);

        if (!isLocalPD) return;

        if (player == null || player.Data == null) return;

        if (PlagueDoctorRole.InfectedPlayers.ContainsKey(player.PlayerId))
        {
            var colorHex = ColorUtility.ToHtmlStringRGBA(PlagueDoctorRole.PlagueDoctorColor);
            __result += $"<color=#{colorHex}> {InfectedSymbol}</color>";
        }
    }
}
