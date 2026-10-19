using HarmonyLib;
using MiraAPI.Modifiers;
using DivaniMods.Assets;
using DivaniMods.Modifiers.Crewmate.CrewmatePower;
using DivaniMods.Roles.Crewmate.CrewmatePower;
using TownOfUs.Extensions;
using TownOfUs.Modules;
using TownOfUs.Utilities;
using UnityEngine;

namespace DivaniMods.Patches;

internal static class MentorTaughtDisplay
{
    private static string? _chunk;

    // " (<icon>)" - the parentheses mark the role as temporary.
    private static string Chunk
    {
        get
        {
            // Builds/registers the TMP sprite asset so the <sprite> tag resolves.
            _ = DivaniAssets.MentorTaughtTmp;

            var hex = ColorUtility.ToHtmlStringRGBA(MentorRole.MentorColor);
            return _chunk ??= $"<color=#{hex}> (</color>{DivaniAssets.MentorTaughtSpriteTag}<color=#{hex}>)</color>";
        }
    }

    internal static bool LocalShouldShow(PlayerControl row)
    {
        var local = PlayerControl.LocalPlayer;
        if (local == null || row == null || local.Data == null)
        {
            return false;
        }

        if (!row.HasModifier<MentorTaughtModifier>())
        {
            return false;
        }

        return local.HasDied()
               || GameHistory.IsFullyDead(local)
               || local.Data.Role is MentorRole
               || local.PlayerId == row.PlayerId;
    }

    internal static void TryAppend(ref string result, PlayerControl row)
    {
        if (!LocalShouldShow(row))
        {
            return;
        }

        var chunk = Chunk;
        if (result.Contains(chunk))
        {
            return;
        }

        result += chunk;
    }
}

[HarmonyPatch(typeof(PlayerRoleTextExtensions), nameof(PlayerRoleTextExtensions.UpdateTargetSymbols),
    new[] { typeof(string), typeof(PlayerControl), typeof(bool) })]
public static class MentorTaughtSymbolPatch
{
    [HarmonyPostfix]
    public static void Postfix(ref string __result, PlayerControl player, bool hidden = false)
    {
        MentorTaughtDisplay.TryAppend(ref __result, player);
    }
}

[HarmonyPatch(typeof(PlayerRoleTextExtensions), nameof(PlayerRoleTextExtensions.UpdateTargetSymbols),
    new[] { typeof(string), typeof(PlayerControl), typeof(DataVisibility) })]
public static class MentorTaughtSymbolDataVisibilityPatch
{
    [HarmonyPostfix]
    public static void Postfix(ref string __result, PlayerControl player, DataVisibility visibility)
    {
        MentorTaughtDisplay.TryAppend(ref __result, player);
    }
}
