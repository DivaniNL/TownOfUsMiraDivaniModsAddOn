using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using DivaniMods.Assets;
using DivaniMods.Options;
using DivaniMods.Patches;
using DivaniMods.Roles.Neutral.NeutralKilling;
using TownOfUs.Buttons;
using UnityEngine;

namespace DivaniMods.Buttons.Neutral.NeutralKilling;

public class FragGiveBombButton : TownOfUsTargetButton<PlayerControl>, IDiseaseableButton
{
    private static bool _cooldownQueued;
    private static bool _bombCountdownOnButton;
    private static bool _giveCooldownRunning;

    public static FragGiveBombButton? Instance { get; private set; }

    public override string Name => "Give Frag";
    public override float Cooldown => OptionGroupSingleton<FragOptions>.Instance.GiveBombCooldown.Value;

    public override float EffectDuration =>
        FragBombState.IsActive && !FragBombState.IsArmed
            ? Mathf.Max(FragBombState.ArmingDuration, 1f)
            : Mathf.Max(OptionGroupSingleton<FragOptions>.Instance.BombTimer.Value, 1f);
    public override int MaxUses => 0;
    public override LoadableAsset<Sprite> Sprite => DivaniAssets.FragGiveButton;
    public override float Distance => 1.5f;
    public override ButtonLocation Location { get; set; } = ButtonLocation.BottomRight;
    public override Color TextOutlineColor => FragRole.FragColor;
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;

    public override bool Enabled(RoleBehaviour? role)
    {
        Instance = this;
        ApplyQueuedCooldown();
        return role is FragRole;
    }
    public void SetDiseasedTimer(float multiplier)
    {
        SetTimer(Cooldown * multiplier);
    }
    public override PlayerControl? GetTarget()
    {
        var localPlayer = PlayerControl.LocalPlayer;
        if (localPlayer == null) return null;

        return localPlayer.GetClosestPlayer(true, Distance, true);
    }

    public override void SetOutline(bool active)
    {
        if (Target == null) return;
        Target.cosmetics.SetOutline(active, new Il2CppSystem.Nullable<Color>(Palette.ImpostorRed));
    }

    public override bool IsTargetValid(PlayerControl? target)
    {
        if (target == null || target.Data == null || target.Data.IsDead || target.Data.Disconnected) return false;

        var localPlayer = PlayerControl.LocalPlayer;
        if (localPlayer == null || target.PlayerId == localPlayer.PlayerId) return false;
        if (localPlayer.Data?.Role is not FragRole) return false;
        if (FragBombState.IsActive) return false;

        return true;
    }

    public override bool CanUse()
    {
        ApplyQueuedCooldown();

        var localPlayer = PlayerControl.LocalPlayer;
        if (localPlayer == null || localPlayer.Data == null || localPlayer.Data.IsDead) return false;
        if (localPlayer.Data.Role is not FragRole) return false;
        if (FragBombState.IsActive) return false;

        return base.CanUse() && Timer <= 0;
    }

    public override void ClickHandler()
    {
        if (!CanUse()) return;

        OnClick();
        Button?.SetDisabled();
    }

    protected override void OnClick()
    {
        var localPlayer = PlayerControl.LocalPlayer;
        if (localPlayer == null || Target == null) return;
        if (!IsTargetValid(Target)) return;

        var delay = UnityEngine.Random.Range(2f, 7f);
        var duration = OptionGroupSingleton<FragOptions>.Instance.BombTimer.Value;

        FragBombState.PlayGivePassSoundLocal();
        FragBombButton.RpcPassBomb(localPlayer, Target.PlayerId, byte.MaxValue, duration, delay);
        ResetTarget();
    }

    public static void StartCooldown()
    {
        _cooldownQueued = true;
        ApplyQueuedCooldown();
    }

    public static void UpdateVisuals()
    {
        var instance = Instance;
        if (instance?.Button == null) return;

        if (MeetingHud.Instance || ExileController.Instance)
        {
            instance.Button.ToggleVisible(false);
            return;
        }

        ApplyQueuedCooldown();

        var local = PlayerControl.LocalPlayer;
        var isFrag = local?.Data?.Role is FragRole;
        var showBombOnButton = FragBombState.IsActive && isFrag;

        if (showBombOnButton)
        {
            instance.OverrideName(FragBombState.IsArmed ? "ARMED" : "ARMING");
            _bombCountdownOnButton = true;
            _giveCooldownRunning = false;
            instance.TimerPaused = true;
            instance.EffectActive = true;
            instance.SetTimer(Mathf.Max(0f, FragBombState.GetSecondsUntilExplosionForDisplay()));
            instance.Button.buttonLabelText.color = Color.gray;
        }
        else
        {
            instance.OverrideName("GIVE FRAG");
            instance.TimerPaused = false;
            instance.EffectActive = false;
            if (_bombCountdownOnButton)
            {
                _bombCountdownOnButton = false;
                if (!FragBombState.IsActive && !_cooldownQueued && !_giveCooldownRunning)
                {
                    instance.SetTimer(0f);
                }
            }

            if (_giveCooldownRunning && instance.Timer <= 0f)
            {
                _giveCooldownRunning = false;
            }

            instance.Button.buttonLabelText.color = Color.white;
        }
    }

    private static void ApplyQueuedCooldown()
    {
        var instance = Instance;
        if (!_cooldownQueued || instance == null) return;

        instance.Timer = instance.Cooldown;
        instance.Button?.SetDisabled();
        _cooldownQueued = false;
        _giveCooldownRunning = true;
    }
}
