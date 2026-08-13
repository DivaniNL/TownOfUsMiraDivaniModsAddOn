using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;
using MiraAPI.Utilities.Assets;
using PowerTools;
using Reactor.Utilities.Extensions;
using DivaniMods.Assets;
using DivaniMods.Modifiers.Neutral.NeutralOutlier;
using DivaniMods.Options;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Game.Alliance;
using TownOfUs.Modules.Anims;
using TownOfUs.Options;
using TownOfUs.Roles;
using TownOfUs.Utilities;
using TownOfUs.Utilities.Appearances;
using UnityEngine;
using TownOfUs.Modules;

namespace DivaniMods.Modifiers.Crewmate.CrewmatePower;

public sealed class ShockShieldModifier(PlayerControl mage) : TimedModifier
{
    public override string ModifierName => "Shock Shield";
    public override float Duration => OptionGroupSingleton<MageOptions>.Instance.ShockShieldDuration.Value;
    public override bool AutoStart => true;
    public override bool HideOnUi => true;

    public PlayerControl Mage { get; } = mage;
    public GameObject? ShieldAnim { get; set; }

    private bool ShouldShow()
    {
        if (Player.HasModifier<DuelModifier>())
        {
            return false;
        }

        if (Mage != null && Mage.AmOwner)
        {
            return true;
        }

        var genOpt = OptionGroupSingleton<GeneralOptions>.Instance;
        if (GameHistory.IsFullyDead(PlayerControl.LocalPlayer) && genOpt.TheDeadKnow)
        {
            return true;
        }

        var visibility = (ShockShieldVisibility)OptionGroupSingleton<MageOptions>.Instance.ShockShieldVisibleTo.Value;
        switch (visibility)
        {
            case ShockShieldVisibility.MageAndTarget:
                return Player.AmOwner;

            case ShockShieldVisibility.Teammates:
                var local = PlayerControl.LocalPlayer;
                if (Mage != null && Mage.HasModifier<CrewpostorModifier>())
                {
                    return local.IsImpostor();
                }
                if (Mage != null && Mage.HasModifier<EgotistModifier>())
                {
                    return local.IsImpostor() || local.Is(RoleAlignment.NeutralKilling);
                }
                return local.IsCrewmate();

            default:
                return false;
        }
    }

    public override void OnActivate()
    {
        if (!ShouldShow())
        {
            return;
        }

        ShieldAnim = AnimStore.SpawnAnimBody(Player, DivaniAssets.ShockShieldPrefab.LoadAsset()!, false, -1.1f, -0.35f, 1.5f);
        if (ShieldAnim != null)
        {
            ShieldAnim.transform.localScale *= 0.5f;
            var anim = ShieldAnim.GetComponent<SpriteAnim>();
            if (anim != null)
            {
                anim.SetSpeed(2f);
            }
        }
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        if (!Player)
        {
            ModifierComponent?.RemoveModifier(this);
            return;
        }

        if (!MeetingHud.Instance && ShieldAnim)
        {
            ShieldAnim!.SetActive(!Player.IsConcealed() && ShouldShow());
        }
    }

    public override void OnDeath(DeathReason reason)
    {
        ModifierComponent?.RemoveModifier(this);
    }

    public override void OnMeetingStart()
    {
        ModifierComponent?.RemoveModifier(this);
    }

    public override void OnDeactivate()
    {
        if (ShieldAnim)
        {
            ShieldAnim!.Destroy();
        }
    }
}
