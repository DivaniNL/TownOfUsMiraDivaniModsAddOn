using MiraAPI.Modifiers;
using Reactor.Utilities.Extensions;
using TownOfUs.Modifiers;
using TownOfUs.Utilities;
using TownOfUs.Utilities.Appearances;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DivaniMods.Modifiers.Neutral.NeutralOutlier;

public sealed class DuelReturnInvisModifier : ConcealedModifier, IVisualAppearance
{
    public override string ModifierName => "Swooped";
    public override float Duration => 5f;
    public override bool HideOnUi => true;
    public override bool AutoStart => true;
    public bool VisualPriority => true;

    public VisualAppearance GetVisualAppearance()
    {
        var observer = PlayerControl.LocalPlayer;
        var canSee = Player.AmOwner || (observer != null && DeathHandlerModifier.IsFullyDead(observer));
        var playerColor = canSee ? new Color(0f, 0f, 0f, 0.1f) : Color.clear;

        var app = new VisualAppearance(Player.GetDefaultAppearance(), TownOfUsAppearances.PlayerNameOnly)
        {
            HatId = string.Empty,
            SkinId = string.Empty,
            VisorId = string.Empty,
            PetId = string.Empty,
            PlayerName = string.Empty,
            NameVisible = false,
            RendererColor = playerColor,
            NameColor = Color.clear,
            ColorBlindTextColor = Color.clear,
        };
        return app;
    }

    public override void OnActivate()
    {
        Player.RawSetAppearance(this);
        Player.cosmetics.ToggleNameVisible(false);
        if (!Player.HasModifier<DuelReturnDisabledModifier>())
        {
            Player.AddModifier<DuelReturnDisabledModifier>();
        }
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        var mushroom = Object.FindObjectOfType<MushroomMixupSabotageSystem>();
        if (mushroom && mushroom.IsActive)
        {
            Player.RawSetAppearance(this);
            Player.cosmetics.ToggleNameVisible(false);
        }
    }

    public override void OnDeath(DeathReason reason)
    {
        Player.RemoveModifier(this);
    }

    public override void OnMeetingStart()
    {
        Player.RemoveModifier(this);
    }

    public override void OnDeactivate()
    {
        Player.ResetAppearance();
        Player.cosmetics.ToggleNameVisible(true);
        if (Player.HasModifier<DuelReturnDisabledModifier>())
        {
            Player.RemoveModifier<DuelReturnDisabledModifier>();
        }
    }
}

public sealed class DuelReturnDisabledModifier : DisabledModifier
{
    public override string ModifierName => "Swooped";
    public override float Duration => 5f;
    public override bool AutoStart => true;
    public override bool RemoveOnComplete => false;
    public override bool CanUseAbilities => false;
}
