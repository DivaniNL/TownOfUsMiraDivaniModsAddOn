using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Utilities.Assets;
using DivaniMods.Assets;
using DivaniMods.Options;
using DivaniMods.Roles;
using TownOfUs.Buttons;
using UnityEngine;

namespace DivaniMods.Buttons;

public class PlaceBeaconButton : CustomActionButton
{
    public override string Name => "Place Beacon";
    public override float Cooldown => OptionGroupSingleton<SentinelOptions>.Instance.PlaceBeaconCooldown;
    public override float EffectDuration => 0f;
    public override int MaxUses => 0;
    public override LoadableAsset<Sprite>? Sprite => DivaniAssets.SentinelIcon;
    public override ButtonLocation Location { get; set; } = ButtonLocation.BottomRight;
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;

    private static readonly Color SentinelColor = SentinelRole.SentinelColor;
    public override Color TextOutlineColor => SentinelColor;

    /// <summary>Button instance for visibility patch.</summary>
    public static PlaceBeaconButton? Instance { get; private set; }

    public override bool Enabled(RoleBehaviour? role)
    {
        Instance = this;
        return role is SentinelRole;
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

        int maxBeacons = (int)OptionGroupSingleton<SentinelOptions>.Instance.MaxBeacons;
        if (BeaconManager.BeaconsPlaced >= maxBeacons) return false;

        // Only usable when standing in a valid room
        var position = player.GetTruePosition();
        if (!BeaconManager.IsInRoom(position)) return false;

        SetUses(maxBeacons - BeaconManager.BeaconsPlaced);

        return base.CanUse();
    }

    protected override void OnClick()
    {
        var player = PlayerControl.LocalPlayer;
        if (player == null) return;

        int maxBeacons = (int)OptionGroupSingleton<SentinelOptions>.Instance.MaxBeacons;
        if (BeaconManager.BeaconsPlaced >= maxBeacons) return;

        var position = player.GetTruePosition();
        if (!BeaconManager.IsInRoom(position)) return;

        var roomName = BeaconManager.GetRoomName(position) ?? "Unknown";

        // Place immediately - no coroutine, no shake
        BeaconManager.RpcPlaceBeacon(player, position.x, position.y);

        int beaconNum = BeaconManager.BeaconsPlaced;
        char label = (char)('A' + beaconNum - 1);
        var colorHex = ColorUtility.ToHtmlStringRGB(SentinelColor);
        var message = $"<b><color=#{colorHex}>Beacon {label} placed in {roomName}!</color></b>";

        MiraAPI.Utilities.Helpers.CreateAndShowNotification(
            message,
            Color.white,
            new Vector3(0f, 1f, -20f),
            spr: DivaniAssets.SentinelIcon.LoadAsset());

        DivaniPlugin.Instance.Log.LogInfo($"Sentinel: Placed beacon {label} in {roomName}");
    }
}
