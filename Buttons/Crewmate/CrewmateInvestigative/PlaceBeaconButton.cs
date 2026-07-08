using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using DivaniMods.Assets;
using DivaniMods.Options;
using DivaniMods.Roles.Crewmate.CrewmateInvestigative;
using System.Collections;
using TownOfUs.Buttons;
using UnityEngine;

namespace DivaniMods.Buttons.Crewmate.CrewmateInvestigative;

public class PlaceBeaconButton : TownOfUsButton
{
    public override string Name => "Place Beacon";
    public override float Cooldown => OptionGroupSingleton<SentinelOptions>.Instance.PlaceBeaconCooldown.Value;
    public override float EffectDuration => OptionGroupSingleton<SentinelOptions>.Instance.PlaceBeaconDuration.Value;
    public override int MaxUses => 0;
    public override LoadableAsset<Sprite> Sprite => DivaniAssets.SentinelPlaceBeaconButton;
    public override ButtonLocation Location { get; set; } = ButtonLocation.BottomRight;
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;

    private static readonly Color SentinelColor = SentinelRole.SentinelColor;
    public override Color TextOutlineColor => SentinelColor;

    public static PlaceBeaconButton? Instance { get; private set; }
    
    private bool _isPlacing;

    public override bool Enabled(RoleBehaviour? role)
    {
        Instance = this;
        return role is SentinelRole;
    }

    private static bool BeaconVisibleNow()
    {
        var player = PlayerControl.LocalPlayer;
        if (player == null || player.Data == null || player.Data.IsDead) return false;
        if (player.Data.Role is not SentinelRole) return false;
        return BeaconManager.IsInRoom(player.GetTruePosition());
    }

    public override void SetActive(bool visible, RoleBehaviour role)
    {
        Button?.ToggleVisible(visible && Enabled(role) && BeaconVisibleNow());
    }

    protected override void FixedUpdate(PlayerControl playerControl)
    {
        if (MeetingHud.Instance)
        {
            return;
        }

        var hudActive = HudManager.Instance.UseButton.isActiveAndEnabled ||
                        HudManager.Instance.PetButton.isActiveAndEnabled;
        Button?.gameObject.SetActive(hudActive && BeaconVisibleNow());
    }

    public override bool CanUse()
    {
        var player = PlayerControl.LocalPlayer;
        if (player == null || player.Data == null || player.Data.IsDead) return false;

        if (MeetingHud.Instance || ExileController.Instance)
            return false;

        // Disabled during comms sabotage
        if (PlayerTask.PlayerHasTaskOfType<IHudOverrideTask>(player))
            return false;
        
        if (_isPlacing) return false;

        int maxBeacons = (int)OptionGroupSingleton<SentinelOptions>.Instance.MaxBeacons.Value;
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

        int maxBeacons = (int)OptionGroupSingleton<SentinelOptions>.Instance.MaxBeacons.Value;
        if (BeaconManager.BeaconsPlaced >= maxBeacons) return;

        var position = player.GetTruePosition();
        if (!BeaconManager.IsInRoom(position)) return;

        if (_isPlacing) return;

        Coroutines.Start(PlaceBeaconCoroutine(player, position));
    }

    private IEnumerator PlaceBeaconCoroutine(PlayerControl player, Vector2 capturedPosition)
    {
        _isPlacing = true;
        
        var colorHex = ColorUtility.ToHtmlStringRGB(SentinelColor);
        MiraAPI.Utilities.Helpers.CreateAndShowNotification(
            $"<b><color=#{colorHex}>Placing beacon...</color></b>",
            Color.white,
            new Vector3(0f, 1f, -20f),
            spr: DivaniAssets.SentinelIcon.LoadAsset());
        
        yield return new WaitForSeconds(EffectDuration);
        
        if (player == null || player.Data == null || player.Data.IsDead)
        {
            _isPlacing = false;
            yield break;
        }
        
        var roomName = BeaconManager.GetRoomName(capturedPosition) ?? "Unknown";
        
        BeaconManager.RpcPlaceBeacon(player, capturedPosition.x, capturedPosition.y);

        int beaconNum = BeaconManager.BeaconsPlaced;
        char label = (char)('A' + beaconNum - 1);
        var message = $"<b><color=#{colorHex}>Beacon {label} placed in {roomName}!</color></b>";

        MiraAPI.Utilities.Helpers.CreateAndShowNotification(
            message,
            Color.white,
            new Vector3(0f, 1f, -20f),
            spr: DivaniAssets.SentinelIcon.LoadAsset());

        
        _isPlacing = false;
    }
}
