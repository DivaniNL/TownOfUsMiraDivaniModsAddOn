using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using TownOfUs.Modifiers;
using TownOfUs.Utilities;

namespace DivaniMods.Modifiers.Game.Alliance;

public sealed class YangMarkedModifier : BaseRevealModifier
{
    public override string ModifierName => "Yang Marked";
    public override bool HideOnUi => true;

    public override ChangeRoleResult ChangeRoleResult { get; set; } = ChangeRoleResult.Nothing;

    public override RoleBehaviour? ShownRole
    {
        get => Player?.Data?.Role;
        set { }
    }

    public override bool RevealRole
    {
        get => false;
        set { }
    }

    public override bool Visible
    {
        get
        {
            var local = PlayerControl.LocalPlayer;
            if (local == null)
            {
                return false;
            }

            return local.HasDied() ||
                   (local.TryGetModifier<YinYangModifier>(out var side) && side.Side == YinYangSide.Yang);
        }
        set { }
    }

    public override string ExtraNameText
    {
        get => $" {YinYangModifier.YangColor.ToTextColor()}{YinYangModifier.MarkSymbol}</color>";
        set { }
    }
}
