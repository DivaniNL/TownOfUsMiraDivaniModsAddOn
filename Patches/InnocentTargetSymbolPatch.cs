using HarmonyLib;
using DivaniMods.Roles;
using TownOfUs.Utilities;
using UnityEngine;

namespace DivaniMods.Patches;

[HarmonyPatch(typeof(PlayerRoleTextExtensions), nameof(PlayerRoleTextExtensions.UpdateTargetSymbols),
    new[] { typeof(string), typeof(PlayerControl), typeof(bool) })]
public static class InnocentTargetSymbolPatch
{
    private const string TauntedTargetSymbol = "⊕";

    [HarmonyPostfix]
    public static void Postfix(ref string __result, PlayerControl player, bool hidden = false)
    {
        if (!InnocentTargetColorPatch.ShouldHighlightTarget(player))
        {
            return;
        }

        var colorHex = ColorUtility.ToHtmlStringRGBA(InnocentRole.InnocentColor);
        __result += $"<color=#{colorHex}> {TauntedTargetSymbol}</color>";
    }
}

[HarmonyPatch(typeof(PlayerRoleTextExtensions), nameof(PlayerRoleTextExtensions.UpdateTargetColor),
    new[] { typeof(Color), typeof(PlayerControl), typeof(DataVisibility) })]
public static class InnocentTargetColorPatch
{
    [HarmonyPostfix]
    public static void Postfix(ref Color __result, PlayerControl player, DataVisibility visibility)
    {
        if (ShouldHighlightTarget(player))
        {
            __result = InnocentRole.InnocentColor;
        }
    }

    public static bool ShouldHighlightTarget(PlayerControl player)
    {
        var localPlayer = PlayerControl.LocalPlayer;
        if (localPlayer == null || player == null)
        {
            return false;
        }

        if (!InnocentRole.ActiveInnocents.TryGetValue(localPlayer.PlayerId, out var innocent))
        {
            return false;
        }

        return innocent.ShowTauntedTargetSymbol && innocent.TauntedKillerId == player.PlayerId;
    }
}
