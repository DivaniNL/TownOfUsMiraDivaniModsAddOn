using MiraAPI.Hud;
using MiraAPI.Utilities.Assets;
using DivaniMods.Assets;
using DivaniMods.Events.Crewmate.CrewmateKilling;
using DivaniMods.Networking.Crewmate.CrewmateKilling;
using DivaniMods.Roles.Crewmate.CrewmateAfterlife;
using DivaniMods.Roles.Crewmate.CrewmateKilling;
using TownOfUs.Buttons;
using UnityEngine;
using TownOfUs.Utilities;

namespace DivaniMods.Buttons.Crewmate.CrewmateAfterlife;

public sealed class RevengeButton : TownOfUsKillRoleButton<VengefulSoulRole, PlayerControl>, IKillButton
{
    public override string Name => "Revenge";
    public override float Cooldown => 0f;
    public override float InitialCooldown => 0f;
    public override float EffectDuration => 0f;
    public override int MaxUses => 0;
    public override bool ZeroIsInfinite { get; set; } = true;
    public override LoadableAsset<Sprite> Sprite => DivaniAssets.VengefulSoulRevengeButton;
    public override float Distance => 1.5f;
    public override ButtonLocation Location { get; set; } = ButtonLocation.BottomRight;
    public override Color TextOutlineColor => RetributionistRole.RetributionistColor;
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override bool UsableInDeath => true;

    public override bool Enabled(RoleBehaviour? role)
    {
        return role is VengefulSoulRole { GhostActive: true };
    }

    public override PlayerControl? GetTarget()
    {
        var player = PlayerControl.LocalPlayer;
        if (player == null)
        {
            return null;
        }

        if (!RetributionistManager.TryGetKiller(player.PlayerId, out var killer) || killer == null)
        {
            return null;
        }

        if (killer.HasDied() || killer.Data == null)
        {
            return null;
        }

        var distance = Vector2.Distance(player.transform.position, killer.transform.position);
        return distance <= Distance ? killer : null;
    }

    public override bool CanUse()
    {
        var player = PlayerControl.LocalPlayer;
        if (player?.Data?.Role is not VengefulSoulRole vengefulSoul || !vengefulSoul.GhostActive)
        {
            return false;
        }

        return base.CanUse() && Target != null;
    }

    public override void SetOutline(bool active)
    {
        if (Target is PlayerControl target)
        {
            target.cosmetics.SetOutline(active, new Il2CppSystem.Nullable<Color>(RetributionistRole.RetributionistColor));
        }
    }

    protected override void OnClick()
    {
        var player = PlayerControl.LocalPlayer;
        if (player == null || Target == null)
        {
            return;
        }

        if (player.Data?.Role is not VengefulSoulRole)
        {
            return;
        }

        RetributionistRpc.RpcRevenge(player, Target);
        ResetTarget();
    }
}
