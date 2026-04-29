using BepInEx.Logging;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using Reactor.Networking.Attributes;
using DivaniMods.Assets;
using DivaniMods.Modifiers;
using DivaniMods.Options;
using DivaniMods.Roles;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace DivaniMods.Buttons;

public class ShuffleButton : CustomActionButton
{
    private static ManualLogSource Log => DivaniPlugin.Instance.Log;
    
    public override string Name => "Shuffle";
    public override float Cooldown => OptionGroupSingleton<ShuffleOptions>.Instance.ShuffleCooldown.Value;
    public override float EffectDuration => 0f;
    public override int MaxUses => (int)OptionGroupSingleton<ShuffleOptions>.Instance.ShuffleUses.Value;
    public override LoadableAsset<Sprite>? Sprite => DivaniAssets.ShuffleButton;
    public override Color TextOutlineColor => new Color32(0, 255, 30, 255);

    public override bool Enabled(RoleBehaviour? role)
    {
        var player = PlayerControl.LocalPlayer;
        if (player == null || player.Data == null) return false;
        return player.HasModifier<ShuffleModifier>();
    }

    public override bool CanUse()
    {
        var player = PlayerControl.LocalPlayer;
        if (player == null || player.Data == null || player.Data.IsDead) return false;
        var modifier = player.GetModifier<ShuffleModifier>();
        if (modifier == null)
        {
            SetUses(0);
            return false;
        }
        
        SetUses(modifier.UsesRemaining);
        return modifier.UsesRemaining > 0;
    }

    protected override void OnClick()
    {
        var player = PlayerControl.LocalPlayer;
        if (player == null) return;
        
        var modifier = player.GetModifier<ShuffleModifier>();
        if (modifier == null || modifier.UsesRemaining <= 0) return;
        
        Log.LogInfo("Shuffle activated!");
        modifier.UsesRemaining--;
        
        var targets = PlayerControl.AllPlayerControls.ToArray()
            .Where(p => p != null && p.Data != null && !p.Data.IsDead && !p.Data.Disconnected)
            .ToList();
        
        if (targets.Count < 2)
        {
            Log.LogInfo("Not enough players to shuffle");
            return;
        }
        
        var originalPositions = new List<Vector2>();
        foreach (var t in targets)
        {
            var pos = t.GetTruePosition();
            originalPositions.Add(new Vector2(pos.x, pos.y + 0.3636f));
        }
        
        var includeDeadBodies = OptionGroupSingleton<ShuffleOptions>.Instance.ShuffleDeadBodies;
        var deadBodies = new List<DeadBody>();
        var deadBodyPositions = new List<Vector2>();
        
        if (includeDeadBodies)
        {
            deadBodies = UnityEngine.Object.FindObjectsOfType<DeadBody>().ToList();
            foreach (var body in deadBodies)
            {
                if (body != null)
                {
                    deadBodyPositions.Add((Vector2)body.transform.position);
                    originalPositions.Add((Vector2)body.transform.position);
                }
            }
            Log.LogInfo($"Including {deadBodies.Count} dead bodies in shuffle");
        }
        
        var shuffledPositions = new List<Vector2>(originalPositions);
        var rng = new System.Random();
        for (int i = shuffledPositions.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (shuffledPositions[i], shuffledPositions[j]) = (shuffledPositions[j], shuffledPositions[i]);
        }
        
        bool anyMoved = false;
        for (int i = 0; i < shuffledPositions.Count; i++)
        {
            if (Vector2.Distance(originalPositions[i], shuffledPositions[i]) > 0.5f)
            {
                anyMoved = true;
                break;
            }
        }
        if (!anyMoved && shuffledPositions.Count >= 2)
            (shuffledPositions[0], shuffledPositions[1]) = (shuffledPositions[1], shuffledPositions[0]);
        
        var parts = new List<string>();
        for (int i = 0; i < targets.Count; i++)
        {
            var id = targets[i].PlayerId;
            var pos = shuffledPositions[i];
            parts.Add($"P{id},{pos.x.ToString(CultureInfo.InvariantCulture)},{pos.y.ToString(CultureInfo.InvariantCulture)}");
        }
        
        if (includeDeadBodies)
        {
            for (int i = 0; i < deadBodies.Count; i++)
            {
                var body = deadBodies[i];
                var pos = shuffledPositions[targets.Count + i];
                parts.Add($"B{body.ParentId},{pos.x.ToString(CultureInfo.InvariantCulture)},{pos.y.ToString(CultureInfo.InvariantCulture)}");
            }
        }
        
        string data = string.Join(";", parts);
        
        Log.LogInfo($"Sending shuffle data: {data}");
        RpcShuffle(player, data);
    }

    [MethodRpc((uint)DivaniRpcCalls.DoShuffle)]
    public static void RpcShuffle(PlayerControl sender, string data)
    {
        DivaniPlugin.Instance.Log.LogInfo($"RpcShuffle received: {data}");
        
        var entries = data.Split(';');
        var playerCoordinates = new Dictionary<byte, Vector2>();
        var bodyCoordinates = new Dictionary<byte, Vector2>();
        
        foreach (var entry in entries)
        {
            var parts = entry.Split(',');
            if (parts.Length != 3) continue;
            
            var idPart = parts[0];
            if (idPart.StartsWith("P"))
            {
                if (byte.TryParse(idPart.Substring(1), out byte playerId) &&
                    float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float x) &&
                    float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float y))
                {
                    playerCoordinates[playerId] = new Vector2(x, y);
                }
            }
            else if (idPart.StartsWith("B"))
            {
                if (byte.TryParse(idPart.Substring(1), out byte bodyId) &&
                    float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float x) &&
                    float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float y))
                {
                    bodyCoordinates[bodyId] = new Vector2(x, y);
                }
            }
        }
        
        DivaniPlugin.Instance.Log.LogInfo($"Parsed {playerCoordinates.Count} player positions and {bodyCoordinates.Count} body positions");
        
        // Only close minigames/vents for living players who are being shuffled
        var localPlayer = PlayerControl.LocalPlayer;
        if (localPlayer != null && localPlayer.Data != null && !localPlayer.Data.IsDead && playerCoordinates.ContainsKey(localPlayer.PlayerId))
        {
            if (Minigame.Instance)
            {
                try { Minigame.Instance.Close(); }
                catch { }
            }
            
            if (localPlayer.inVent)
            {
                localPlayer.MyPhysics.ExitAllVents();
            }
        }
        
        foreach (var kvp in playerCoordinates)
        {
            var player = PlayerById(kvp.Key);
            if (player == null) continue;
            // Defensive: if a player died between the sender's snapshot and the RPC
            // delivery, don't teleport their ghost.
            if (player.Data == null || player.Data.IsDead || player.Data.Disconnected) continue;
            
            var position = kvp.Value;
            
            player.MyPhysics.ResetMoveState();
            player.transform.position = new Vector3(position.x, position.y, player.transform.position.z);
            
            if (player.NetTransform != null)
            {
                player.NetTransform.SnapTo(position, (ushort)(player.NetTransform.lastSequenceId + 1));
            }
            
            if (player.MyPhysics?.body != null)
            {
                player.MyPhysics.body.velocity = Vector2.zero;
            }
        }
        
        foreach (var kvp in bodyCoordinates)
        {
            var body = UnityEngine.Object.FindObjectsOfType<DeadBody>().FirstOrDefault(b => b.ParentId == kvp.Key);
            if (body != null)
            {
                body.transform.position = new Vector3(kvp.Value.x, kvp.Value.y, body.transform.position.z);
                DivaniPlugin.Instance.Log.LogInfo($"Moved body {kvp.Key} to {kvp.Value}");
            }
        }
        
        if (playerCoordinates.TryGetValue(PlayerControl.LocalPlayer.PlayerId, out var localPos))
        {
            PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(localPos);
        }
        
        var local = PlayerControl.LocalPlayer;
        
        if (local.walkingToVent)
        {
            local.inVent = false;
            Vent.currentVent = null;
            local.moveable = true;
            local.MyPhysics.StopAllCoroutines();
        }
        
        if (local.onLadder)
        {
            local.onLadder = false;
            local.moveable = true;
            local.MyPhysics.StopAllCoroutines();
            local.SetPetPosition(local.MyPhysics.transform.position);
            local.MyPhysics.ResetAnimState();
            local.Collider.enabled = true;
        }
        
        MiraAPI.Utilities.Helpers.CreateAndShowNotification(
            $"<b><color=#808080>Everyone has been shuffled!</color></b>", 
            Color.white,
            new Vector3(0f, 1f, -20f), 
            spr: DivaniAssets.ShuffleButton.LoadAsset());
        
        DivaniPlugin.Instance.Log.LogInfo($"Shuffled {playerCoordinates.Count} players and {bodyCoordinates.Count} bodies!");
    }
    
    private static PlayerControl? PlayerById(byte id)
    {
        foreach (var p in PlayerControl.AllPlayerControls)
            if (p != null && p.PlayerId == id)
                return p;
        return null;
    }
}

public enum DivaniRpcCalls : uint
{
    DoShuffle = 200,
    StealModifier = 201,
    GiveRandomModifier = 202,
    StartLockdown = 203,
    EndLockdown = 204,
    PlacePortal = 205,
    UsePortal = 206,
    ResetPortals = 207,
    PlagueDoctorSetInfected = 208,
    PlagueDoctorUpdateProgress = 209,
    PlaceBeacon = 211,
    FragPassBomb = 212,
    InnocentTaunt = 213,
    OpportunistWin = 214
}
