using DivaniMods.Modules.Monster;
using DivaniMods.Roles.Neutral.NeutralKilling;
using MiraAPI.Utilities.Assets;
using DivaniMods.Assets;
using TownOfUs.Buttons;
using UnityEngine;
using TownOfUs.Utilities;

namespace DivaniMods.Buttons.Neutral.NeutralKilling;

public sealed class MonsterDevourButton : TownOfUsRoleButton<MonsterRole, PlayerControl>
{
    public override string Name => "Devour";
    public override float EffectDuration => 0f;
    public override bool HasEffect => false;
    public override Color TextOutlineColor => MonsterRole.MonsterColor;
    public override LoadableAsset<Sprite> Sprite => DivaniAssets.MonsterDevourButton;

    public override float Cooldown
    {
        get
        {
            var local = PlayerControl.LocalPlayer;
            return local != null ? MonsterState.CooldownFor(local.PlayerId) : 20f;
        }
    }

    public override PlayerControl? GetTarget()
    {
        var player = PlayerControl.LocalPlayer;
        if (player == null) return null;

        PlayerControl? closest = null;
        var closestDist = float.MaxValue;
        var myPos = player.GetTruePosition();

        foreach (var other in PlayerControl.AllPlayerControls)
        {
            if (other == null || other.PlayerId == player.PlayerId) continue;
            if (other.Data == null || other.Data.Disconnected || other.HasDied()) continue;
            if (MonsterState.IsEaten(other.PlayerId)) continue;

            var dist = Vector2.Distance(myPos, other.GetTruePosition());
            if (dist > Distance || dist >= closestDist) continue;

            closestDist = dist;
            closest = other;
        }

        return closest;
    }

    public override bool IsTargetValid(PlayerControl? target)
    {
        if (target == null || target.HasDied() || target.Data == null) return false;
        if (target.Data.Disconnected || MonsterState.IsEaten(target.PlayerId)) return false;

        return base.IsTargetValid(target);
    }

    public override bool CanUse()
    {
        if (!base.CanUse()) return false;

        var player = PlayerControl.LocalPlayer;
        return player != null && MonsterState.HasRoomToEat(player.PlayerId) && Timer <= 0;
    }

    protected override void OnClick()
    {
        var player = PlayerControl.LocalPlayer;
        if (player == null || Target == null) return;

        MonsterRole.RpcEat(player, Target.PlayerId);
    }
}