using TownOfUs;
using TownOfUs.Modifiers;
using TownOfUs.Utilities;
using UnityEngine;

namespace DivaniMods.Modifiers.Crewmate.CrewmatePower;

public sealed class OverworkedEvilRevealModifier : BaseRevealModifier
{
    public override string ModifierName => "Revealed Evil";
    public override bool HideOnUi => true;

    public override ChangeRoleResult ChangeRoleResult { get; set; } = ChangeRoleResult.Nothing;

    public override void OnActivate()
    {
        base.OnActivate();
        SetNewInfo(false, null, null, null, Color.red);
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        if (Player.IsImpostor())
        {
            NameColor = Color.red;
        }
        else if (!Player.HasDied())
        {
            NameColor = TownOfUsColors.Neutral;
        }
    }
}
