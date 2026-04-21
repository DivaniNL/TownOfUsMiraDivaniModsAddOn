using System.Collections.Generic;
using System.Linq;
using System.Text;
using Reactor.Networking.Attributes;
using DivaniMods.Assets;
using DivaniMods.Roles;
using TownOfUs.Utilities;
using UnityEngine;

namespace DivaniMods.Buttons;

public static class BeaconManager
{
    /// <summary>
    /// Represents a placed beacon with its position, room name, and tracking data.
    /// </summary>
    public class BeaconData
    {
        public Vector2 Position { get; set; }
        public string RoomName { get; set; } = string.Empty;
        public GameObject? Visual { get; set; }
        public HashSet<string> PlayersPassedThrough { get; } = new();
        /// <summary>Players currently inside the beacon room (for enter/exit detection).</summary>
        public HashSet<byte> PlayersInRoom { get; } = new();
    }

    public static List<BeaconData> Beacons { get; } = new();

    public static int BeaconsPlaced => Beacons.Count;

    /// <summary>
    /// Check if a position is inside a valid room (not hallway/outside).
    /// </summary>
    public static bool IsInRoom(Vector2 position)
    {
        if (ShipStatus.Instance == null) return false;

        foreach (var room in ShipStatus.Instance.FastRooms.Values)
        {
            if (room.roomArea != null && room.roomArea.OverlapPoint(position))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Get the room name for a given position using the game's translation system.
    /// Returns null if the position is not in any room.
    /// </summary>
    public static string? GetRoomName(Vector2 position)
    {
        if (ShipStatus.Instance == null) return null;

        foreach (var room in ShipStatus.Instance.FastRooms.Values)
        {
            if (room.roomArea != null && room.roomArea.OverlapPoint(position))
                return TranslationController.Instance.GetString(room.RoomId);
        }

        return null;
    }

    /// <summary>
    /// Get the PlainShipRoom at a given position. Returns null if not in any room.
    /// </summary>
    public static PlainShipRoom? GetShipRoom(Vector2 position)
    {
        if (ShipStatus.Instance == null) return null;

        foreach (var room in ShipStatus.Instance.FastRooms.Values)
        {
            if (room.roomArea != null && room.roomArea.OverlapPoint(position))
                return room;
        }

        return null;
    }

    /// <summary>
    /// Check if a player is in the same room as any beacon.
    /// Returns the beacon if found, null otherwise.
    /// </summary>
    public static BeaconData? GetBeaconInSameRoom(Vector2 playerPosition)
    {
        var playerRoom = GetShipRoom(playerPosition);
        if (playerRoom == null) return null;

        foreach (var beacon in Beacons)
        {
            var beaconRoom = GetShipRoom(beacon.Position);
            if (beaconRoom != null && beaconRoom.RoomId == playerRoom.RoomId)
                return beacon;
        }

        return null;
    }

    [MethodRpc((uint)DivaniRpcCalls.PlaceBeacon)]
    public static void RpcPlaceBeacon(PlayerControl sender, float x, float y)
    {
        PlaceBeacon(new Vector2(x, y));
    }

    public static void PlaceBeacon(Vector2 position)
    {
        var roomName = GetRoomName(position) ?? "Unknown";

        var beacon = new BeaconData
        {
            Position = position,
            RoomName = roomName,
        };

        CreateBeaconVisual(beacon, Beacons.Count + 1);
        Beacons.Add(beacon);

        // Pre-populate PlayersInRoom so players already in the room don't trigger a flash
        var beaconRoom = GetShipRoom(position);
        if (beaconRoom != null)
        {
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player == null || player.Data == null || player.Data.IsDead) continue;
                if (player.Data.Disconnected) continue;
                if (player.Data.Role is SentinelRole) continue;

                var playerRoom = GetShipRoom(player.GetTruePosition());
                if (playerRoom != null && playerRoom.RoomId == beaconRoom.RoomId)
                {
                    beacon.PlayersInRoom.Add(player.PlayerId);
                }
            }
        }

        DivaniPlugin.Instance.Log.LogInfo($"Beacon {Beacons.Count} placed at {position} in {roomName}");
    }

    private static void CreateBeaconVisual(BeaconData beacon, int beaconNumber)
    {
        var go = new GameObject($"SentinelBeacon{beaconNumber}");
        // Z = y / 1000f so players walk in front/behind naturally (same as sentry, dead bodies, etc.)
        go.transform.position = new Vector3(beacon.Position.x, beacon.Position.y, beacon.Position.y / 1000f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = DivaniAssets.BeaconSprite.LoadAsset();

        go.transform.localScale = new Vector3(0.35f, 0.35f, 1f);

        beacon.Visual = go;
    }

    /// <summary>
    /// Track players entering/exiting beacon rooms each frame.
    /// Returns list of (beacon, playerName) for new entries (for flash triggers).
    /// Only call on the Sentinel's local client.
    /// </summary>
    public static List<(BeaconData Beacon, string PlayerName)> UpdatePlayerTracking()
    {
        var newEntries = new List<(BeaconData, string)>();

        foreach (var beacon in Beacons)
        {
            var beaconRoom = GetShipRoom(beacon.Position);
            if (beaconRoom == null) continue;

            var currentPlayersInRoom = new HashSet<byte>();

            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player == null || player.Data == null || player.Data.IsDead) continue;
                if (player.Data.Disconnected) continue;

                // Don't track the Sentinel themselves
                if (player.Data.Role is SentinelRole) continue;

                var playerRoom = GetShipRoom(player.GetTruePosition());
                if (playerRoom != null && playerRoom.RoomId == beaconRoom.RoomId)
                {
                    currentPlayersInRoom.Add(player.PlayerId);

                    // New entry - player wasn't in room before
                    if (!beacon.PlayersInRoom.Contains(player.PlayerId))
                    {
                        var playerName = player.Data.PlayerName;
                        beacon.PlayersPassedThrough.Add(playerName);
                        newEntries.Add((beacon, playerName));
                    }
                }
            }

            // Update tracked players: remove those who left, add those who entered
            beacon.PlayersInRoom.Clear();
            foreach (var id in currentPlayersInRoom)
            {
                beacon.PlayersInRoom.Add(id);
            }
        }

        return newEntries;
    }

    /// <summary>
    /// Generate meeting chat report for the Sentinel.
    /// </summary>
    public static void ReportBeaconActivity(PlayerControl sentinel)
    {
        if (!sentinel.AmOwner) return;

        if (Beacons.Count == 0) return;

        var colorHex = ColorUtility.ToHtmlStringRGBA(SentinelRole.SentinelColor);
        var title = $"<color=#{colorHex}>Beacon Report</color>";

        var sb = new StringBuilder();

        char label = 'A';
        foreach (var beacon in Beacons)
        {
            if (beacon.PlayersPassedThrough.Count == 0)
            {
                sb.AppendLine($"Beacon {label} ({beacon.RoomName}): No activity.");
            }
            else
            {
                var names = string.Join(", ", beacon.PlayersPassedThrough);
                sb.AppendLine($"Beacon {label} ({beacon.RoomName}): {names}");
            }

            label++;
        }

        MiscUtils.AddFakeChat(sentinel.Data, title, sb.ToString().TrimEnd(), false, true);

        // Clear tracked names for next round and pre-populate PlayersInRoom
        // so players already in the beacon room at round start don't trigger a flash
        foreach (var beacon in Beacons)
        {
            beacon.PlayersPassedThrough.Clear();
            beacon.PlayersInRoom.Clear();

            var beaconRoom = GetShipRoom(beacon.Position);
            if (beaconRoom == null) continue;

            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player == null || player.Data == null || player.Data.IsDead) continue;
                if (player.Data.Disconnected) continue;
                if (player.Data.Role is SentinelRole) continue;

                var playerRoom = GetShipRoom(player.GetTruePosition());
                if (playerRoom != null && playerRoom.RoomId == beaconRoom.RoomId)
                {
                    beacon.PlayersInRoom.Add(player.PlayerId);
                }
            }
        }
    }

    public static void Reset()
    {
        foreach (var beacon in Beacons)
        {
            if (beacon.Visual != null)
            {
                UnityEngine.Object.Destroy(beacon.Visual);
            }
        }

        Beacons.Clear();
        DivaniPlugin.Instance.Log.LogInfo("Beacon Manager reset");
    }
}
