using HarmonyLib;
using MiraAPI.Modifiers;
using DivaniMods.Modifiers.Game.Alliance;
using TownOfUs.Utilities;
using UnityEngine;

namespace DivaniMods.Patches;

[HarmonyPatch]
public static class YinYangSymbolPatch
{
    [HarmonyPatch(typeof(PlayerRoleTextExtensions), nameof(PlayerRoleTextExtensions.UpdateAllianceSymbols),
        typeof(string), typeof(PlayerControl), typeof(DataVisibility))]
    [HarmonyPostfix]
    public static void UpdateAllianceSymbolsPostfix(ref string __result, PlayerControl player,
        DataVisibility visibility)
    {
        if (player == null || !player.TryGetModifier<YinYangModifier>(out var side) ||
            side.Side == YinYangSide.Unassigned)
        {
            return;
        }

        var local = PlayerControl.LocalPlayer;
        var hidden = visibility == DataVisibility.Hidden;
        var reveal = visibility is DataVisibility.Show || (!hidden && local != null && local.HasDied());

        if (!player.AmOwner && !reveal)
        {
            return;
        }

        var hex = ColorUtility.ToHtmlStringRGB(side.SideColor);
        __result += $"<color=#FFFFFF> (<color=#{hex}>{side.ShortName}</color>)</color>";
    }
}
