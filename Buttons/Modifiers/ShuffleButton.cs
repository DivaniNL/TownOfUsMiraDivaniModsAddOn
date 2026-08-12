using BepInEx.Logging;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using Reactor.Networking.Attributes;
using DivaniMods.Assets;
using DivaniMods.Modifiers.Game.Universal;
using DivaniMods.Options;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TownOfUs.Buttons;
using TownOfUs.Interfaces;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modifiers.Game.Universal;
using TownOfUs.Modules;
using UnityEngine;

namespace DivaniMods.Buttons.Modifiers;

public class ShuffleButton : TownOfUsButton
{
    private static ManualLogSource Log => DivaniPlugin.Instance.Log;

    private const float MiniOffset = 0.2233912f * 0.75f;
    private const float TransformOffset = 0.3636f;

    private enum ShuffleKind
    {
        Player,
        Body,
        Stone
    }

    private sealed class ShuffleEntry
    {
        public ShuffleKind Kind;
        public byte Id;
        public Vector2 Anchor;
        public bool IsMini;
        public bool CanMove;
    }

    private static bool CanBeShuffled(PlayerControl player)
    {
        if (player.HasModifier<ImmovableModifier>()) return false;
        if (player.HasModifier<NoTransportModifier>()) return false;
        if (player.HasModifier<WardenFortifiedModifier>()) return false;
        if (player.HasModifier<ClericBarrierModifier>()) return false;

        return !player.GetModifiers<BaseModifier>().Any(x => x is IUntransportable);
    }

    public override string Name => "Shuffle";
    public override float Cooldown => OptionGroupSingleton<ShuffleOptions>.Instance.ShuffleCooldown.Value;
    public override float EffectDuration => 0f;
    public override int MaxUses => (int)OptionGroupSingleton<ShuffleOptions>.Instance.ShuffleUses.Value;
    public override LoadableAsset<Sprite> Sprite => DivaniAssets.ShuffleButton;
    public override Color TextOutlineColor => new Color32(0, 255, 30, 255);
    public override BaseKeybind Keybind => Keybinds.ModifierAction;
    public override ButtonLocation Location => ButtonLocation.BottomLeft;
    
    public override bool Enabled(RoleBehaviour? role)
    {
        var player = PlayerControl.LocalPlayer;
        if (player == null || player.Data == null || player.Data.IsDead) return false;
        return player.HasModifier<ShuffleModifier>();
    }

    public override bool CanUse()
    {
        if (!base.CanUse()) return false;

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
        if (MeetingHud.Instance || ExileController.Instance) return;

        var player = PlayerControl.LocalPlayer;
        if (player == null) return;

        var modifier = player.GetModifier<ShuffleModifier>();
        if (modifier == null || modifier.UsesRemaining <= 0) return;

        var entries = new List<ShuffleEntry>();

        foreach (var target in PlayerControl.AllPlayerControls.ToArray()
            .Where(p => p != null && p.Data != null && !p.Data.IsDead && !p.Data.Disconnected))
        {
            entries.Add(new ShuffleEntry
            {
                Kind = ShuffleKind.Player,
                Id = target.PlayerId,
                Anchor = target.GetTruePosition(),
                IsMini = target.HasModifier<MiniModifier>(),
                CanMove = CanBeShuffled(target)
            });
        }

        var includeDeadBodies = OptionGroupSingleton<ShuffleOptions>.Instance.ShuffleCorpses;

        if (includeDeadBodies)
        {
            foreach (var body in UnityEngine.Object.FindObjectsOfType<DeadBody>())
            {
                if (body == null) continue;

                entries.Add(new ShuffleEntry
                {
                    Kind = ShuffleKind.Body,
                    Id = body.ParentId,
                    Anchor = body.TruePosition,
                    IsMini = false,
                    CanMove = true
                });
            }

            foreach (var stone in StonedPlayer.FakePlayers.ToArray())
            {
                if (stone == null || !stone.body) continue;
                if (stone.ProgressStage is StoneStage.Permanent or StoneStage.Shatter) continue;

                entries.Add(new ShuffleEntry
                {
                    Kind = ShuffleKind.Stone,
                    Id = (byte)stone.PlayerId,
                    Anchor = (Vector2)stone.body.transform.position - new Vector2(0f, TransformOffset),
                    IsMini = stone.OriginalPlayer != null && stone.OriginalPlayer.HasModifier<MiniModifier>(),
                    CanMove = stone.ProgressStage is StoneStage.Frozen
                });
            }
        }

        if (entries.Count < 2)
        {
            return;
        }

        var slots = Enumerable.Range(0, entries.Count).ToList();
        var rng = new System.Random();
        for (int i = slots.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (slots[i], slots[j]) = (slots[j], slots[i]);
        }

        bool anyMoved = false;
        for (int i = 0; i < slots.Count; i++)
        {
            if (Vector2.Distance(entries[i].Anchor, entries[slots[i]].Anchor) > 0.5f)
            {
                anyMoved = true;
                break;
            }
        }
        if (!anyMoved)
            (slots[0], slots[1]) = (slots[1], slots[0]);

        var parts = new List<string>();
        for (int i = 0; i < entries.Count; i++)
        {
            var mover = entries[i];
            if (!mover.CanMove)
            {
                continue;
            }

            var slot = entries[slots[i]];
            var pos = slot.Anchor;
            if (slot.IsMini) pos.y += MiniOffset;
            if (mover.IsMini) pos.y -= MiniOffset;
            if (mover.Kind is not ShuffleKind.Body) pos.y += TransformOffset;

            var prefix = mover.Kind switch
            {
                ShuffleKind.Body => "B",
                ShuffleKind.Stone => "S",
                _ => "P"
            };
            parts.Add($"{prefix}{mover.Id},{pos.x.ToString(CultureInfo.InvariantCulture)},{pos.y.ToString(CultureInfo.InvariantCulture)}");
        }

        if (parts.Count == 0)
        {
            return;
        }

        modifier.UsesRemaining--;

        string data = string.Join(";", parts);

        RpcShuffle(player, data);
    }

    [MethodRpc((uint)DivaniRpcCalls.DoShuffle)]
    public static void RpcShuffle(PlayerControl sender, string data)
    {
        
        var entries = data.Split(';');
        var playerCoordinates = new Dictionary<byte, Vector2>();
        var bodyCoordinates = new Dictionary<byte, Vector2>();
        var stoneCoordinates = new Dictionary<byte, Vector2>();

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
            else if (idPart.StartsWith("S"))
            {
                if (byte.TryParse(idPart.Substring(1), out byte stoneId) &&
                    float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float x) &&
                    float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float y))
                {
                    stoneCoordinates[stoneId] = new Vector2(x, y);
                }
            }
        }
        
        
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
            if (player.Data == null || player.Data.IsDead || player.Data.Disconnected) continue;
            if (!CanBeShuffled(player)) continue;
            
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
            if (body == null) continue;

            var offset = body.myCollider != null ? body.myCollider.offset : Vector2.zero;
            var target = kvp.Value - offset;
            body.transform.position = new Vector3(target.x, target.y, target.y / 1000f);
        }

        foreach (var kvp in stoneCoordinates)
        {
            var stone = StonedPlayer.FakePlayers.FirstOrDefault(x => x != null && (byte)x.PlayerId == kvp.Key);
            if (stone == null || !stone.body) continue;
            if (stone.ProgressStage is not StoneStage.Frozen) continue;

            stone.body.transform.position = new Vector3(kvp.Value.x, kvp.Value.y, kvp.Value.y / 1000f);
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
            spr: DivaniAssets.ShuffleIcon.LoadAsset());
        
    }
    
    private static PlayerControl? PlayerById(byte id)
    {
        foreach (var p in PlayerControl.AllPlayerControls)
            if (p != null && p.PlayerId == id)
                return p;
        return null;
    }
}
