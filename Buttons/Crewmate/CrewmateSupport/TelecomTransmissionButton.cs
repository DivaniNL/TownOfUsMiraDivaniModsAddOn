using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using DivaniMods.Assets;
using DivaniMods.Options;
using DivaniMods.Roles.Crewmate.CrewmateSupport;
using TownOfUs.Buttons;
using UnityEngine;

namespace DivaniMods.Buttons.Crewmate.CrewmateSupport;

public sealed class TelecomTransmissionButton : TownOfUsTargetButton<PlayerControl>
{
    public override string Name => "Transmit";
    public override float Cooldown => (float)OptionGroupSingleton<TelecomOptions>.Instance.TransmissionCooldown.Value;
    public override float EffectDuration => Delay;
    public override bool HasEffect => Delay > 0f;
    public override LoadableAsset<Sprite> Sprite => DivaniAssets.TelecomTransmissionButton;
    public override float Distance => 2f;
    public override ButtonLocation Location { get; set; } = ButtonLocation.BottomRight;
    public override Color TextOutlineColor => TelecomRole.TelecomColor;
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;

    private PlayerControl? _pendingTarget;

    private static float Delay => (float)OptionGroupSingleton<TelecomOptions>.Instance.TransmissionDelay.Value;

    private static bool MidRoundMode =>
        OptionGroupSingleton<TelecomOptions>.Instance.TargetSelection.Value ==
        (int)TelecomTargetSelectionOptions.MidRound;

    public override bool Enabled(RoleBehaviour? role)
    {
        return role is TelecomRole && MidRoundMode;
    }

    public override PlayerControl? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetClosestPlayer(true, Distance, true);
    }

    public override void SetOutline(bool active)
    {
        if (Target == null)
        {
            return;
        }

        Target.cosmetics.SetOutline(active, new Il2CppSystem.Nullable<Color>(TelecomRole.TelecomColor));
    }

    public override bool IsTargetValid(PlayerControl? target)
    {
        if (!TelecomRole.IsValidTarget(target, PlayerControl.LocalPlayer))
        {
            return false;
        }

        if (PlayerControl.LocalPlayer.Data?.Role is TelecomRole role && target != null && target.PlayerId == role.TargetId)
        {
            return false;
        }

        return true;
    }

    public override bool CanUse()
    {
        if (EffectActive)
        {
            return false;
        }

        var player = PlayerControl.LocalPlayer;
        if (player == null || player.Data == null || player.Data.IsDead)
        {
            return false;
        }

        if (player.Data.Role is not TelecomRole role || !MidRoundMode || role.TransmittedThisRound)
        {
            return false;
        }

        return base.CanUse();
    }

    public override void ClickHandler()
    {
        if (EffectActive)
        {
            CancelTransmission();
            return;
        }

        if (!CanClick() || Target == null)
        {
            return;
        }

        _pendingTarget = Target;
        SoundManager.Instance.PlaySound(DivaniAssets.TelecomTransmissionSound.LoadAsset(), false);

        if (HasEffect)
        {
            EffectActive = true;
            Timer = EffectDuration;
            SetLabel("Transmissing");
        }
        else
        {
            ApplyTransmission();
            Timer = Cooldown;
        }
    }

    protected override void OnClick()
    {
    }

    public override void OnEffectEnd()
    {
        SetLabel(Name);
        ApplyTransmission();
    }

    private void ApplyTransmission()
    {
        var player = PlayerControl.LocalPlayer;
        if (player?.Data?.Role is not TelecomRole role || _pendingTarget == null)
        {
            _pendingTarget = null;
            return;
        }

        if (!TelecomRole.IsValidTarget(_pendingTarget, player))
        {
            _pendingTarget = null;
            return;
        }

        TelecomRole.RpcSetTransmission(player, _pendingTarget.PlayerId);
        role.TransmittedThisRound = true;
        _pendingTarget = null;
    }

    private void CancelTransmission()
    {
        EffectActive = false;
        Timer = 0f;
        _pendingTarget = null;
        SetLabel(Name);
    }

    private void SetLabel(string text)
    {
        if (Button != null && Button.buttonLabelText != null)
        {
            Button.buttonLabelText.text = text;
        }
    }
}
