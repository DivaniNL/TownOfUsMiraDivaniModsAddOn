using MiraAPI.Modifiers;

namespace DivaniMods.Modifiers.Neutral.NeutralBenign;

public sealed class CupidToBeLoversModifier(byte cupidPlayerId) : BaseModifier
{
    public byte CupidPlayerId => cupidPlayerId;

    public override string ModifierName => "Cupid To Be Lovers";
    public override bool HideOnUi => true;
}
