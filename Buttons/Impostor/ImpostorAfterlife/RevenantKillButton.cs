using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Utilities.Assets;
using DivaniMods.Options;
using DivaniMods.Networking.Impostor.ImpostorAfterlife;
using DivaniMods.Roles.Impostor.ImpostorAfterlife;
using TownOfUs.Assets;
using TownOfUs.Buttons;
using TownOfUs.Utilities;
using UnityEngine;

namespace DivaniMods.Buttons.Impostor.ImpostorAfterlife;

public sealed class RevenantKillButton : TownOfUsKillRoleButton<RevenantRole, PlayerControl>, IKillButton
{
    public override string Name => "Kill";
    public override float Cooldown => OptionGroupSingleton<SummonerOptions>.Instance.RevenantKillCooldown.Value;
    public override float EffectDuration => 0f;
    public override int MaxUses => 1;
    public override LoadableAsset<Sprite> Sprite => TouAssets.KillSprite;
    public override float Distance => 1.5f;
    public override ButtonLocation Location { get; set; } = ButtonLocation.BottomRight;
    public override Color TextOutlineColor => RevenantRole.RevenantColor;
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override bool UsableInDeath => true;

    public override bool Enabled(RoleBehaviour? role)
    {
        return role is RevenantRole { GhostActive: true };
    }

    public override PlayerControl? GetTarget()
    {
        var player = PlayerControl.LocalPlayer;
        return player == null ? null : MiscUtils.GetImpostorTarget(Distance);
    }

    public override void SetOutline(bool active)
    {
        if (Target is PlayerControl target)
        {
            target.cosmetics.SetOutline(active, new Il2CppSystem.Nullable<Color>(RevenantRole.RevenantColor));
        }
    }

    public override bool CanUse()
    {
        var player = PlayerControl.LocalPlayer;
        if (player?.Data?.Role is not RevenantRole revenant || !revenant.GhostActive)
        {
            return false;
        }

        return base.CanUse() && Target != null;
    }

    protected override void OnClick()
    {
        var player = PlayerControl.LocalPlayer;
        if (player == null || Target == null)
        {
            return;
        }

        if (player.Data?.Role is not RevenantRole)
        {
            return;
        }

        RevenantRpc.RpcRevenantKill(player, Target);
        ResetTarget();
    }
}
