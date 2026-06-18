using System.Collections.Generic;
using System.Linq;
using System.Text;
using Reactor.Networking.Attributes;
using DivaniMods.Assets;
using DivaniMods.Roles.Crewmate.CrewmateInvestigative;
using TownOfUs.Utilities;
using UnityEngine;
using DivaniMods.Modifiers.Neutral.NeutralOutlier;
using DivaniMods.Modules.Duelist;
using MiraAPI.Modifiers;
using TownOfUs.Modifiers.Impostor;

namespace DivaniMods.Buttons.Crewmate.CrewmateInvestigative;

public static class BeaconManager
{
    public class BeaconData
    {
        public Vector2 Position { get; set; }
        public string RoomName { get; set; } = string.Empty;
        public GameObject? Visual { get; set; }
        public HashSet<string> PlayersPassedThrough { get; } = new();
        public HashSet<byte> PlayersInRoom { get; } = new();
    }

    public static List<BeaconData> Beacons { get; } = new();

    public static int BeaconsPlaced => Beacons.Count;

    public static bool IsInRoom(Vector2 position)
    {
        if (!ShipStatus.Instance) return false;

        foreach (var room in ShipStatus.Instance.FastRooms.Values)
        {
            if (room.roomArea != null && room.roomArea.OverlapPoint(position))
                return true;
        }

        return false;
    }

    public static string? GetRoomName(Vector2 position) => RoomHelpers.GetRoomName(position);

    public static PlainShipRoom? GetShipRoom(Vector2 position)
    {
        if (!ShipStatus.Instance) return null;

        foreach (var room in ShipStatus.Instance.FastRooms.Values)
        {
            if (room.roomArea != null && room.roomArea.OverlapPoint(position))
                return room;
        }

        return null;
    }

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

        SeedPlayersInRoom(beacon);
    }

    private static void SeedPlayersInRoom(BeaconData beacon)
    {
        beacon.PlayersInRoom.Clear();

        var beaconRoom = GetShipRoom(beacon.Position);
        if (beaconRoom == null) return;

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

    public static void ReseedAllBeacons()
    {
        foreach (var beacon in Beacons)
        {
            SeedPlayersInRoom(beacon);
        }
    }

    private static void CreateBeaconVisual(BeaconData beacon, int beaconNumber)
    {
        var go = new GameObject($"SentinelBeacon{beaconNumber}");
        go.transform.position = new Vector3(beacon.Position.x, beacon.Position.y, beacon.Position.y / 1000f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = DivaniAssets.BeaconSprite.LoadAsset();

        go.transform.localScale = new Vector3(0.35f, 0.35f, 1f);

        beacon.Visual = go;
    }

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
                if (player.Data.Role is SentinelRole) continue;

                var playerRoom = GetShipRoom(player.GetTruePosition());
                if (playerRoom == null || playerRoom.RoomId != beaconRoom.RoomId) continue;

                // Track presence even for duellists, so when their modifier is later removed
                // (duel ends / teleport back) they aren't treated as a fresh entry and flashed.
                currentPlayersInRoom.Add(player.PlayerId);

                if (player.HasModifier<DuelModifier>() || DuelManager.IsInDuel(player.PlayerId)) continue; // in a duel: no flash, no report

                if (!beacon.PlayersInRoom.Contains(player.PlayerId))
                {
                    var playerName = player.Data.PlayerName;
                    beacon.PlayersPassedThrough.Add(playerName);
                    newEntries.Add((beacon, playerName));
                }
            }

            beacon.PlayersInRoom.Clear();
            foreach (var id in currentPlayersInRoom)
            {
                beacon.PlayersInRoom.Add(id);
            }
        }

        return newEntries;
    }

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

        foreach (var beacon in Beacons)
        {
            beacon.PlayersPassedThrough.Clear();
            SeedPlayersInRoom(beacon);
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
    }
}
