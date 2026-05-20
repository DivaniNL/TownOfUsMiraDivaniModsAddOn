using MiraAPI.Modifiers;

namespace DivaniMods.Modifiers.Neutral.NeutralEvil;

public sealed class InnocentTargetModifier(byte innocentPlayerId) : BaseModifier
{
    public byte InnocentPlayerId => innocentPlayerId;

    public override string ModifierName => "Innocent Target";
    public override bool HideOnUi => true;
}
