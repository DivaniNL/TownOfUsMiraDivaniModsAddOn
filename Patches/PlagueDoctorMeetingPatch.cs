using HarmonyLib;
using DivaniMods.Roles.Neutral.NeutralEvil;
using TownOfUs.Modifiers;
using TownOfUs.Utilities;
using UnityEngine;
using TownOfUs.Modules;

namespace DivaniMods.Patches;

[HarmonyPatch(typeof(PlayerRoleTextExtensions), nameof(PlayerRoleTextExtensions.UpdateTargetSymbols),
    new[] { typeof(string), typeof(PlayerControl), typeof(bool) })]
public static class PlagueDoctorMeetingPatch
{
    private const string InfectedSymbol = "µ";

    [HarmonyPostfix]
    public static void Postfix(ref string __result, PlayerControl player, bool hidden = false)
    {
        if (PlayerControl.LocalPlayer == null) return;

        var localPlayer = PlayerControl.LocalPlayer;

        bool isLocalPD = localPlayer.Data.Role is PlagueDoctorRole ||
                         (PlagueDoctorRole.PlagueDoctorPlayer != null &&
                          localPlayer.PlayerId == PlagueDoctorRole.PlagueDoctorPlayer.PlayerId);

        bool localIsFullyDead = GameHistory.IsFullyDead(localPlayer);

        if (!isLocalPD && !localIsFullyDead) return;

        if (player == null || player.Data == null) return;

        if (PlagueDoctorRole.GetDisplayedInfectionState(player, out _))
        {
            var colorHex = ColorUtility.ToHtmlStringRGBA(PlagueDoctorRole.PlagueDoctorColor);
            __result += $"<color=#{colorHex}> {InfectedSymbol}</color>";
        }
    }
}
