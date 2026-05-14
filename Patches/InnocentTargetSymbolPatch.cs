using System.Linq;
using HarmonyLib;
using MiraAPI.Modifiers;
using DivaniMods.Modifiers.Neutral.NeutralEvil;
using DivaniMods.Roles.Neutral.NeutralEvil;
using TownOfUs.Utilities;
using UnityEngine;

namespace DivaniMods.Patches;

/// <summary>
/// Meeting nameplate tint + ⊕ for the taunted killer. Shared helpers keep symbol + color paths in sync.
/// </summary>
internal static class InnocentTauntMeetingDisplay
{
    private const string TauntSymbol = "⊕";

    private static string? _tauntSymbolRichChunk;

    private static string TauntSymbolRichChunk =>
        _tauntSymbolRichChunk ??=
            $"<color=#{ColorUtility.ToHtmlStringRGBA(InnocentRole.InnocentColor)}> {TauntSymbol}</color>";

    internal static bool LocalShouldHighlightTauntTarget(PlayerControl row)
    {
        var local = PlayerControl.LocalPlayer;
        if (local == null || row == null || local.Data == null)
        {
            return false;
        }

        foreach (var mod in row.GetModifiers<InnocentTargetModifier>())
        {
            if (!InnocentRole.ActiveInnocents.TryGetValue(mod.InnocentPlayerId, out var innocent))
            {
                continue;
            }

            if (innocent.TauntedKillerId != row.PlayerId)
            {
                continue;
            }

            if (local.Data.IsDead)
            {
                return true;
            }

            if (local.PlayerId == mod.InnocentPlayerId)
            {
                return true;
            }
        }

        return false;
    }

    internal static void TryAppendTauntSymbol(ref string result, PlayerControl row)
    {
        if (!LocalShouldHighlightTauntTarget(row))
        {
            return;
        }

        // Both UpdateTargetSymbols(bool) and UpdateTargetSymbols(DataVisibility) can run for the same row in one refresh.
        var chunk = TauntSymbolRichChunk;
        if (result.Contains(chunk))
        {
            return;
        }

        result += chunk;
    }

    /// <summary>True if <paramref name="killer"/> still carries the taunt marker for this innocent.</summary>
    internal static bool KillerHasTauntMarkerForInnocent(PlayerControl killer, byte innocentPlayerId)
    {
        return killer.GetModifiers<InnocentTargetModifier>().Any(m => m.InnocentPlayerId == innocentPlayerId);
    }
}

[HarmonyPatch(typeof(PlayerRoleTextExtensions), nameof(PlayerRoleTextExtensions.UpdateTargetSymbols),
    new[] { typeof(string), typeof(PlayerControl), typeof(bool) })]
public static class InnocentTargetSymbolPatch
{
    [HarmonyPostfix]
    public static void Postfix(ref string __result, PlayerControl player, bool hidden = false)
    {
        InnocentTauntMeetingDisplay.TryAppendTauntSymbol(ref __result, player);
    }
}

/// <summary>Ghost / dead UI calls the <see cref="DataVisibility"/> overload, not the bool one.</summary>
[HarmonyPatch(typeof(PlayerRoleTextExtensions), nameof(PlayerRoleTextExtensions.UpdateTargetSymbols),
    new[] { typeof(string), typeof(PlayerControl), typeof(DataVisibility) })]
public static class InnocentTargetSymbolDataVisibilityPatch
{
    [HarmonyPostfix]
    public static void Postfix(ref string __result, PlayerControl player, DataVisibility visibility)
    {
        InnocentTauntMeetingDisplay.TryAppendTauntSymbol(ref __result, player);
    }
}

[HarmonyPatch(typeof(PlayerRoleTextExtensions), nameof(PlayerRoleTextExtensions.UpdateTargetColor),
    new[] { typeof(Color), typeof(PlayerControl), typeof(DataVisibility) })]
public static class InnocentTargetColorPatch
{
    [HarmonyPostfix]
    public static void Postfix(ref Color __result, PlayerControl player, DataVisibility visibility)
    {
        if (InnocentTauntMeetingDisplay.LocalShouldHighlightTauntTarget(player))
        {
            __result = InnocentRole.InnocentColor;
        }
    }
}
