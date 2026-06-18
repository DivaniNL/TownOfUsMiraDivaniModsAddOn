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

public sealed class CupidBestowOneButton : TownOfUsRoleButton<CupidRole>
{
    private static readonly Color LabelColor = new Color32(0xAB, 0x30, 0xA5, 0xFF);

    public override string Name
    {
        get
        {
            var lp = PlayerControl.LocalPlayer;
            if (lp != null && lp.Data != null && lp.Data.Role is CupidRole cupid && cupid.LoverOne != null)
            {
                return $"<color=#AB30A5>{cupid.LoverOne.Data.PlayerName}</color>";
            }
            return "Bestow";
        }
    }
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => LabelColor;
    public override float Cooldown => Math.Clamp(OptionGroupSingleton<CupidOptions>.Instance.ProtectCooldown.Value + MapCooldown, 5f, 120f);
    public override float EffectDuration => OptionGroupSingleton<CupidOptions>.Instance.ProtectDuration.Value;
    public override LoadableAsset<Sprite> Sprite => DivaniAssets.CupidProtectOneButton;

    public override bool Enabled(RoleBehaviour? role)
    {
        return base.Enabled(role) && role is CupidRole cupid && cupid.Finalized &&
               OptionGroupSingleton<CupidOptions>.Instance.ProtectSeparately;
    }

    protected override void OnClick()
    {
        var lover = Role?.LoverOne;
        if (lover == null || lover.HasDied())
        {
            return;
        }
        lover.RpcAddModifier<CupidProtectModifier>(PlayerControl.LocalPlayer);
    }

    protected override void FixedUpdate(PlayerControl playerControl)
    {
        base.FixedUpdate(playerControl);
        if (Button != null)
        {
            OverrideName(Name);
        }
    }
}
