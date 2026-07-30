using DivaniMods.Modules.Monster;
using DivaniMods.Roles.Neutral.NeutralKilling;
using MiraAPI.Utilities.Assets;
using DivaniMods.Assets;
using TownOfUs.Buttons;
using UnityEngine;
using TownOfUs.Utilities;
using MiraAPI.GameOptions;
using DivaniMods.Options;
using TownOfUs.Assets;
using TownOfUs;
using MiraAPI.LocalSettings;
using MiraAPI.Modifiers;
using TownOfUs.Modifiers;

namespace DivaniMods.Buttons.Neutral.NeutralKilling;

public sealed class MonsterDevourButton : TownOfUsRoleButton<MonsterRole, PlayerControl>
{
    public override string Name => "Devour";
    public override float EffectDuration => 2f;
    public override bool HasEffect => true;
    public override int MaxUses => (int)OptionGroupSingleton<MonsterOptions>.Instance.MaxDevouredPerRound;
    public override Color TextOutlineColor => MonsterRole.MonsterColor;
    public override bool ZeroIsInfinite => true;
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override bool ShouldPauseInVent => true;
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

    public override void CreateButton(Transform parent)
    {
        base.CreateButton(parent);

        if (Button == null) return;

        Button.usesRemainingSprite.sprite = TouAssets.AbilityCounterPlayerSprite.LoadAsset();

        TownOfUsColors.UseBasic = false;
        if (TextOutlineColor != Color.clear)
        {
            SetTextOutline(TextOutlineColor);
            Button.usesRemainingSprite.color = TextOutlineColor;
        }

        TownOfUsColors.UseBasic =
            LocalSettingsTabSingleton<TownOfUsLocalRoleSettings>.Instance.UseCrewmateTeamColorToggle.Value;

        PassiveComp = Button.GetComponent<PassiveButton>();
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
        
        if (Target.HasModifier<FirstDeadShield>())
        {
            return;
        }

        if (Target.HasModifier<BaseShieldModifier>())
        {
            return;
        }

        MonsterRole.RpcEat(player, Target.PlayerId);
        if (LimitedUses)
            {
                Button?.SetUsesRemaining(UsesLeft);
            }
    }
}