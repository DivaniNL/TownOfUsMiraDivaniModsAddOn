using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using DivaniMods.Assets;
using DivaniMods.Options;
using DivaniMods.Roles.Crewmate.CrewmateSupport;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TownOfUs.Buttons;
using TownOfUs.Modules;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DivaniMods.Buttons.Crewmate.CrewmateSupport;

public class PlacePortalButton : TownOfUsButton
{
    public override string Name => "Place Portal";
    public override float Cooldown => OptionGroupSingleton<PortalmakerOptions>.Instance.PlacePortalCooldown.Value;
    public override float EffectDuration => OptionGroupSingleton<PortalmakerOptions>.Instance.PlacePortalDuration.Value;
    public override int MaxUses => 2;
    public override LoadableAsset<Sprite> Sprite => DivaniAssets.PlacePortalButton;
    public override ButtonLocation Location { get; set; } = ButtonLocation.BottomRight;
    public override Color TextOutlineColor => new Color(0.047f, 0.420f, 0.961f);
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    
    private const float ConsoleBlockRange = 1.2f;

    private static readonly List<Vector2> BlockedPositions = new();
    private static ShipStatus? _cachedShip;

    private bool _isPlacing;

    private Vector2 _placementSize = new Vector2(0.6f, 0.6f);

    public override bool Enabled(RoleBehaviour? role)
    {
        return role is PortalmakerRole;
    }

    public override void CreateButton(Transform parent)
    {
        base.CreateButton(parent);

        var vents = Object.FindObjectsOfType<Vent>();
        if (vents.Count > 0)
        {
            _placementSize = Vector2.Scale(vents[0].GetComponent<BoxCollider2D>().size, vents[0].transform.localScale) *
                             0.75f;
        }
    }

    public override bool CanUse()
    {
        var player = PlayerControl.LocalPlayer;
        if (player == null || player.Data == null || player.Data.IsDead) return false;

        if (PlayerTask.PlayerHasTaskOfType<IHudOverrideTask>(player))
            return false;

        if (_isPlacing) return false;

        int placedCount = PortalManager.PortalsPlaced;
        SetUses(2 - placedCount);

        return placedCount < 2 && base.CanUse() && IsValidPlacement(player);
    }

    private bool IsValidPlacement(PlayerControl player)
    {
        var hits = Physics2D.OverlapBoxAll(player.transform.position, _placementSize, 0f);

        hits = hits.Where(c =>
            (c.name.Contains("Vent") || c.name.Contains("Door") || !c.isTrigger) && c.gameObject.layer != 8 &&
            c.gameObject.layer != 5).ToArray();

        var noConflict = !PhysicsHelpers.AnythingBetween(player.Collider, player.Collider.bounds.center,
            player.transform.position, Constants.ShipAndAllObjectsMask, false);

        return hits.Count == 0 && noConflict && !ModCompatibility.GetPlayerElevator(player).Item1 &&
               !IsNearBlockedObject(player.GetTruePosition());
    }

    private static bool IsNearBlockedObject(Vector2 position)
    {
        EnsureBlockCache();

        foreach (var blocked in BlockedPositions)
        {
            if (Vector2.Distance(position, blocked) <= ConsoleBlockRange)
            {
                return true;
            }
        }

        var otherPortal = PortalManager.Portal1Position ?? PortalManager.Portal2Position;
        return otherPortal.HasValue && Vector2.Distance(position, otherPortal.Value) <= ConsoleBlockRange;
    }

    private static void EnsureBlockCache()
    {
        var ship = ShipStatus.Instance;
        if (ship == null)
        {
            _cachedShip = null;
            BlockedPositions.Clear();
            return;
        }

        if (_cachedShip == ship)
        {
            return;
        }

        _cachedShip = ship;
        BlockedPositions.Clear();

        foreach (var console in Object.FindObjectsOfType<Console>())
        {
            if (console != null) BlockedPositions.Add(console.transform.position);
        }

        foreach (var console in Object.FindObjectsOfType<SystemConsole>())
        {
            if (console != null) BlockedPositions.Add(console.transform.position);
        }

        foreach (var console in Object.FindObjectsOfType<MapConsole>())
        {
            if (console != null) BlockedPositions.Add(console.transform.position);
        }

        foreach (var console in Object.FindObjectsOfType<DoorConsole>())
        {
            if (console != null) BlockedPositions.Add(console.transform.position);
        }

        foreach (var vent in ship.AllVents)
        {
            if (vent != null) BlockedPositions.Add(vent.transform.position);
        }
    }

    protected override void OnClick()
    {
        var player = PlayerControl.LocalPlayer;
        if (player == null) return;
        
        if (PortalManager.PortalsPlaced >= 2)
        {
            return;
        }
        
        if (_isPlacing) return;
        
        var capturedPosition = player.GetTruePosition();
        Coroutines.Start(PlacePortalCoroutine(player, capturedPosition));
    }
    
    private IEnumerator PlacePortalCoroutine(PlayerControl player, Vector2 capturedPosition)
    {
        _isPlacing = true;
        
        
        MiraAPI.Utilities.Helpers.CreateAndShowNotification(
            "<b><color=#0C6BF5>Placing portal...</color></b>",
            Color.white,
            new Vector3(0f, 1f, -20f),
            spr: DivaniAssets.PortalmakerIcon.LoadAsset());
        
        yield return new WaitForSeconds(EffectDuration);

        if (player == null || player.Data == null || player.Data.IsDead)
        {
            _isPlacing = false;
            yield break;
        }

        PortalManager.RpcPlacePortal(player, capturedPosition.x, capturedPosition.y);

        PlayPlacePortalSound();
        
        int portalNum = PortalManager.PortalsPlaced;
        var afterMeeting = OptionGroupSingleton<PortalmakerOptions>.Instance.EnableAfterFirstMeeting;
        string message = portalNum == 1
            ? "<b><color=#0C6BF5>Portal 1 placed! Place another portal to complete the link.</color></b>"
            : afterMeeting
                ? "<b><color=#0C6BF5>Portal 2 placed! Portals will be enabled after the next meeting.</color></b>"
                : "<b><color=#0C6BF5>Portal 2 placed! Portals are now active!</color></b>";
        
        MiraAPI.Utilities.Helpers.CreateAndShowNotification(
            message,
            Color.white,
            new Vector3(0f, 1f, -20f),
            spr: DivaniAssets.PortalmakerIcon.LoadAsset());
        
        _isPlacing = false;
    }
    
    private static void PlayPlacePortalSound()
    {
        if (!SoundManager.Instance) return;
        try
        {
            var clip = DivaniAssets.PlacePortalSound.LoadAsset();
            if (clip == null) return;
            SoundManager.Instance.PlaySound(clip, false, 1f);
        }
        catch (System.Exception ex)
        {
            DivaniPlugin.Instance.Log.LogWarning($"Portalmaker: place sfx failed: {ex.Message}");
        }
    }

}
