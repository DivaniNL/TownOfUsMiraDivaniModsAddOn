using MiraAPI.GameOptions;
using MiraAPI.LocalSettings;
using MiraAPI.Roles;
using MiraAPI.Utilities.Assets;
using DivaniMods.Assets;
using DivaniMods.Options;
using DivaniMods.Roles.Neutral.NeutralBenign;
using TownOfUs;
using TownOfUs.Modifiers;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DivaniMods.Modifiers.Neutral.NeutralBenign;

public sealed class CupidProtectModifier(PlayerControl cupid) : BaseShieldModifier
{
    public override float Duration => OptionGroupSingleton<CupidOptions>.Instance.ProtectDuration.Value;
    public override string ModifierName => "Protected";
    public override LoadableAsset<Sprite>? ModifierIcon => DivaniAssets.CupidIcon;
    public override string ShieldDescription => "You are protected by Cupid!\nYou cannot be killed.";
    public override bool AutoStart => true;
    public PlayerControl Cupid => cupid;

    public override bool HideOnUi
    {
        get
        {
            var showProtect = OptionGroupSingleton<CupidOptions>.Instance.ShowProtect.Value;
            return !LocalSettingsTabSingleton<TownOfUsLocalRoleSettings>.Instance.ShowShieldHudToggle.Value ||
                   !OptionGroupSingleton<CupidOptions>.Instance.LoversKnowCupid ||
                   showProtect == (int)CupidProtectShowOptions.Cupid;
        }
    }

    public override void OnActivate()
    {
        var showProtect = OptionGroupSingleton<CupidOptions>.Instance.ShowProtect.Value;
        var cupidRole = CustomRoleUtils.GetActiveRolesOfType<CupidRole>().FirstOrDefault(x => x.IsLover(Player));

        var showProtectEveryone = showProtect == (int)CupidProtectShowOptions.Everyone;
        var showProtectLover = PlayerControl.LocalPlayer.PlayerId == Player.PlayerId &&
                               showProtect == (int)CupidProtectShowOptions.CupidAndLovers &&
                               OptionGroupSingleton<CupidOptions>.Instance.LoversKnowCupid;
        var showProtectCupid = cupidRole != null && PlayerControl.LocalPlayer.PlayerId == cupidRole.Player.PlayerId;

        if (showProtectEveryone || showProtectLover || showProtectCupid)
        {
            var roleEffectAnimation = Object.Instantiate(RoleManager.Instance.protectLoopAnim,
                Player.gameObject.transform);
            roleEffectAnimation.SetMaterialColor(7);
            roleEffectAnimation.SetMaskLayerBasedOnWhoShouldSee(true);
            roleEffectAnimation.Play(Player, new Action(OnDeactivate), Player.cosmetics.FlipX,
                RoleEffectAnimation.SoundType.Local, Duration);
        }
    }

    public override void OnDeactivate()
    {
        for (var i = Player.currentRoleAnimations.Count - 1; i >= 0; i--)
        {
            if (Player.currentRoleAnimations[i] != null && Player.currentRoleAnimations[i].effectType ==
                RoleEffectAnimation.EffectType.ProtectLoop)
            {
                Object.Destroy(Player.currentRoleAnimations[i].gameObject);
                Player.currentRoleAnimations.RemoveAt(i);
            }
        }
    }

    public override void OnMeetingStart()
    {
        ModifierComponent?.RemoveModifier(this);
    }

    public override void OnDeath(DeathReason reason)
    {
        ModifierComponent?.RemoveModifier(this);
    }
}
