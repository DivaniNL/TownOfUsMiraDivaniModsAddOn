using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using DivaniMods.Assets;
using DivaniMods.Options;
using DivaniMods.Patches;
using DivaniMods.Roles;
using UnityEngine;

namespace DivaniMods.Buttons;

public class FragGiveBombButton : CustomActionButton<PlayerControl>
{
    private static bool _cooldownQueued;
    /// <summary>True while we drive <see cref="Timer"/> from <see cref="FragBombState"/> for the on-button fuse.</summary>
    private static bool _bombCountdownOnButton;

    public static FragGiveBombButton? Instance { get; private set; }

    public override string Name => "Give Bomb";
    public override float Cooldown => OptionGroupSingleton<FragOptions>.Instance.GiveBombCooldown;

    /// <summary>
    /// Fuse ring denominator (Mirrorcaster-style effect timer). Must fit arming (≤7s) + longest fuse.
    /// </summary>
    public override float EffectDuration =>
        Mathf.Max(OptionGroupSingleton<FragOptions>.Instance.BombTimer + 7f, 1f);
    public override int MaxUses => 0;
    public override LoadableAsset<Sprite> Sprite => DivaniAssets.FragGiveButton;
    public override float Distance => 1.5f;
    public override ButtonLocation Location { get; set; } = ButtonLocation.BottomRight;
    public override Color TextOutlineColor => Palette.ImpostorRed;

    public override bool Enabled(RoleBehaviour? role)
    {
        Instance = this;
        ApplyQueuedCooldown();
        return role is FragRole;
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

        return base.CanUse();
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
        var duration = OptionGroupSingleton<FragOptions>.Instance.BombTimer;

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

        ApplyQueuedCooldown();

        var local = PlayerControl.LocalPlayer;
        var isFrag = local?.Data?.Role is FragRole;
        var showBombOnButton = FragBombState.IsActive && isFrag;

        if (showBombOnButton)
        {
            instance.OverrideName("ARMED");
            _bombCountdownOnButton = true;
            instance.TimerPaused = true;
            instance.EffectActive = true;
            instance.SetTimer(Mathf.Max(0f, FragBombState.GetSecondsUntilExplosionForDisplay()));
            instance.Button.buttonLabelText.color = Color.gray;
        }
        else
        {
            instance.OverrideName("GIVE BOMB");
            instance.TimerPaused = false;
            instance.EffectActive = false;
            if (_bombCountdownOnButton)
            {
                _bombCountdownOnButton = false;
                if (!FragBombState.IsActive && !_cooldownQueued)
                {
                    instance.SetTimer(0f);
                }
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
    }
}
