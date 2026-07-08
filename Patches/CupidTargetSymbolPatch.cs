using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using DivaniMods.Modifiers.Neutral.NeutralBenign;
using DivaniMods.Options;
using DivaniMods.Roles.Neutral.NeutralBenign;
using TownOfUs.Modifiers;
using TownOfUs.Utilities;
using UnityEngine;

namespace DivaniMods.Patches;

internal static class CupidProvisionalDisplay
{
    private const string ProvisionalHeartSymbol = "♡";
    private const string LoverHeartSymbol = "♥";

    private const string LoverOneCircle = "<color=#AB30A5>●</color> ";
    private const string LoverTwoCircle = "<color=#25972B>●</color> ";

    private const string OriginalLoverHeart = "<color=#FF66CC> ♥</color>";

    private static string? _provisionalHeartChunk;
    private static string? _loverHeartChunk;

    private static string ProvisionalHeartChunk =>
        _provisionalHeartChunk ??=
            $"<color=#{ColorUtility.ToHtmlStringRGBA(CupidRole.CupidColor)}> {ProvisionalHeartSymbol}</color>";

    private static string LoverHeartChunk =>
        _loverHeartChunk ??=
            $"<color=#{ColorUtility.ToHtmlStringRGBA(CupidRole.CupidColor)}> {LoverHeartSymbol}</color>";

    internal static bool LocalShouldSeeProvisional(PlayerControl row)
    {
        var local = PlayerControl.LocalPlayer;
        if (local == null || row == null || local.Data == null)
        {
            return false;
        }

        if (!row.HasModifier<CupidToBeLoversModifier>())
        {
            return false;
        }

        if (DeathHandlerModifier.IsFullyDead(local))
        {
            return true;
        }

        if (local.Data.Role is CupidRole)
        {
            return true;
        }

        foreach (var mod in row.GetModifiers<CupidToBeLoversModifier>())
        {
            if (local.PlayerId == mod.CupidPlayerId)
            {
                return true;
            }
        }

        return false;
    }

    internal static void TryAppendHeartSymbol(ref string result, PlayerControl row)
    {
        if (LocalShouldSeeProvisional(row))
        {
            if (!result.Contains(ProvisionalHeartChunk))
            {
                result += ProvisionalHeartChunk;
            }
            return;
        }

        if (OwningCupidVisibleTo(row) != null && !result.Contains(LoverHeartChunk))
        {
            result += LoverHeartChunk;
        }
    }

    internal static void TryStripOriginalLoverHeart(ref string result, PlayerControl row)
    {
        if (OwningCupidVisibleTo(row) == null)
        {
            return;
        }

        if (result.Contains(OriginalLoverHeart))
        {
            result = result.Replace(OriginalLoverHeart, string.Empty);
        }
    }

    private static CupidRole? OwningCupidVisibleTo(PlayerControl row)
    {
        if (row == null)
        {
            return null;
        }

        var local = PlayerControl.LocalPlayer;
        if (local == null || local.Data == null)
        {
            return null;
        }

        foreach (var cupid in CustomRoleUtils.GetActiveRolesOfType<CupidRole>())
        {
            if (!cupid.Finalized || cupid.Player == null)
            {
                continue;
            }

            var localIsLover = (cupid.LoverOne != null && cupid.LoverOne.PlayerId == local.PlayerId) ||
                               (cupid.LoverTwo != null && cupid.LoverTwo.PlayerId == local.PlayerId);

            if (!DeathHandlerModifier.IsFullyDead(local) && local.PlayerId != cupid.Player.PlayerId && !localIsLover)
            {
                continue;
            }

            if ((cupid.LoverOne != null && cupid.LoverOne.PlayerId == row.PlayerId) ||
                (cupid.LoverTwo != null && cupid.LoverTwo.PlayerId == row.PlayerId))
            {
                return cupid;
            }
        }

        return null;
    }

    internal static void TryPrependLoverCircle(ref string result, PlayerControl row)
    {
        if (!OptionGroupSingleton<CupidOptions>.Instance.ProtectSeparately)
        {
            return;
        }

        var local = PlayerControl.LocalPlayer;
        if (local == null || local.Data == null || !DeathHandlerModifier.IsFullyDead(local) && local.Data.Role is not CupidRole)
        {
            return;
        }

        var cupid = OwningCupidVisibleTo(row);
        if (cupid == null)
        {
            return;
        }

        string? chunk = null;
        if (cupid.LoverOne != null && cupid.LoverOne.PlayerId == row.PlayerId)
        {
            chunk = LoverOneCircle;
        }
        else if (cupid.LoverTwo != null && cupid.LoverTwo.PlayerId == row.PlayerId)
        {
            chunk = LoverTwoCircle;
        }

        if (chunk == null || result.StartsWith(chunk))
        {
            return;
        }

        result = chunk + result;
    }
}

[HarmonyPatch(typeof(PlayerRoleTextExtensions), nameof(PlayerRoleTextExtensions.UpdateTargetSymbols),
    new[] { typeof(string), typeof(PlayerControl), typeof(bool) })]
public static class CupidTargetSymbolPatch
{
    [HarmonyPostfix]
    public static void Postfix(ref string __result, PlayerControl player, bool hidden = false)
    {
        CupidProvisionalDisplay.TryAppendHeartSymbol(ref __result, player);
        CupidProvisionalDisplay.TryPrependLoverCircle(ref __result, player);
    }
}

[HarmonyPatch(typeof(PlayerRoleTextExtensions), nameof(PlayerRoleTextExtensions.UpdateTargetSymbols),
    new[] { typeof(string), typeof(PlayerControl), typeof(DataVisibility) })]
public static class CupidTargetSymbolDataVisibilityPatch
{
    [HarmonyPostfix]
    public static void Postfix(ref string __result, PlayerControl player, DataVisibility visibility)
    {
        CupidProvisionalDisplay.TryAppendHeartSymbol(ref __result, player);
        CupidProvisionalDisplay.TryPrependLoverCircle(ref __result, player);
    }
}

[HarmonyPatch(typeof(PlayerRoleTextExtensions), nameof(PlayerRoleTextExtensions.UpdateAllianceSymbols),
    new[] { typeof(string), typeof(PlayerControl), typeof(DataVisibility) })]
public static class CupidStripLoverHeartPatch
{
    [HarmonyPostfix]
    public static void Postfix(ref string __result, PlayerControl player, DataVisibility visibility)
    {
        CupidProvisionalDisplay.TryStripOriginalLoverHeart(ref __result, player);
    }
}

[HarmonyPatch(typeof(PlayerRoleTextExtensions), nameof(PlayerRoleTextExtensions.UpdateTargetColor),
    new[] { typeof(Color), typeof(PlayerControl), typeof(DataVisibility) })]
public static class CupidTargetColorPatch
{
    [HarmonyPostfix]
    public static void Postfix(ref Color __result, PlayerControl player, DataVisibility visibility)
    {
        if (CupidProvisionalDisplay.LocalShouldSeeProvisional(player))
        {
            __result = CupidRole.CupidColor;
        }
    }
}
