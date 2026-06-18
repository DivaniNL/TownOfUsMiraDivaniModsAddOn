using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using DivaniMods.Assets;
using DivaniMods.Options;
using DivaniMods.Roles.Neutral.NeutralBenign;
using TownOfUs;
using TownOfUs.Buttons;
using TownOfUs.Utilities;
using UnityEngine;

namespace DivaniMods.Buttons.Neutral.NeutralBenign;

public sealed class CupidMatchmakeButton : TownOfUsRoleButton<CupidRole, PlayerControl>
{
    public override string Name => "Matchmake";
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => CupidRole.CupidColor;
    public override float Cooldown => Math.Clamp(OptionGroupSingleton<CupidOptions>.Instance.MatchmakeCooldown.Value + MapCooldown, 5f, 120f);
    public override LoadableAsset<Sprite> Sprite => DivaniAssets.CupidMatchmakeButton;

    public override bool Enabled(RoleBehaviour? role)
    {
        return base.Enabled(role) && role is CupidRole cupid && !cupid.Finalized;
    }

    public override bool CanUse()
    {
        if (Role == null || Role.Finalized)
        {
            return false;
        }
        return base.CanUse() && Target != null && Timer <= 0;
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            return;
        }
        CupidRole.RpcSetMatchTarget(PlayerControl.LocalPlayer, Target.PlayerId);
    }

    public override PlayerControl? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance, predicate: IsValidTarget);
    }

    private bool IsValidTarget(PlayerControl plr)
    {
        if (plr == null || plr.Data == null || plr.HasDied() || plr.AmOwner)
        {
            return false;
        }
        if (Role != null && Role.ProvisionalTargets.Contains(plr.PlayerId))
        {
            return false;
        }
        return true;
    }
}
