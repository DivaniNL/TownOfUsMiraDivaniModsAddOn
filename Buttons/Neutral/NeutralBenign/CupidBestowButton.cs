using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using DivaniMods.Assets;
using DivaniMods.Modifiers.Neutral.NeutralBenign;
using DivaniMods.Options;
using DivaniMods.Roles.Neutral.NeutralBenign;
using TownOfUs.Buttons;
using TownOfUs.Utilities;
using UnityEngine;

namespace DivaniMods.Buttons.Neutral.NeutralBenign;

public sealed class CupidBestowButton : TownOfUsRoleButton<CupidRole>
{
    public override string Name => "Bestow";
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => CupidRole.CupidColor;
    public override float Cooldown => Math.Clamp(OptionGroupSingleton<CupidOptions>.Instance.ProtectCooldown.Value + MapCooldown, 5f, 120f);
    public override float EffectDuration => OptionGroupSingleton<CupidOptions>.Instance.ProtectDuration.Value;
    public override LoadableAsset<Sprite> Sprite => DivaniAssets.CupidProtectButton;

    public override bool Enabled(RoleBehaviour? role)
    {
        return base.Enabled(role) && role is CupidRole cupid && cupid.Finalized &&
               !OptionGroupSingleton<CupidOptions>.Instance.ProtectSeparately;
    }

    protected override void OnClick()
    {
        if (Role == null)
        {
            return;
        }

        foreach (var lover in Role.GetCurrentCouple())
        {
            if (lover != null && !lover.HasDied())
            {
                lover.RpcAddModifier<CupidProtectModifier>(PlayerControl.LocalPlayer);
            }
        }
    }
}
