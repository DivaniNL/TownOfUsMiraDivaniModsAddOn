using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using DivaniMods.Modifiers.Crewmate.CrewmateSupport;
using DivaniMods.Options;
using DivaniMods.Roles.Crewmate.CrewmateSupport;
using TownOfUs.Modifiers;
using TownOfUs.Utilities;
using UnityEngine;

namespace DivaniMods.Patches;

internal static class LocatorMarkDisplay
{
    private const string MarkSymbol = "※";

    private static string? _markChunk;

    private static string MarkChunk =>
        _markChunk ??=
            $"<color=#{ColorUtility.ToHtmlStringRGBA(LocatorRole.LocatorColor)}> {MarkSymbol}</color>";

    internal static bool LocalShouldShowMark(PlayerControl row)
    {
        var local = PlayerControl.LocalPlayer;
        if (local == null || row == null || local.Data == null)
        {
            return false;
        }

        if (!row.HasModifier<LocatorMarkModifier>())
        {
            return false;
        }

        return DeathHandlerModifier.IsFullyDead(local)
               || local.Data.Role is LocatorRole
               || (local == row && OptionGroupSingleton<LocatorOptions>.Instance.TargetKnows);
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
public static class LocatorMarkSymbolPatch
{
    [HarmonyPostfix]
    public static void Postfix(ref string __result, PlayerControl player, bool hidden = false)
    {
        LocatorMarkDisplay.TryAppendMarkSymbol(ref __result, player);
    }
}

[HarmonyPatch(typeof(PlayerRoleTextExtensions), nameof(PlayerRoleTextExtensions.UpdateTargetSymbols),
    new[] { typeof(string), typeof(PlayerControl), typeof(DataVisibility) })]
public static class LocatorMarkSymbolDataVisibilityPatch
{
    [HarmonyPostfix]
    public static void Postfix(ref string __result, PlayerControl player, DataVisibility visibility)
    {
        LocatorMarkDisplay.TryAppendMarkSymbol(ref __result, player);
    }
}
