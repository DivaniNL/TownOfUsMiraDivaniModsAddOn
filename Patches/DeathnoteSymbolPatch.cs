using HarmonyLib;
using MiraAPI.Modifiers;
using DivaniMods.Modifiers.Impostors;
using DivaniMods.Roles.Impostor.ImpostorKilling;
using TownOfUs.Modules;
using TownOfUs.Utilities;
using UnityEngine;

namespace DivaniMods.Patches;

internal static class DeathnoteDisplay
{
    private const string MarkSymbol = "ø";

    private static string? _markChunk;

    private static string MarkChunk =>
        _markChunk ??=
            $"<color=#{ColorUtility.ToHtmlStringRGBA(Palette.ImpostorRed)}> {MarkSymbol}</color>";

    internal static bool LocalShouldShowMark(PlayerControl row)
    {
        var local = PlayerControl.LocalPlayer;
        if (local == null || row == null || local.Data == null)
        {
            return false;
        }

        if (!row.HasModifier<DeathnoteModifier>())
        {
            return false;
        }

        var isDead = local.HasDied() || GameHistory.IsFullyDead(local);
        return local.Data.Role is DeathnoteRole
               || local.Data.Role.IsImpostor
               || isDead;
    }

    internal static void TryAppendMarkSymbol(ref string result, PlayerControl row)
    {
        if (!LocalShouldShowMark(row))
        {
            return;
        }

        var chunk = MarkChunk;
        if (result.Contains(chunk))
        {
            return;
        }

        result += chunk;
    }
}

[HarmonyPatch(typeof(PlayerRoleTextExtensions), nameof(PlayerRoleTextExtensions.UpdateTargetSymbols),
    new[] { typeof(string), typeof(PlayerControl), typeof(bool) })]
public static class DeathnoteMarkSymbolPatch
{
    [HarmonyPostfix]
    public static void Postfix(ref string __result, PlayerControl player, bool hidden = false)
    {
        DeathnoteDisplay.TryAppendMarkSymbol(ref __result, player);
    }
}

[HarmonyPatch(typeof(PlayerRoleTextExtensions), nameof(PlayerRoleTextExtensions.UpdateTargetSymbols),
    new[] { typeof(string), typeof(PlayerControl), typeof(DataVisibility) })]
public static class DeathnoteMarkSymbolDataVisibilityPatch
{
    [HarmonyPostfix]
    public static void Postfix(ref string __result, PlayerControl player, DataVisibility visibility)
    {
        DeathnoteDisplay.TryAppendMarkSymbol(ref __result, player);
    }
}


