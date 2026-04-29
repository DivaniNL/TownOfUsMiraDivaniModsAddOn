using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using DivaniMods.Assets;
using DivaniMods.Options;
using DivaniMods.Roles;
using System.Collections;
using TownOfUs.Buttons;
using UnityEngine;

namespace DivaniMods.Buttons;

public class PlacePortalButton : CustomActionButton
{
    public override string Name => "Place Portal";
    public override float Cooldown => OptionGroupSingleton<PortalmakerOptions>.Instance.PlacePortalCooldown;
    public override float EffectDuration => 3f;
    public override int MaxUses => 2;
    public override LoadableAsset<Sprite>? Sprite => DivaniAssets.PlacePortalButton;
    public override ButtonLocation Location { get; set; } = ButtonLocation.BottomRight;
    public override Color TextOutlineColor => new Color(0.047f, 0.420f, 0.961f);
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    
    private bool _isPlacing;

    public override bool Enabled(RoleBehaviour? role)
    {
        return role is PortalmakerRole;
    }

    public override bool CanUse()
    {
        var player = PlayerControl.LocalPlayer;
        if (player == null || player.Data == null || player.Data.IsDead) return false;

        // Disabled during comms sabotage
        if (PlayerTask.PlayerHasTaskOfType<IHudOverrideTask>(player))
            return false;
        
        if (_isPlacing) return false;
        
        int placedCount = PortalManager.PortalsPlaced;
        SetUses(2 - placedCount);
        
        return placedCount < 2 && base.CanUse();
    }

    protected override void OnClick()
    {
        var player = PlayerControl.LocalPlayer;
        if (player == null) return;
        
        if (PortalManager.PortalsPlaced >= 2)
        {
            DivaniPlugin.Instance.Log.LogInfo("Both portals already placed");
            return;
        }
        
        if (_isPlacing) return;
        
        Coroutines.Start(PlacePortalCoroutine(player));
    }
    
    private IEnumerator PlacePortalCoroutine(PlayerControl player)
    {
        _isPlacing = true;
        
        DivaniPlugin.Instance.Log.LogInfo("Starting portal placement...");
        
        MiraAPI.Utilities.Helpers.CreateAndShowNotification(
            "<b><color=#6633CC>Placing portal...</color></b>",
            Color.white,
            new Vector3(0f, 1f, -20f),
            spr: DivaniAssets.PortalmakerIcon.LoadAsset());
        
        yield return new WaitForSeconds(3f);
        
        if (player == null || player.Data == null || player.Data.IsDead)
        {
            _isPlacing = false;
            yield break;
        }
        
        var position = player.GetTruePosition();
        DivaniPlugin.Instance.Log.LogInfo($"Placing portal at {position}");
        PortalManager.RpcPlacePortal(player, position.x, position.y);

        // Local-only SFX: RpcPlacePortal is the network broadcast, PlaySound
        // runs only on this client so only the Portalmaker hears the drop.
        PlayPlacePortalSound();
        
        int portalNum = PortalManager.PortalsPlaced;
        string message = portalNum == 1 
            ? "<b><color=#6633CC>Portal 1 placed! Place another portal to complete the link.</color></b>"
            : "<b><color=#6633CC>Portal 2 placed! Portals are now active!</color></b>";
        
        MiraAPI.Utilities.Helpers.CreateAndShowNotification(
            message,
            Color.white,
            new Vector3(0f, 1f, -20f),
            spr: DivaniAssets.PortalmakerIcon.LoadAsset());
        
        if (Button != null)
        {
            Coroutines.Start(ShakeButton(Button));
        }
        
        _isPlacing = false;
    }
    
    private static void PlayPlacePortalSound()
    {
        if (SoundManager.Instance == null) return;
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

    private static IEnumerator ShakeButton(ActionButton button)
    {
        var originalPosition = button.transform.localPosition;
        float elapsed = 0f;
        float shakeDuration = 0.3f;
        float shakeIntensity = 0.1f;
        
        while (elapsed < shakeDuration)
        {
            float x = UnityEngine.Random.Range(-shakeIntensity, shakeIntensity);
            float y = UnityEngine.Random.Range(-shakeIntensity, shakeIntensity);
            button.transform.localPosition = originalPosition + new Vector3(x, y, 0);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        button.transform.localPosition = originalPosition;
    }
}
