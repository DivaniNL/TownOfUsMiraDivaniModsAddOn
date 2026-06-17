using System.Collections;
using System.Collections.Generic;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using Reactor.Utilities;
using DivaniMods.Assets;
using DivaniMods.Modifiers.Neutral.NeutralOutlier;
using DivaniMods.Options;
using DivaniMods.Roles.Neutral.NeutralOutlier;
using TownOfUs.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DivaniMods.Modules.Duelist;

public static class DuelManager
{
    private static readonly Dictionary<byte, int> Wins = new();
    private static readonly Dictionary<byte, int> Losses = new();
    private static readonly HashSet<byte> ActiveDuelers = new();
    private static readonly HashSet<byte> DuelDeaths = new();
    private static readonly HashSet<byte> Resolved = new();
    private static readonly HashSet<byte> Struck = new();

    public static bool IsInDuel(byte playerId) => ActiveDuelers.Contains(playerId);

    public static void MarkInDuel(byte playerId)
    {
        ActiveDuelers.Add(playerId);
        Resolved.Remove(playerId);
        Struck.Remove(playerId);
    }

    public static void ClearActiveDuelers()
    {
        ActiveDuelers.Clear();
        Resolved.Clear();
        Struck.Clear();
    }

    public static bool IsResolved(byte playerId) => Resolved.Contains(playerId);
    public static void MarkResolved(byte a, byte b)
    {
        Resolved.Add(a);
        Resolved.Add(b);
    }

    public static bool HasStruck(byte playerId) => Struck.Contains(playerId);
    public static void MarkStruck(byte playerId) => Struck.Add(playerId);

    public static bool DiedInDuel(byte playerId) => DuelDeaths.Contains(playerId);
    public static void MarkDuelDeath(byte playerId) => DuelDeaths.Add(playerId);

    public static int GetWins(byte playerId) => Wins.TryGetValue(playerId, out var v) ? v : 0;
    public static int GetLosses(byte playerId) => Losses.TryGetValue(playerId, out var v) ? v : 0;
    public static void AddWin(byte playerId) => Wins[playerId] = GetWins(playerId) + 1;
    public static void AddLoss(byte playerId) => Losses[playerId] = GetLosses(playerId) + 1;

    public static void RefundLoss(byte playerId)
    {
        if (Losses.TryGetValue(playerId, out var v) && v > 0)
        {
            Losses[playerId] = v - 1;
        }
        DuelDeaths.Remove(playerId);
    }

    public static void ResetAll()
    {
        Wins.Clear();
        Losses.Clear();
        ActiveDuelers.Clear();
        DuelDeaths.Clear();
        Resolved.Clear();
        Struck.Clear();
    }

    public static bool TryGetDuelDestinations(PlayerControl duelist, PlayerControl target,
        out Vector2 duelistDest, out Vector2 targetDest)
    {
        duelistDest = duelist.GetTruePosition();
        targetDest = target.GetTruePosition();

        if (ShipStatus.Instance == null)
        {
            return false;
        }

        var spawnType = (DuelSpawnType)OptionGroupSingleton<DuelistOptions>.Instance.SpawnType.Value;

        if (spawnType == DuelSpawnType.Far &&
            DuelRoomPositions.TryGetFarRooms(out var farA, out var farB) &&
            TryGetRoomDest(farA, out var farDestA) && TryGetRoomDest(farB, out var farDestB))
        {
            duelistDest = farDestA;
            targetDest = farDestB;
            return true;
        }

        if (spawnType == DuelSpawnType.Random && TryGetRandomDestinations(out var randA, out var randB))
        {
            duelistDest = randA;
            targetDest = randB;
            return true;
        }

        var from = duelist.GetTruePosition();
        var currentRoom = GetRoomAt(from);

        var rooms = new List<PlainShipRoom>();
        foreach (var room in ShipStatus.Instance.FastRooms.Values)
        {
            if (room == null || room.roomArea == null)
            {
                continue;
            }
            if (currentRoom != null && room.RoomId == currentRoom.RoomId)
            {
                continue;
            }
            rooms.Add(room);
        }

        if (rooms.Count < 2)
        {
            return false;
        }

        rooms.Sort((a, b) =>
            Vector2.Distance(from, a.roomArea.bounds.center)
                .CompareTo(Vector2.Distance(from, b.roomArea.bounds.center)));

        duelistDest = GetRoomDest(rooms[0]);
        targetDest = GetRoomDest(rooms[1]);
        return true;
    }

    private static bool TryGetRoomDest(SystemTypes room, out Vector2 dest)
    {
        dest = Vector2.zero;

        if (DuelRoomPositions.TryGet(room, out var pos))
        {
            dest = new Vector2(pos.x, pos.y + 0.3636f);
            return true;
        }

        foreach (var r in ShipStatus.Instance.FastRooms.Values)
        {
            if (r != null && r.roomArea != null && r.RoomId == room)
            {
                dest = r.roomArea.bounds.center;
                return true;
            }
        }

        return false;
    }

    private static bool TryGetRandomDestinations(out Vector2 destA, out Vector2 destB)
    {
        destA = Vector2.zero;
        destB = Vector2.zero;

        var rooms = DuelRoomPositions.GetAllRooms();
        rooms.Shuffle();

        var found = new List<Vector2>();
        foreach (var room in rooms)
        {
            if (TryGetRoomDest(room, out var dest))
            {
                found.Add(dest);
            }
            if (found.Count == 2)
            {
                break;
            }
        }

        if (found.Count < 2)
        {
            return false;
        }

        destA = found[0];
        destB = found[1];
        return true;
    }

    private static PlainShipRoom? GetRoomAt(Vector2 pos)
    {
        foreach (var room in ShipStatus.Instance.FastRooms.Values)
        {
            if (room != null && room.roomArea != null && room.roomArea.OverlapPoint(pos))
            {
                return room;
            }
        }
        return null;
    }

    private static Vector2 GetRoomDest(PlainShipRoom room)
    {
        return DuelRoomPositions.TryGet(room.RoomId, out var pos)
            ? new Vector2(pos.x, pos.y + 0.3636f)
            : room.roomArea.bounds.center;
    }

    public static void EndDuel(PlayerControl winner, PlayerControl loser, bool loserDied)
    {
        if (winner == null || loser == null)
        {
            return;
        }

        ShowResultNotifs(winner, loser);
        Coroutines.Start(CoEndDuel(winner, loser, loserDied));
    }

    private static IEnumerator CoEndDuel(PlayerControl winner, PlayerControl loser, bool loserDied)
    {
        yield return new WaitForSeconds(0.5f);

        if (winner.TryGetModifier<DuelModifier>(out var winnerMod))
        {
            TeleportBack(winner, winnerMod.ReturnPos);
        }

        if (!loserDied && loser.TryGetModifier<DuelModifier>(out var loserMod))
        {
            TeleportBack(loser, loserMod.ReturnPos);
        }

        RemoveDuel(winner);
        RemoveDuel(loser);

        ApplyReturnInvisibility(winner);
        ApplyReturnInvisibility(loser);
        ShowReturnNotif(winner, loser);
    }

    private static void ApplyReturnInvisibility(PlayerControl player)
    {
        if (player == null || player.HasDied())
        {
            return;
        }

        if (player.TryGetComponent<ModifierComponent>(out var comp) && !player.HasModifier<DuelReturnInvisModifier>())
        {
            comp.AddModifier(new DuelReturnInvisModifier());
        }
    }

    private static void ShowReturnNotif(PlayerControl winner, PlayerControl loser)
    {
        Coroutines.Start(CoReturnCountdown(winner, loser));
    }

    private static IEnumerator CoReturnCountdown(PlayerControl winner, PlayerControl loser)
    {
        var hex = ColorUtility.ToHtmlStringRGB(DuelistRole.DuelistColor);
        var icon = DivaniAssets.DuelistIcon.LoadAsset();

        for (var seconds = 5; seconds >= 1; seconds--)
        {
            var local = PlayerControl.LocalPlayer;
            if (local == null || local.HasDied() ||
                (local.PlayerId != winner.PlayerId && local.PlayerId != loser.PlayerId))
            {
                yield break;
            }

            var unit = seconds == 1 ? "second" : "seconds";
            MiraAPI.Utilities.Helpers.CreateAndShowNotification(
                $"<b><color=#{hex}>Returning to the map in {seconds} {unit}</color></b>", Color.white,
                new Vector3(0f, 1f, -20f), spr: icon);

            yield return new WaitForSeconds(1f);
        }
    }

    public static void AbortDuel(PlayerControl player)
    {
        if (player == null || !player.TryGetModifier<DuelModifier>(out var mod))
        {
            return;
        }

        var opponent = MiscUtils.PlayerById(mod.OpponentId);
        TeleportBack(player, mod.ReturnPos);
        RemoveDuel(player);
        if (opponent != null)
        {
            RemoveDuel(opponent);
        }
    }

    private static void ShowResultNotifs(PlayerControl winner, PlayerControl loser)
    {
        var local = PlayerControl.LocalPlayer;
        if (local == null)
        {
            return;
        }

        var hex = ColorUtility.ToHtmlStringRGB(DuelistRole.DuelistColor);
        var icon = DivaniAssets.DuelistIcon.LoadAsset();

        if (local.PlayerId == winner.PlayerId)
        {
            MiraAPI.Utilities.Helpers.CreateAndShowNotification(
                $"<b><color=#{hex}>You won the duel!</color></b>", Color.white,
                new Vector3(0f, 1f, -20f), spr: icon);
            Coroutines.Start(MiscUtils.CoFlash(Color.green, alpha: 0.4f));
        }
        else if (local.PlayerId == loser.PlayerId)
        {
            MiraAPI.Utilities.Helpers.CreateAndShowNotification(
                $"<b><color=#{hex}>You lost the duel!</color></b>", Color.white,
                new Vector3(0f, 1f, -20f), spr: icon);
            Coroutines.Start(MiscUtils.CoFlash(Color.red, alpha: 0.4f));
        }
    }

    private static void TeleportBack(PlayerControl player, Vector2 pos)
    {
        if (player == null || player.HasDied())
        {
            return;
        }

        player.MyPhysics.ResetMoveState();
        player.transform.position = pos;
        if (player.AmOwner)
        {
            player.NetTransform.RpcSnapTo(pos);
            MiscUtils.SnapPlayerCamera(player);
        }
    }

    private static void RemoveDuel(PlayerControl player)
    {
        if (player == null)
        {
            return;
        }

        ActiveDuelers.Remove(player.PlayerId);
        Resolved.Remove(player.PlayerId);
        Struck.Remove(player.PlayerId);
        if (player.TryGetModifier<DuelModifier>(out var mod))
        {
            player.RemoveModifier(mod);
        }
    }
}
