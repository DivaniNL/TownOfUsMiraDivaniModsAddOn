using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Reactor.Networking.Attributes;
using MiraAPI.Modifiers;
using DivaniMods.Assets;
using DivaniMods.Roles.Crewmate.CrewmateSupport;
using MiraAPI.Hud;
using TownOfUs.Modifiers.Game.Universal;
using TownOfUs.Utilities;
using DivaniMods.Modifiers.Neutral.NeutralOutlier;

namespace DivaniMods.Buttons.Crewmate.CrewmateSupport;

public static class PortalManager
{
    private const float PortalAnchorYOffset = 0.3636f;

    public static Vector2? Portal1Position { get; private set; }
    public static Vector2? Portal2Position { get; private set; }
    
    public static GameObject? Portal1Object { get; private set; }
    public static GameObject? Portal2Object { get; private set; }

    public static string Portal1RoomName { get; private set; } = "Outside/Hallway";
    public static string Portal2RoomName { get; private set; } = "Outside/Hallway";
    
    public static bool BothPortalsPlaced => Portal1Position.HasValue && Portal2Position.HasValue;
    public static int PortalsPlaced => (Portal1Position.HasValue ? 1 : 0) + (Portal2Position.HasValue ? 1 : 0);

    public static bool PortalsUnlocked { get; set; }
    
    private static readonly Dictionary<byte, float> PlayerCooldowns = new();
    private static readonly HashSet<string> PortalUsers = new();
    private static readonly HashSet<string> ImmovablePortalUsers = new();
    
    public static void Reset()
    {
        Portal1Position = null;
        Portal2Position = null;
        Portal1RoomName = "Portal 1";
        Portal2RoomName = "Portal 2";
        PortalsUnlocked = false;

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
        ImmovablePortalUsers.Clear();
        Portal1Materials.Clear();
        Portal2Materials.Clear();
    }
    
    public static void ClearPortalUsers()
    {
        PortalUsers.Clear();
        ImmovablePortalUsers.Clear();
    }
    
    public static void AddPortalUser(PlayerControl player)
    {
        if (player?.Data == null) return;
        if (player.HasModifier<DuelModifier>()) return;
        var playerName = player.Data.PlayerName;
        PortalUsers.Add(playerName);
    }

    private static void AddImmovablePortalUser(PlayerControl player)
    {
        if (player?.Data == null) return;
        if (player.HasModifier<DuelModifier>()) return;
        ImmovablePortalUsers.Add(player.Data.PlayerName);
    }
    
    public static void ReportPortalUsage(PlayerControl portalmaker)
    {
        if (!portalmaker.AmOwner) return;
        
        string msg;
        if (PortalUsers.Count == 0 && ImmovablePortalUsers.Count == 0)
        {
            msg = "No one used the portals.";
        }
        else
        {
            var message = new StringBuilder();

            if (PortalUsers.Count > 0)
            {
                message.Append("Players who used the portals:\n");
                message.Append(string.Join(", ", PortalUsers));
            }

            if (ImmovablePortalUsers.Count > 0)
            {
                if (message.Length > 0)
                {
                    message.Append("\n\n");
                }
                message.Append("Immovable player(s) who tried to use the portals:\n");
                message.Append(string.Join(", ", ImmovablePortalUsers));
                message.Append("\nFeels bad man..."); 
            }

            msg = message.ToString();
        }
        
        var title = "<color=#0C6BF5FF>Portal Activity</color>";
        
        MiscUtils.AddFakeChat(portalmaker.Data, title, msg, false, true);
        
        PortalUsers.Clear();
        ImmovablePortalUsers.Clear();
    }
    
    public static void PlacePortal(Vector2 position)
    {
        if (!Portal1Position.HasValue)
        {
            Portal1Position = position;
            Portal1RoomName = RoomHelpers.GetRoomName(position) ?? "Outside/Hallway";
            CreatePortalVisual(position, 1);
        }
        else if (!Portal2Position.HasValue)
        {
            Portal2Position = position;
            Portal2RoomName = RoomHelpers.GetRoomName(position) ?? "Outside/Hallway";
            CreatePortalVisual(position, 2);
        }
    }

    public static string GetPortalRoomName(int index) => index == 1 ? Portal1RoomName : Portal2RoomName;

    public static Vector2? GetPortalDestination(int index)
    {
        var pos = index == 1 ? Portal1Position : index == 2 ? Portal2Position : null;
        if (!pos.HasValue) return null;
        return new Vector2(pos.Value.x, pos.Value.y + PortalAnchorYOffset);
    }

    public static void SyncPortalButtonCooldowns()
    {
        var cd = MiraAPI.GameOptions.OptionGroupSingleton<Options.PortalmakerOptions>.Instance.UsePortalCooldown.Value;
        TrySetButtonTimer<UsePortalButton>(cd);
        TrySetButtonTimer<PortalTeleportButton1>(cd);
        TrySetButtonTimer<PortalTeleportButton2>(cd);
    }

    private static void TrySetButtonTimer<T>(float cd) where T : CustomActionButton
    {
        try
        {
            var button = CustomButtonSingleton<T>.Instance;
            if (button != null)
            {
                button.Timer = cd;
            }
        }
        catch
        {
        }
    }
    
    private static readonly Color PortalOutlineColor = new Color(0.047f, 0.420f, 0.961f);
    private const float PortalUseRange = 0.8f;

    private static readonly List<Material> Portal1Materials = new();
    private static readonly List<Material> Portal2Materials = new();

    private static void CreatePortalVisual(Vector2 position, int portalNumber)
    {
        var portal = UnityEngine.Object.Instantiate(DivaniAssets.PortalPrefab.LoadAsset());
        portal.name = $"Portal{portalNumber}";
        portal.transform.position = new Vector3(position.x, position.y, position.y / 1000f + 1f);

        ApplyOutline(portal, portalNumber, PortalOutlineColor);
        SetupPortalClick(portal);

        if (portalNumber == 1)
            Portal1Object = portal;
        else
            Portal2Object = portal;
    }

    private static void SetupPortalClick(GameObject portal)
    {
        var collider = portal.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;

        var sr = portal.GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            var bounds = sr.bounds;
            collider.offset = (Vector2)(bounds.center - portal.transform.position);
            collider.size = bounds.size;
        }
        else
        {
            collider.size = new Vector2(0.7f, 1f);
        }

        var passive = portal.AddComponent<PassiveButton>();
        passive.OnClick = new Button.ButtonClickedEvent();
        passive.OnClick.AddListener((Action)OnPortalClicked);
        passive.OnMouseOver = new UnityEvent();
        passive.OnMouseOut = new UnityEvent();
        passive.Colliders = (Il2CppReferenceArray<Collider2D>)new Collider2D[] { collider };
    }

    private static void OnPortalClicked()
    {
        CustomButtonSingleton<UsePortalButton>.Instance?.TriggerFromUseButton();
    }

    private static void ApplyOutline(GameObject portal, int portalNumber, Color color)
    {
        var mats = portalNumber == 1 ? Portal1Materials : Portal2Materials;
        mats.Clear();

        var shader = GetOutlineShader();
        if (shader == null)
        {
            return;
        }

        foreach (var sr in portal.GetComponentsInChildren<SpriteRenderer>(true))
        {
            if (sr == null)
            {
                continue;
            }

            var mat = sr.material;
            if (mat == null)
            {
                continue;
            }

            mat.shader = shader;

            if (mat.HasProperty("_Outline"))
            {
                mat.SetFloat("_Outline", 0f);
            }
            if (mat.HasProperty("_OutlineColor"))
            {
                mat.SetColor("_OutlineColor", color);
            }
            if (mat.HasProperty("_AddColor"))
            {
                mat.SetColor("_AddColor", Color.clear);
            }

            mats.Add(mat);
        }
    }

    public static void UpdatePortalOutlines()
    {
        if (Portal1Materials.Count == 0 && Portal2Materials.Count == 0)
        {
            return;
        }

        var player = PlayerControl.LocalPlayer;
        var usable = player != null && player.Data != null && !player.Data.IsDead
            && BothPortalsPlaced
            && !(MiraAPI.GameOptions.OptionGroupSingleton<Options.PortalmakerOptions>.Instance.EnableAfterFirstMeeting && !PortalsUnlocked);

        if (!usable)
        {
            SetOutline(Portal1Materials, false);
            SetOutline(Portal2Materials, false);
            return;
        }

        var pos = player!.GetTruePosition();

        var near1 = Portal1Position.HasValue
            && Vector2.Distance(pos, GetPortalUseAnchor(Portal1Position.Value, Portal1Object)) <= PortalUseRange;
        var near2 = Portal2Position.HasValue
            && Vector2.Distance(pos, GetPortalUseAnchor(Portal2Position.Value, Portal2Object)) <= PortalUseRange;

        SetOutline(Portal1Materials, near1);
        SetOutline(Portal2Materials, near2);
    }

    private static void SetOutline(List<Material> mats, bool on)
    {
        foreach (var mat in mats)
        {
            if (mat == null)
            {
                continue;
            }

            if (mat.HasProperty("_Outline"))
            {
                mat.SetFloat("_Outline", on ? 1f : 0f);
            }
            if (mat.HasProperty("_AddColor"))
            {
                mat.SetColor("_AddColor", on ? PortalOutlineColor : Color.clear);
            }
        }
    }

    private static Shader? GetOutlineShader()
    {
        var ship = ShipStatus.Instance;
        if (ship == null || ship.AllVents == null)
        {
            return null;
        }

        foreach (var vent in ship.AllVents)
        {
            if (vent != null && vent.myRend != null && vent.myRend.sharedMaterial != null)
            {
                return vent.myRend.sharedMaterial.shader;
            }
        }

        return null;
    }

    private static Vector2 GetPortalUseAnchor(Vector2 fallbackPosition, GameObject? portalObject)
    {
        if (portalObject != null)
        {
            var sr = portalObject.GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
            {
                return sr.bounds.center;
            }
        }

        return new Vector2(fallbackPosition.x, fallbackPosition.y + PortalAnchorYOffset);
    }
    
    public static Vector2? GetDestination(Vector2 playerPosition)
    {
        if (!BothPortalsPlaced) return null;

        var portal1UseAnchor = GetPortalUseAnchor(Portal1Position!.Value, Portal1Object);
        var portal2UseAnchor = GetPortalUseAnchor(Portal2Position!.Value, Portal2Object);
        
        float dist1 = Vector2.Distance(playerPosition, portal1UseAnchor);
        float dist2 = Vector2.Distance(playerPosition, portal2UseAnchor);

        var portal1Destination = new Vector2(Portal1Position.Value.x, Portal1Position.Value.y + PortalAnchorYOffset);
        var portal2Destination = new Vector2(Portal2Position.Value.x, Portal2Position.Value.y + PortalAnchorYOffset);
        
        const float useRange = PortalUseRange;
        
        if (dist1 <= useRange)
            return portal2Destination;
        if (dist2 <= useRange)
            return portal1Destination;
        
        return null;
    }
    
    public static bool IsNearPortal(Vector2 playerPosition, float range = PortalUseRange)
    {
        if (!BothPortalsPlaced) return false;

        var portal1Anchor = GetPortalUseAnchor(Portal1Position!.Value, Portal1Object);
        var portal2Anchor = GetPortalUseAnchor(Portal2Position!.Value, Portal2Object);
        
        float dist1 = Vector2.Distance(playerPosition, portal1Anchor);
        float dist2 = Vector2.Distance(playerPosition, portal2Anchor);
        
        return dist1 <= range || dist2 <= range;
    }
    
    public static bool CanUsePortal(byte playerId)
    {
        if (!PlayerCooldowns.TryGetValue(playerId, out float lastUse))
            return true;
        
        return Time.time - lastUse >= MiraAPI.GameOptions.OptionGroupSingleton<Options.PortalmakerOptions>.Instance.UsePortalCooldown.Value;
    }
    
    public static void SetPlayerCooldown(byte playerId)
    {
        PlayerCooldowns[playerId] = Time.time;
    }
    
    public static float GetRemainingCooldown(byte playerId)
    {
        if (!PlayerCooldowns.TryGetValue(playerId, out float lastUse))
            return 0f;
        
        float cooldown = MiraAPI.GameOptions.OptionGroupSingleton<Options.PortalmakerOptions>.Instance.UsePortalCooldown.Value;
        float remaining = cooldown - (Time.time - lastUse);
        return remaining > 0 ? remaining : 0f;
    }

    [MethodRpc((uint)DivaniRpcCalls.PlacePortal)]
    public static void RpcPlacePortal(PlayerControl sender, float x, float y)
    {
        PlacePortal(new Vector2(x, y));
    }
    
    [MethodRpc((uint)DivaniRpcCalls.UsePortal)]
    public static void RpcUsePortal(PlayerControl user, float destX, float destY)
    {

        if (user.HasModifier<ImmovableModifier>())
        {
            AddImmovablePortalUser(user);
            return;
        }
        
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
        
        if (user.AmOwner && user.NetTransform != null)
        {
            user.NetTransform.RpcSnapTo(destination);
        }
    }
    
    [MethodRpc((uint)DivaniRpcCalls.ResetPortals)]
    public static void RpcResetPortals(PlayerControl sender)
    {
        Reset();
    }
}
