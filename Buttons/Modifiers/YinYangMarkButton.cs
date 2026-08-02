using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using DivaniMods.Assets;
using DivaniMods.Modifiers.Game.Alliance;
using DivaniMods.Options;
using TownOfUs.Buttons;
using UnityEngine;

namespace DivaniMods.Buttons.Modifiers;

public sealed class YinYangMarkButton : TownOfUsTargetButton<PlayerControl>
{
    public override string Name => "Mark";
    public override float EffectDuration => 0f;
    public override float Distance => 1.5f;
    public override ButtonLocation Location { get; set; } = ButtonLocation.BottomLeft;
    public override BaseKeybind Keybind => DivaniKeybinds.MarkPlayer;

    public override float Cooldown => OptionGroupSingleton<YinYangOptions>.Instance.MarkCooldown.Value;

    public override LoadableAsset<Sprite> Sprite =>
        Side?.Side == YinYangSide.Yang ? DivaniAssets.YangMarkButton : DivaniAssets.YinMarkButton;

    public override Color TextOutlineColor => Side?.SideColor ?? YinYangModifier.YinColor;

    private static YinYangModifier? Side
    {
        get
        {
            var side = YinYangModifier.GetSideOf(PlayerControl.LocalPlayer);

            return side == null || side.Side == YinYangSide.Unassigned ? null : side;
        }
    }

    public override bool Enabled(RoleBehaviour? role)
    {
        var side = Side;
        return side != null && !side.HasWon && !side.HasLost;
    }

    protected override void FixedUpdate(PlayerControl playerControl)
    {
        base.FixedUpdate(playerControl);

        if (Button == null || Button.graphic == null)
        {
            return;
        }

        var spr = Sprite.LoadAsset();
        if (Button.graphic.sprite != spr)
        {
            Button.graphic.sprite = spr;
        }
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

        Target.cosmetics.SetOutline(active, new Il2CppSystem.Nullable<Color>(TextOutlineColor));
    }

    public override bool IsTargetValid(PlayerControl? target)
    {
        if (target == null || target.Data == null || target.Data.IsDead || target.Data.Disconnected)
        {
            return false;
        }

        if (target == PlayerControl.LocalPlayer)
        {
            return false;
        }

        var side = Side;
        return side != null && !side.IsMarkedByMe(target);
    }

    public override bool CanUse()
    {
        var player = PlayerControl.LocalPlayer;
        if (player == null || player.Data == null || player.Data.IsDead)
        {
            return false;
        }

        var side = Side;
        if (side == null || side.HasWon || side.HasLost)
        {
            return false;
        }

        return base.CanUse();
    }

    protected override void OnClick()
    {
        if (MeetingHud.Instance || ExileController.Instance)
        {
            return;
        }

        var player = PlayerControl.LocalPlayer;
        var side = Side;

        if (player == null || Target == null || side == null || side.HasWon || side.HasLost ||
            !IsTargetValid(Target))
        {
            return;
        }

        YinYangModifier.RpcYinYangMark(player, Target);
        ResetTarget();

        if (side.HasMarkedEveryone())
        {
            YinYangModifier.RpcYinYangWin(player);
        }
    }
}
