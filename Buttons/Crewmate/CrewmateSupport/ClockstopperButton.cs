using System;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Translation;
using MiraAPI.Utilities.Assets;
using DivaniMods.Assets;
using DivaniMods.Events.Crewmate.CrewmateSupport;
using DivaniMods.Networking.Crewmate.CrewmateSupport;
using DivaniMods.Options;
using TownOfUs.Buttons;
using TownOfUs.Buttons.Crewmate;
using TownOfUs.Modules;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using UnityEngine;

namespace DivaniMods.Buttons.Crewmate.CrewmateSupport;

public sealed class ClockstopperButton : TownOfUsRoleButton<TimeLordRole>
{
    public static ClockstopperButton? Instance { get; private set; }

    private static int _charges;
    private static float _lastChargeTime;
    private static int _taskProgress;

    public override string Name => MiraLocaleManager.Get("DivaniMods.Role.ClockstopperAbility.Button", "Reset");
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => TownOfUsColors.TimeLord;
    public override float Cooldown =>
        Math.Clamp(OptionGroupSingleton<TimeLordOptions>.Instance.RewindCooldown + TownOfUsButton.MapCooldown, 5f, 120f);
    public override LoadableAsset<Sprite> Sprite => DivaniAssets.ResetButton;
    public override int MaxUses => Options.UnlimitedUses ? -1 : (int)Options.MaxCharges.Value;

    private static ClockstopperAbilityOptions Options => OptionGroupSingleton<ClockstopperAbilityOptions>.Instance;

    public override bool Enabled(RoleBehaviour? role)
    {
        if (Instance != this)
        {
            Instance = this;
            SetUses(_charges);
        }

        return role is TimeLordRole && Options.Enabled;
    }

    protected override void FixedUpdate(PlayerControl playerControl)
    {
        base.FixedUpdate(playerControl);
        if (!playerControl.AmOwner || playerControl.Data?.Role is not TimeLordRole)
        {
            return;
        }

        UpdateTimedCharges();
    }

    public override bool CanUse()
    {
        return base.CanUse() && (Options.UnlimitedUses || _charges > 0);
    }

    protected override void OnClick()
    {
        var player = PlayerControl.LocalPlayer;
        if (player?.Data?.Role is not TimeLordRole || (!Options.UnlimitedUses && _charges <= 0))
        {
            return;
        }

        if (!Options.UnlimitedUses)
        {
            _charges--;
            SetUses(_charges);
        }

        ClockstopperRpc.RpcResetCooldowns(player);
        CustomButtonSingleton<TimeLordRewindButton>.Instance.ResetCooldownAndOrEffect();
    }

    public static void ResetCharges()
    {
        _charges = Math.Clamp((int)Options.StartingCharges.Value, 0, (int)Options.MaxCharges.Value);
        _taskProgress = 0;
        _lastChargeTime = Time.time;
        Instance?.SetUses(_charges);
    }

    public static void GrantTaskProgress()
    {
        if (!Options.GainChargesFromTasks)
        {
            return;
        }

        var tasksPerCharge = (int)Options.TasksPerCharge.Value;
        if (tasksPerCharge <= 0)
        {
            return;
        }

        _taskProgress++;
        if (_taskProgress >= tasksPerCharge)
        {
            GrantCharges(_taskProgress / tasksPerCharge);
            _taskProgress %= tasksPerCharge;
        }
    }

    public static void GrantRewindCharge()
    {
        if (Options.GainChargeOnRewind)
        {
            GrantCharges(1);
        }
    }

    private static void GrantCharges(int amount)
    {
        _charges = Math.Clamp(_charges + amount, 0, (int)Options.MaxCharges.Value);
        Instance?.SetUses(_charges);
    }

    private void UpdateTimedCharges()
    {
        var now = Time.time;
        if (!Options.GainChargesOverTime)
        {
            _lastChargeTime = now;
            return;
        }

        var interval = Options.TimePerCharge.Value;
        if (interval <= 0f)
        {
            return;
        }

        var earnedCharges = (int)((now - _lastChargeTime) / interval);
        if (earnedCharges <= 0)
        {
            return;
        }

        GrantCharges(earnedCharges);
        _lastChargeTime += earnedCharges * interval;
    }

}
