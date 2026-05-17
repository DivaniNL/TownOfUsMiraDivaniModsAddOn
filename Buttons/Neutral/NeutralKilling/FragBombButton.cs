using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Reactor.Networking.Attributes;
using DivaniMods.Assets;
using DivaniMods.Options;
using DivaniMods.Patches;
using DivaniMods.Roles.Neutral.NeutralKilling;
using TownOfUs.Buttons;
using UnityEngine;

namespace DivaniMods.Buttons.Neutral.NeutralKilling;

public class FragBombButton : TownOfUsTargetButton<PlayerControl>
{
    public static FragBombButton? Instance { get; private set; }

    public override string Name => "Pass Bomb";
    public override float Cooldown => 0f;
    public override float InitialCooldown => 0f;
    public override float EffectDuration => 0f;
    public override int MaxUses => 0;
    public override LoadableAsset<Sprite> Sprite => DivaniAssets.FragPassButton;
    public override float Distance => 1.5f;
    public override ButtonLocation Location { get; set; } = ButtonLocation.BottomLeft;
    public override Color TextOutlineColor => FragRole.FragColor;
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;

    public override bool Enabled(RoleBehaviour? role)
    {
        Instance = this;
        var localPlayer = PlayerControl.LocalPlayer;
        if (localPlayer == null || localPlayer.Data == null || localPlayer.Data.IsDead) return false;

        // Pass Bomb is only visible once the bomb is armed. During the initial
        // 2-7 second arming window the holder has no Pass button at all, in
        // line with the spec that nothing about the bomb is visible/audible
        // until it arms.
        return FragBombState.IsHolder(localPlayer.PlayerId) && FragBombState.IsArmed;
    }

    public override PlayerControl? GetTarget()
    {
        var localPlayer = PlayerControl.LocalPlayer;
        if (localPlayer == null) return null;

        return localPlayer.GetClosestPlayer(true, Distance, true);
    }

    public override void SetOutline(bool active)
    {
        // Pass-button highlight intentionally has no outline / no name color
        // changes. The frag UI is otherwise loud enough.
    }

    public override bool IsTargetValid(PlayerControl? target)
    {
        if (target == null || target.Data == null || target.Data.IsDead || target.Data.Disconnected) return false;

        var localPlayer = PlayerControl.LocalPlayer;
        if (localPlayer == null || target.PlayerId == localPlayer.PlayerId) return false;

        if (!FragBombState.IsHolder(localPlayer.PlayerId)) return false;
        if (!FragBombState.IsArmed) return false;
        if (FragBombState.IsImmune(target.PlayerId)) return false;

        return true;
    }

    public override bool CanUse()
    {
        var localPlayer = PlayerControl.LocalPlayer;
        if (localPlayer == null || localPlayer.Data == null || localPlayer.Data.IsDead) return false;

        if (!FragBombState.IsHolder(localPlayer.PlayerId)) return false;
        if (!FragBombState.IsArmed) return false;

        return base.CanUse();
    }

    protected override void OnClick()
    {
        var localPlayer = PlayerControl.LocalPlayer;
        if (localPlayer == null || Target == null) return;
        if (!IsTargetValid(Target)) return;

        FragBombState.PlayGivePassSoundLocal();
        RpcPassBomb(localPlayer, Target.PlayerId, localPlayer.PlayerId, 0f, 0f);
        ResetTarget();
    }

    public static void UpdateVisuals()
    {
        var instance = Instance;
        if (instance?.Button == null) return;

        if (MeetingHud.Instance != null || ExileController.Instance != null)
        {
            instance.Button.ToggleVisible(false);
            return;
        }

        var localPlayer = PlayerControl.LocalPlayer;
        if (localPlayer == null) return;

        instance.OverrideName("PASS BOMB");

        if (FragBombState.IsHolder(localPlayer.PlayerId) && FragBombState.IsArmed)
        {
            instance.Button.buttonLabelText.text = "PASS BOMB";
            instance.Button.buttonLabelText.color = Color.white;
        }
        else
        {
            instance.Button.buttonLabelText.color = Color.white;
        }
    }

    [MethodRpc((uint)DivaniRpcCalls.FragPassBomb)]
    public static void RpcPassBomb(PlayerControl sender, byte targetId, byte immuneId, float duration, float armingDelay)
    {
        FragBombState.PassBomb(sender, targetId, immuneId, duration, armingDelay);
    }
}
