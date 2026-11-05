using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.PluginLoading;
using MiraAPI.Utilities.Assets;
using DivaniMods.Assets;
using DivaniMods.Options;
using DivaniMods.Roles.Crewmate.CrewmateSupport;
using TMPro;
using TownOfUs.Buttons;
using UnityEngine;

namespace DivaniMods.Buttons.Crewmate.CrewmateSupport;

[MiraIgnore]
public abstract class PortalTeleportButtonBase : TownOfUsButton
{
    protected abstract int PortalIndex { get; }

    public override string Name => PortalManager.GetPortalRoomName(PortalIndex);
    public override float Cooldown => OptionGroupSingleton<PortalmakerOptions>.Instance.UsePortalCooldown.Value;
    public override float InitialCooldown => 0f;
    public override float EffectDuration => 0f;
    public override int MaxUses => 0;
    public override bool ZeroIsInfinite { get; set; } = true;
    public override LoadableAsset<Sprite> Sprite => DivaniAssets.UsePortalButton;
    public override ButtonLocation Location => ButtonLocation.BottomRight;
    public override Color TextOutlineColor => new Color(0.047f, 0.420f, 0.961f);

    public override bool Enabled(RoleBehaviour? role) => role is PortalmakerRole;

    private static bool ShouldShow()
    {
        var player = PlayerControl.LocalPlayer;
        if (player == null || player.Data == null || player.Data.IsDead) return false;
        if (player.Data.Role is not PortalmakerRole) return false;
        if (!OptionGroupSingleton<PortalmakerOptions>.Instance.PortalmakerDirectTeleport) return false;
        if (!PortalManager.BothPortalsPlaced) return false;
        if (OptionGroupSingleton<PortalmakerOptions>.Instance.EnableAfterFirstMeeting &&
            !PortalManager.PortalsUnlocked) return false;
        return true;
    }

    public override void SetActive(bool visible, RoleBehaviour role)
    {
        Button?.ToggleVisible(visible && Enabled(role) && ShouldShow());
    }

    protected override void FixedUpdate(PlayerControl playerControl)
    {
        if (MeetingHud.Instance)
        {
            return;
        }

        var hudActive = HudManager.Instance.UseButton.isActiveAndEnabled ||
                        HudManager.Instance.PetButton.isActiveAndEnabled;
        var show = hudActive && ShouldShow();
        Button?.gameObject.SetActive(show);

        if (show)
        {
            Button?.OverrideText(Name.ToUpperInvariant());

            if (KeybindIcon != null && KeybindIcon.transform.childCount > 0 && Keybind != null)
            {
                var label = KeybindIcon.transform.GetChild(0).GetComponent<TextMeshPro>();
                if (label != null)
                {
                    label.text = KeybindHelpers.PrettyKeyName(Keybind.CurrentKey);
                }
            }
        }
    }

    public override bool CanUse()
    {
        var player = PlayerControl.LocalPlayer;
        if (player == null || player.Data == null || player.Data.IsDead) return false;
        if (!ShouldShow()) return false;
        if (!PortalManager.CanUsePortal(player.PlayerId)) return false;
        return base.CanUse();
    }

    protected override void OnClick()
    {
        var player = PlayerControl.LocalPlayer;
        if (player == null) return;

        var destination = PortalManager.GetPortalDestination(PortalIndex);
        if (!destination.HasValue) return;

        PortalManager.RpcUsePortal(player, destination.Value.x, destination.Value.y);
        PortalManager.SyncPortalButtonCooldowns();
    }
}

public sealed class PortalTeleportButton1 : PortalTeleportButtonBase
{
    protected override int PortalIndex => 1;
    public override BaseKeybind Keybind => DivaniMods.DivaniKeybinds.TeleportPortal1;
}

public sealed class PortalTeleportButton2 : PortalTeleportButtonBase
{
    protected override int PortalIndex => 2;
    public override BaseKeybind Keybind => DivaniMods.DivaniKeybinds.TeleportPortal2;
}
