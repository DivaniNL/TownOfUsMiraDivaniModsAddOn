using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Utilities.Assets;
using DivaniMods.Assets;
using DivaniMods.Options;
using DivaniMods.Patches;
using TownOfUs.Buttons;
using UnityEngine;

namespace DivaniMods.Buttons;

public class UsePortalButton : CustomActionButton
{
    public override string Name => "Use Portal";
    public override float Cooldown => OptionGroupSingleton<PortalmakerOptions>.Instance.UsePortalCooldown;
    public override float EffectDuration => 0f;
    public override int MaxUses => 0;
    public override LoadableAsset<Sprite>? Sprite => DivaniAssets.UsePortalButton;
    public override ButtonLocation Location { get; set; } = ButtonLocation.BottomLeft;
    public override Color TextOutlineColor => new Color(0.047f, 0.420f, 0.961f);
    public override BaseKeybind Keybind => Keybinds.TertiaryAction;

    public override bool Enabled(RoleBehaviour? role)
    {
        UsePortalButtonVisibilityPatch.ButtonInstance = this;
        return true;
    }

    public override bool CanUse()
    {
        var player = PlayerControl.LocalPlayer;
        if (player == null || player.Data == null || player.Data.IsDead) return false;

        // Disabled during meetings
        if (MeetingHud.Instance || ExileController.Instance)
            return false;

        // Disabled during comms sabotage
        if (PlayerTask.PlayerHasTaskOfType<IHudOverrideTask>(player))
            return false;
        
        if (!PortalManager.BothPortalsPlaced) return false;
        
        var position = player.GetTruePosition();
        if (!PortalManager.IsNearPortal(position)) return false;
        
        if (!PortalManager.CanUsePortal(player.PlayerId)) return false;
        
        return base.CanUse();
    }

    protected override void OnClick()
    {
        var player = PlayerControl.LocalPlayer;
        if (player == null) return;
        
        var position = player.GetTruePosition();
        var destination = PortalManager.GetDestination(position);
        
        if (!destination.HasValue)
        {
            DivaniPlugin.Instance.Log.LogInfo("No valid destination found");
            return;
        }
        
        DivaniPlugin.Instance.Log.LogInfo($"Using portal to teleport to {destination.Value}");
        PortalManager.RpcUsePortal(player, destination.Value.x, destination.Value.y);
    }
}
