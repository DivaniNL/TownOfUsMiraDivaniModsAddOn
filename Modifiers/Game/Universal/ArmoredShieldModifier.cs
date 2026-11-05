using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using DivaniMods.Assets;
using TownOfUs.Modifiers;
using UnityEngine;

namespace DivaniMods.Modifiers.Game.Universal;

public sealed class ArmoredShieldModifier : BaseShieldModifier
{
    public override string ModifierName => "Armored";
    public override LoadableAsset<Sprite>? ModifierIcon => DivaniAssets.ArmoredIcon;
    public override bool HideOnUi => true;
    public override bool VisibleSymbol => false;
    public override void FixedUpdate()
    {
        base.FixedUpdate();

        if (Player == null ||
            !Player.TryGetModifier<ArmoredModifier>(out var armored) ||
            armored.AttacksRemaining <= 0)
        {
            ModifierComponent?.RemoveModifier(this);
        }
    }
}
