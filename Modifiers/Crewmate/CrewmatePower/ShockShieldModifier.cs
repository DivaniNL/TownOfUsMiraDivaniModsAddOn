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
using TownOfUs.Modules.Anims;
using TownOfUs.Options;
using TownOfUs.Utilities;
using TownOfUs.Utilities.Appearances;
using UnityEngine;

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

        var genOpt = OptionGroupSingleton<GeneralOptions>.Instance;
        var seesSelf = Player.AmOwner && OptionGroupSingleton<MageOptions>.Instance.TargetSeesShockShield.Value;
        var seesMage = Mage != null && Mage.AmOwner;
        var seesDead = DeathHandlerModifier.IsFullyDead(PlayerControl.LocalPlayer) && genOpt.TheDeadKnow;
        return seesSelf || seesMage || seesDead;
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
