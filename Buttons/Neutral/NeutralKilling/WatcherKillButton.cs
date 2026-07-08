using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using MiraAPI.Utilities.Assets;
using DivaniMods.Assets;
using DivaniMods.Modules.Watcher;
using DivaniMods.Options;
using DivaniMods.Roles.Neutral.NeutralKilling;
using TownOfUs.Buttons;
using TownOfUs.Modifiers;
using TownOfUs.Options.Modifiers.Alliance;
using TownOfUs.Utilities;
using UnityEngine;

namespace DivaniMods.Buttons.Neutral.NeutralKilling;

public sealed class WatcherKillButton : TownOfUsKillRoleButton<WatcherRole, PlayerControl>, IDiseaseableButton, IKillButton
{
    public override string Name => TranslationController.Instance.GetStringWithDefault(StringNames.KillLabel, "Kill");
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => WatcherRole.WatcherColor;
    public override float Cooldown => OptionGroupSingleton<WatcherOptions>.Instance.KillCooldown.Value;
    public override LoadableAsset<Sprite> Sprite => DivaniAssets.WatcherKillButton;

    public override bool ZeroIsInfinite { get; set; } = true;

    public void SetDiseasedTimer(float multiplier)
    {
        SetTimer(Cooldown * multiplier);
    }

    public override bool CanUse()
    {
        if (OptionGroupSingleton<WatcherOptions>.Instance.LinkWatchKillCooldown.Value
            && WatcherLightSystem.IsActive)
        {
            return false;
        }

        return base.CanUse();
    }

    public override PlayerControl? GetTarget()
    {
        var player = PlayerControl.LocalPlayer;
        if (player == null)
        {
            return null;
        }

        if (!OptionGroupSingleton<LoversOptions>.Instance.LoversKillEachOther && player.IsLover())
        {
            return player.GetClosestLivingPlayer(true, Distance, false, x => !x.IsLover());
        }

        return player.GetClosestLivingPlayer(true, Distance);
    }

    public override bool IsTargetValid(PlayerControl? target)
    {
        if (target == null)
        {
            return false;
        }

        if (target.HasModifier<BaseShieldModifier>() || target.HasModifier<FirstDeadShield>())
        {
            return false;
        }

        return base.IsTargetValid(target);
    }

    protected override void OnClick()
    {
        var player = PlayerControl.LocalPlayer;
        if (player == null || Target == null || !IsTargetValid(Target))
        {
            return;
        }

        if (WatcherLightSystem.IsActive)
        {
            WatcherLightSystem.RegisterManualKill(Target.PlayerId);
        }

        player.RpcCustomMurder(Target);

        if (OptionGroupSingleton<WatcherOptions>.Instance.LinkWatchKillCooldown.Value)
        {
            var watch = CustomButtonSingleton<WatcherWatchButton>.Instance;
            watch?.SetTimer(watch.Cooldown);
        }
    }
}
