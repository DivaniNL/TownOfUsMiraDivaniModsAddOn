using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;
using MiraAPI.Roles;
using DivaniMods.Assets;
using DivaniMods.Modifiers.Neutral.NeutralOutlier;
using DivaniMods.Options;
using DivaniMods.Roles.Crewmate.CrewmatePower;
using DivaniMods.Utilities;
using TownOfUs.Modifiers;
using TownOfUs.Patches;
using TownOfUs.Roles;
using TownOfUs.Utilities;
using TownOfUs.Utilities.Appearances;
using UnityEngine;

namespace DivaniMods.Modifiers.Crewmate.CrewmatePower;

public sealed class IllusionModifier(PlayerControl mage) : TimedModifier, IVisualAppearance
{
    public override string ModifierName => "Swooped";
    public override float Duration => OptionGroupSingleton<MageOptions>.Instance.IllusionDuration.Value;
    public override bool AutoStart => true;
    public override bool HideOnUi => true;
    public bool VisualPriority => true;

    public PlayerControl Mage { get; } = mage;

    private const string IllusionTimerId = "MageIllusion";

    private bool _applied;
    private bool _lastFooled;

    private bool IsLocalObserverFooled()
    {
        var obs = PlayerControl.LocalPlayer;
        if (!obs || obs.Data == null)
        {
            return false;
        }
        if (obs.PlayerId == Player.PlayerId || DeathHandlerModifier.IsFullyDead(obs))
        {
            return false;
        }
        if (obs.IsImpostorAligned())
        {
            return true;
        }

        var opts = OptionGroupSingleton<MageOptions>.Instance;
        var alignment = (obs.Data.Role as ITownOfUsRole)?.RoleAlignment;
        return alignment switch
        {
            RoleAlignment.NeutralKilling => true,
            RoleAlignment.CrewmateKilling => !opts.CrewKillingSeesIllusioned.Value,
            RoleAlignment.NeutralEvil => !opts.NeutralEvilSeesIllusioned.Value,
            RoleAlignment.NeutralBenign => !opts.NeutralBenignSeesIllusioned.Value,
            _ => false,
        };
    }

    public VisualAppearance? GetVisualAppearance()
    {
        if (!IsLocalObserverFooled())
        {
            return Player.GetDefaultModifiedAppearance();
        }

        return new VisualAppearance(Player.GetDefaultModifiedAppearance(), TownOfUsAppearances.Swooper)
        {
            HatId = "hat_NoHat",
            SkinId = "skin_None",
            VisorId = "visor_EmptyVisor",
            PetId = "pet_EmptyPet",
            PlayerName = string.Empty,
            RendererColor = Color.clear,
            NameColor = Color.clear,
            ColorBlindTextColor = Color.clear,
            NameVisible = false,
        };
    }

    public override void OnActivate()
    {
        Player.RawSetAppearance(this);
        _lastFooled = IsLocalObserverFooled();
        _applied = true;

        if (Player.AmOwner && OptionGroupSingleton<MageOptions>.Instance.IllusionTargetKnows.Value)
        {
            MiraAPI.Utilities.Helpers.CreateAndShowNotification(
                "<b><color=#1586A2FF>A Mage has cloaked you in an Illusion, hiding you from killers!</color></b>",
                Color.white,
                new Vector3(0f, 1f, -20f),
                spr: DivaniAssets.MageIcon.LoadAsset());

            DivaniTimers.Set(
                IllusionTimerId,
                "<color=#1586A2FF>Cloaking</color>",
                DivaniAssets.MageIllusionButton.LoadAsset(),
                OptionGroupSingleton<MageOptions>.Instance.IllusionDuration.Value);
        }
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        if (!Player || MeetingHud.Instance)
        {
            return;
        }

        var fooled = IsLocalObserverFooled();
        if (!_applied || fooled != _lastFooled)
        {
            Player.RawSetAppearance(this);
            _lastFooled = fooled;
            _applied = true;
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
        if (Player.AmOwner)
        {
            DivaniTimers.Remove(IllusionTimerId);
        }

        if (Player.TryGetModifier<DuelReturnInvisModifier>(out var duelInvis))
        {
            Player.RawSetAppearance(duelInvis);
            Player.cosmetics.ToggleNameVisible(false);
        }
        else
        {
            Player.ResetAppearance();
            Player.cosmetics.ToggleNameVisible(true);

            if (HudManagerPatches.CamouflageCommsEnabled)
            {
                Player.cosmetics.ToggleNameVisible(false);
            }
        }

        _applied = false;
    }
}
