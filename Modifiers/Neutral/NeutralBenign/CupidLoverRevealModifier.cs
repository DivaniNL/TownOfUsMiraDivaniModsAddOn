using MiraAPI.GameOptions;
using DivaniMods.Options;
using TownOfUs.Modifiers;
using UnityEngine;

namespace DivaniMods.Modifiers.Neutral.NeutralBenign;
public sealed class CupidLoverRevealModifier : BaseRevealModifier
{
    public override string ModifierName => "Cupid Lover Reveal";
    public override bool HideOnUi => true;

    public override bool RevealRole { get => true; set { } }
    public override RoleBehaviour? ShownRole { get => Player?.Data?.Role; set { } }
    public override Color? NameColor { get => Player?.Data?.Role?.TeamColor; set { } }

    public override bool Visible
    {
        get => OptionGroupSingleton<CupidOptions>.Instance.CupidKnowsLoverRoles;
        set { }
    }
}
