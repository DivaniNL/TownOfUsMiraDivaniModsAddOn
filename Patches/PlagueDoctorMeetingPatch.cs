using HarmonyLib;
using DivaniMods.Roles.Neutral.NeutralEvil;
using TownOfUs.Modifiers;
using TownOfUs.Utilities;
using UnityEngine;

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

        bool localIsFullyDead = DeathHandlerModifier.IsFullyDead(localPlayer);

        if (!isLocalPD && !localIsFullyDead) return;

        if (player == null || player.Data == null) return;

        if (PlagueDoctorRole.IsInfected(player))
        {
            var colorHex = ColorUtility.ToHtmlStringRGBA(PlagueDoctorRole.PlagueDoctorColor);
            __result += $"<color=#{colorHex}> {InfectedSymbol}</color>";
        }
    }
}
