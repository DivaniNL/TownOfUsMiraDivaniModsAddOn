using System.Collections.Generic;
using MiraAPI.Modifiers;
using DivaniMods.Modifiers.Crewmate.CrewmateKilling;
using UnityEngine;

namespace DivaniMods.Events.Crewmate.CrewmateKilling;

public static class RetributionistManager
{
    private static readonly Dictionary<byte, byte> Killers = new();

    private static readonly Dictionary<byte, Vector2> DeathPositions = new();

    public static readonly HashSet<byte> UsedRevenge = new();

    private static readonly HashSet<byte> PendingRevenge = new();

    private static readonly HashSet<byte> ReviveInProgress = new();

    public static bool AnyRevengeActive => Killers.Count > 0 || PendingRevenge.Count > 0;

    public static bool AnyReviveInProgress => ReviveInProgress.Count > 0;

    public static void MarkReviveInProgress(byte soulId)
    {
        ReviveInProgress.Add(soulId);
    }

    public static void ClearReviveInProgress(byte soulId)
    {
        ReviveInProgress.Remove(soulId);
    }

    public static void MarkRevengePending(byte soulId)
    {
        PendingRevenge.Add(soulId);
    }

    public static void ClearRevengePending(byte soulId)
    {
        PendingRevenge.Remove(soulId);
    }

    public static void ClearAllRevengePending()
    {
        PendingRevenge.Clear();
    }

    public static void StartRevenge(byte soulId, byte killerId, Vector2 deathPos)
    {
        PendingRevenge.Remove(soulId);
        Killers[soulId] = killerId;
        DeathPositions[soulId] = deathPos;
        UsedRevenge.Add(soulId);

        var killer = GameData.Instance?.GetPlayerById(killerId)?.Object;
        if (killer != null && !killer.HasModifier<CursedModifier>())
        {
            killer.AddModifier<CursedModifier>();
        }
    }

    public static bool TryGetKiller(byte soulId, out PlayerControl? killer)
    {
        killer = null;
        if (!Killers.TryGetValue(soulId, out var killerId))
        {
            return false;
        }

        killer = GameData.Instance?.GetPlayerById(killerId)?.Object;
        return killer != null;
    }

    public static Vector2 GetDeathPosition(byte soulId)
    {
        return DeathPositions.TryGetValue(soulId, out var pos) ? pos : Vector2.zero;
    }

    public static int GetSoulHunting(byte killerId)
    {
        foreach (var kvp in Killers)
        {
            if (kvp.Value == killerId)
            {
                return kvp.Key;
            }
        }

        return -1;
    }

    public static bool IsRevengeActive(byte soulId)
    {
        return Killers.ContainsKey(soulId);
    }

    public static void RestoreRevengeCharge(byte soulId)
    {
        UsedRevenge.Remove(soulId);
    }

    public static bool IsCursed(byte playerId)
    {
        var player = GameData.Instance?.GetPlayerById(playerId)?.Object;
        return player != null && player.HasModifier<CursedModifier>();
    }

    public static void EndRevenge(byte soulId)
    {
        if (Killers.TryGetValue(soulId, out var killerId))
        {
            var killer = GameData.Instance?.GetPlayerById(killerId)?.Object;
            if (killer != null && killer.HasModifier<CursedModifier>())
            {
                killer.RemoveModifier<CursedModifier>();
            }
        }

        Killers.Remove(soulId);
        DeathPositions.Remove(soulId);

        var soul = GameData.Instance?.GetPlayerById(soulId)?.Object;
        if (soul != null && soul.HasModifier<RevengeTimerModifier>())
        {
            soul.RemoveModifier<RevengeTimerModifier>();
        }
    }

    public static void PurgeDisconnected()
    {
        foreach (var soulId in new List<byte>(Killers.Keys))
        {
            if (IsGone(soulId))
            {
                EndRevenge(soulId);
            }
        }

        foreach (var soulId in new List<byte>(PendingRevenge))
        {
            if (IsGone(soulId))
            {
                PendingRevenge.Remove(soulId);
            }
        }

        foreach (var soulId in new List<byte>(ReviveInProgress))
        {
            if (IsGone(soulId))
            {
                ReviveInProgress.Remove(soulId);
            }
        }
    }

    private static bool IsGone(byte playerId)
    {
        var data = GameData.Instance?.GetPlayerById(playerId);
        return data == null || data.Disconnected || data.Object == null;
    }

    public static void Reset()
    {
        Killers.Clear();
        DeathPositions.Clear();
        UsedRevenge.Clear();
        PendingRevenge.Clear();
        ReviveInProgress.Clear();
    }
}
