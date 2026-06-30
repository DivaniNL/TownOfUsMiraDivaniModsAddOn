using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Utilities.Assets;
using DivaniMods.Assets;
using DivaniMods.Modules.Watcher;
using DivaniMods.Networking.Neutral.NeutralKilling;
using DivaniMods.Options;
using DivaniMods.Roles.Neutral.NeutralKilling;
using TownOfUs.Buttons;
using UnityEngine;

namespace DivaniMods.Buttons.Neutral.NeutralKilling;

public sealed class WatcherWatchButton : TownOfUsButton
{
    public static WatcherWatchButton? Instance { get; private set; }

    public override string Name => "Watch";
    public override float Cooldown => OptionGroupSingleton<WatcherOptions>.Instance.WatchCooldown.Value;
    public override float EffectDuration => 0f;
    public override int MaxUses => (int)OptionGroupSingleton<WatcherOptions>.Instance.InitialWatchCharges.Value;
    public override LoadableAsset<Sprite> Sprite => DivaniAssets.WatcherWatchButton;
    public override ButtonLocation Location => ButtonLocation.BottomRight;
    public override Color TextOutlineColor => WatcherRole.WatcherColor;
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;

    private int _currentCharges = -1;
    private int _killsTowardCharge;

    public int KillsTowardCharge => _killsTowardCharge;

    public int CurrentCharges
    {
        get
        {
            if (_currentCharges < 0)
            {
                _currentCharges = MaxUses;
            }
            return _currentCharges;
        }
        set
        {
            _currentCharges = value;
            SetUses(value);
        }
    }

    public override bool Enabled(RoleBehaviour? role)
    {
        Instance = this;
        return role is WatcherRole;
    }

    public void AddCharges(int amount)
    {
        if (amount > 0)
        {
            CurrentCharges += amount;
        }
    }

    public void ResetCharges()
    {
        _currentCharges = -1;
        _killsTowardCharge = 0;
    }

    public void AccrueKill()
    {
        var per = (int)OptionGroupSingleton<WatcherOptions>.Instance.KillsPerExtraCharge.Value;
        if (per <= 0)
        {
            return;
        }

        _killsTowardCharge++;
        if (_killsTowardCharge >= per)
        {
            _killsTowardCharge = 0;
            AddCharges(1);
        }
    }

    protected override void FixedUpdate(PlayerControl playerControl)
    {
        base.FixedUpdate(playerControl);

        if (Button != null)
        {
            Button.buttonLabelText.text = WatcherLightSystem.IsActive ? "WATCHING" : "WATCH";
        }
    }

    public override bool CanUse()
    {
        var player = PlayerControl.LocalPlayer;
        if (player == null || player.Data == null || player.Data.IsDead)
        {
            return false;
        }

        if (WatcherLightSystem.IsActive)
        {
            return false;
        }

        if (!base.CanUse())
        {
            return false;
        }

        SetUses(CurrentCharges);
        return CurrentCharges > 0 && Timer <= 0;
    }

    public override void ClickHandler()
    {
        if (!CanUse())
        {
            return;
        }

        OnClick();

        if (CurrentCharges > 0)
        {
            CurrentCharges--;
        }
    }

    protected override void OnClick()
    {
        var player = PlayerControl.LocalPlayer;
        if (player == null)
        {
            return;
        }

        var opt = OptionGroupSingleton<WatcherOptions>.Instance;
        var green = UnityEngine.Random.Range(
            Mathf.Min(opt.GreenLightMinDuration.Value, opt.GreenLightMaxDuration.Value),
            Mathf.Max(opt.GreenLightMinDuration.Value, opt.GreenLightMaxDuration.Value));

        var loops = Mathf.Max(1, (int)opt.RedLightGreenLightLoops.Value);
        WatcherRpc.RpcStartLights(player, green, opt.RedLightDuration.Value, opt.RedLightGracePeriod.Value, loops);
    }

}
