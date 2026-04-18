using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Reactor.Networking.Attributes;
using DivaniMods.Assets;
using DivaniMods.Roles;

namespace DivaniMods.Buttons;

public static class PortalManager
{
    public static Vector2? Portal1Position { get; private set; }
    public static Vector2? Portal2Position { get; private set; }
    
    public static GameObject? Portal1Object { get; private set; }
    public static GameObject? Portal2Object { get; private set; }
    
    public static bool BothPortalsPlaced => Portal1Position.HasValue && Portal2Position.HasValue;
    public static int PortalsPlaced => (Portal1Position.HasValue ? 1 : 0) + (Portal2Position.HasValue ? 1 : 0);
    
    private static readonly Dictionary<byte, float> PlayerCooldowns = new();
    private static readonly HashSet<string> PortalUsers = new();
    
    public static void Reset()
    {
        Portal1Position = null;
        Portal2Position = null;
        
        if (Portal1Object != null)
        {
            UnityEngine.Object.Destroy(Portal1Object);
            Portal1Object = null;
        }
        if (Portal2Object != null)
        {
            UnityEngine.Object.Destroy(Portal2Object);
            Portal2Object = null;
        }
        
        PlayerCooldowns.Clear();
        PortalUsers.Clear();
        DivaniPlugin.Instance.Log.LogInfo("Portal Manager reset");
    }
    
    public static void ClearPortalUsers()
    {
        PortalUsers.Clear();
    }
    
    public static void AddPortalUser(PlayerControl player)
    {
        if (player?.Data == null) return;
        
        var playerName = player.Data.PlayerName;
        PortalUsers.Add(playerName);
    }
    
    public static void ReportPortalUsage(PlayerControl portalmaker)
    {
        if (!portalmaker.AmOwner) return;
        
        string msg;
        if (PortalUsers.Count == 0)
        {
            msg = "No one used the portals.";
        }
        else
        {
            var message = new StringBuilder("Players who used the portals:\n");
            foreach (var user in PortalUsers)
            {
                message.Append($"{user}, ");
            }
            message.Remove(message.Length - 2, 2);
            msg = message.ToString();
        }
        
        var title = "<color=#6633CC>Portal Activity</color>";
        var fullMessage = $"{title}\n{msg}";
        
        if (HudManager.Instance != null && HudManager.Instance.Chat != null)
        {
            HudManager.Instance.Chat.AddChat(portalmaker, fullMessage, false);
        }
        
        PortalUsers.Clear();
    }
    
    public static void PlacePortal(Vector2 position)
    {
        if (!Portal1Position.HasValue)
        {
            Portal1Position = position;
            CreatePortalVisual(position, 1);
            DivaniPlugin.Instance.Log.LogInfo($"Portal 1 placed at {position}");
        }
        else if (!Portal2Position.HasValue)
        {
            Portal2Position = position;
            CreatePortalVisual(position, 2);
            DivaniPlugin.Instance.Log.LogInfo($"Portal 2 placed at {position}");
        }
    }
    
    private static void CreatePortalVisual(Vector2 position, int portalNumber)
    {
        var portal = new GameObject($"Portal{portalNumber}");
        portal.transform.position = new Vector3(position.x, position.y, position.y / 1000f + 1f);
        
        var spriteRenderer = portal.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = DivaniAssets.PortalSprite.LoadAsset();
        
        portal.transform.localScale = new Vector3(1.62f, 1.62f, 1f);
        
        if (portalNumber == 1)
            Portal1Object = portal;
        else
            Portal2Object = portal;
    }
    
    public static Vector2? GetDestination(Vector2 playerPosition)
    {
        if (!BothPortalsPlaced) return null;
        
        float dist1 = Vector2.Distance(playerPosition, Portal1Position!.Value);
        float dist2 = Vector2.Distance(playerPosition, Portal2Position!.Value);
        
        const float useRange = 1.5f;
        
        if (dist1 <= useRange && HasLineOfSight(playerPosition, Portal1Position!.Value))
            return new Vector2(Portal2Position!.Value.x, Portal2Position!.Value.y + 0.3636f);
        if (dist2 <= useRange && HasLineOfSight(playerPosition, Portal2Position!.Value))
            return new Vector2(Portal1Position!.Value.x, Portal1Position!.Value.y + 0.3636f);
        
        return null;
    }
    
    public static bool IsNearPortal(Vector2 playerPosition, float range = 1.5f)
    {
        if (!BothPortalsPlaced) return false;
        
        float dist1 = Vector2.Distance(playerPosition, Portal1Position!.Value);
        float dist2 = Vector2.Distance(playerPosition, Portal2Position!.Value);
        
        bool nearPortal1 = dist1 <= range && HasLineOfSight(playerPosition, Portal1Position!.Value);
        bool nearPortal2 = dist2 <= range && HasLineOfSight(playerPosition, Portal2Position!.Value);
        
        return nearPortal1 || nearPortal2;
    }
    
    private static bool HasLineOfSight(Vector2 from, Vector2 to)
    {
        var hit = Physics2D.Linecast(from, to, Constants.ShipAndAllObjectsMask);
        return hit.collider == null;
    }
    
    public static bool CanUsePortal(byte playerId)
    {
        if (!PlayerCooldowns.TryGetValue(playerId, out float lastUse))
            return true;
        
        return Time.time - lastUse >= MiraAPI.GameOptions.OptionGroupSingleton<Options.PortalmakerOptions>.Instance.UsePortalCooldown;
    }
    
    public static void SetPlayerCooldown(byte playerId)
    {
        PlayerCooldowns[playerId] = Time.time;
    }
    
    public static float GetRemainingCooldown(byte playerId)
    {
        if (!PlayerCooldowns.TryGetValue(playerId, out float lastUse))
            return 0f;
        
        float cooldown = MiraAPI.GameOptions.OptionGroupSingleton<Options.PortalmakerOptions>.Instance.UsePortalCooldown;
        float remaining = cooldown - (Time.time - lastUse);
        return remaining > 0 ? remaining : 0f;
    }

    [MethodRpc((uint)DivaniRpcCalls.PlacePortal)]
    public static void RpcPlacePortal(PlayerControl sender, float x, float y)
    {
        DivaniPlugin.Instance.Log.LogInfo($"RpcPlacePortal received from {sender.name}: ({x}, {y})");
        PlacePortal(new Vector2(x, y));
    }
    
    [MethodRpc((uint)DivaniRpcCalls.UsePortal)]
    public static void RpcUsePortal(PlayerControl user, float destX, float destY)
    {
        DivaniPlugin.Instance.Log.LogInfo($"RpcUsePortal: {user.name} teleporting to ({destX}, {destY})");
        
        AddPortalUser(user);
        
        var destination = new Vector2(destX, destY);
        
        user.MyPhysics.ResetMoveState();
        user.transform.position = new Vector3(destination.x, destination.y, user.transform.position.z);
        
        if (user.NetTransform != null)
        {
            user.NetTransform.SnapTo(destination, (ushort)(user.NetTransform.lastSequenceId + 1));
        }
        
        if (user.MyPhysics?.body != null)
        {
            user.MyPhysics.body.velocity = Vector2.zero;
        }
        
        SetPlayerCooldown(user.PlayerId);
        
        if (user.AmOwner)
        {
            user.NetTransform.RpcSnapTo(destination);
        }
    }
    
    [MethodRpc((uint)DivaniRpcCalls.ResetPortals)]
    public static void RpcResetPortals(PlayerControl sender)
    {
        DivaniPlugin.Instance.Log.LogInfo("RpcResetPortals received");
        Reset();
    }
}
