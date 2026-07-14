using TownOfUs.Modifiers;
using UnityEngine;

namespace DivaniMods.Modifiers.Crewmate.CrewmatePower;

public sealed class WorkhorseRevealModifier : BaseRevealModifier
{
    public override string ModifierName => "Revealed Workhorse";
    public override bool HideOnUi => true;

    public override ChangeRoleResult ChangeRoleResult { get; set; } = ChangeRoleResult.Nothing;

    public override bool RevealRole { get => true; set { } }
    public override RoleBehaviour? ShownRole { get => Player?.Data?.Role; set { } }
    public override Color? NameColor { get => Player?.Data?.Role?.TeamColor; set { } }
}
