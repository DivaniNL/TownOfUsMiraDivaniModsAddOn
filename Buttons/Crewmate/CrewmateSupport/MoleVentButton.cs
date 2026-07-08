using System.Linq;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using MiraAPI.Utilities.Assets;
using DivaniMods.Assets;
using DivaniMods.Options;
using DivaniMods.Roles.Crewmate.CrewmateSupport;
using TownOfUs.Buttons;
using TownOfUs.Modifiers;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace DivaniMods.Buttons.Crewmate.CrewmateSupport;

public sealed class MoleVentButton : TownOfUsTargetButton<Vent>
{
    public override string Name => TranslationController.Instance.GetStringWithDefault(StringNames.VentLabel, "Vent");
    public override BaseKeybind Keybind => Keybinds.VentAction;
    public override Color TextOutlineColor => MoleRole.MoleColor;
    public override float Cooldown => 0.001f;
    public override float InitialCooldown => 0.001f;
    public override LoadableAsset<Sprite> Sprite => DivaniAssets.MoleVentButton;
    public override bool ShouldPauseInVent => false;

    public override bool Enabled(RoleBehaviour? role)
    {
        if (role is MoleRole)
        {
            return true;
        }

        if (role == null)
        {
            return false;
        }

        if (role.IsImpostor || role is EngineerTouRole)
        {
            return false;
        }

        if (role is ICustomRole customRole && customRole.Configuration.CanUseVent)
        {
            return false;
        }

        if (role is not PlumberRole && role.CanVent)
        {
            return false;
        }

        if (!MoleRole.MoleVentsExist)
        {
            return false;
        }

        return OptionGroupSingleton<MoleOptions>.Instance.VentUsage switch
        {
            MoleVentUsage.Anyone => true,
            MoleVentUsage.Crewmates => role.IsCrewmate(),
            _ => false,
        };
    }

    public override void SetOutline(bool active)
    {
        if (Target != null && !PlayerControl.LocalPlayer.HasDied())
        {
            Target.SetOutline(active, true, MoleRole.MoleColor);
        }
    }

    public override Vent? GetTarget()
    {
        var player = PlayerControl.LocalPlayer;
        if (player == null || !(player.CanMove || player.inVent) || ShipStatus.Instance == null)
        {
            return null;
        }

        Vent? closest = null;
        var bestDistance = float.MaxValue;
        var isMole = player.Data?.Role is MoleRole;

        foreach (var vent in ShipStatus.Instance.AllVents)
        {
            if (!vent.name.StartsWith("MoleVent") || !vent.gameObject.activeSelf ||
                (!isMole && !vent.myRend.enabled))
            {
                continue;
            }

            var center = new Vector2(player.Collider.bounds.center.x, player.Collider.bounds.center.y);
            var position = new Vector2(vent.transform.position.x, vent.transform.position.y);
            var distance = Vector2.Distance(center, position);

            if (distance > vent.UsableDistance || distance >= bestDistance ||
                PhysicsHelpers.AnythingBetween(player.Collider, center, position, Constants.ShipOnlyMask, false))
            {
                continue;
            }

            bestDistance = distance;
            closest = vent;
        }

        return closest;
    }

    public override bool CanUse()
    {
        var player = PlayerControl.LocalPlayer;
        if (player == null || player.Data == null || player.HasDied())
        {
            return false;
        }

        if (HudManager.Instance.Chat.IsOpenOrOpening || MeetingHud.Instance)
        {
            return false;
        }

        if (player.GetModifiers<DisabledModifier>().Any(x => !x.CanUseAbilities))
        {
            return false;
        }

        if (MoleRole.VentsDisabledByPlayerCount())
        {
            if (player.inVent && Vent.currentVent != null)
            {
                Vent.currentVent.SetButtons(false);
                player.MyPhysics.RpcExitVent(Vent.currentVent.Id);
                player.MyPhysics.ExitAllVents();
            }

            return false;
        }

        var newTarget = GetTarget();
        if (newTarget != Target)
        {
            Target?.SetOutline(false, false);
        }

        Target = IsTargetValid(newTarget) ? newTarget : null;
        SetOutline(true);

        return player.inVent || (Timer <= 0 && Target != null);
    }

    public override void ClickHandler()
    {
        if (!CanUse())
        {
            return;
        }

        OnClick();
        Button?.SetDisabled();
    }

    protected override void OnClick()
    {
        var player = PlayerControl.LocalPlayer;
        if (player == null)
        {
            return;
        }

        if (!player.inVent)
        {
            if (Target != null)
            {
                player.MyPhysics.RpcEnterVent(Target.Id);
                Target.SetButtons(true);
                Timer = 0.001f;
            }

            return;
        }

        if (Vent.currentVent != null)
        {
            Vent.currentVent.SetButtons(false);
            player.MyPhysics.RpcExitVent(Vent.currentVent.Id);
        }

        Timer = Cooldown;
    }
}
