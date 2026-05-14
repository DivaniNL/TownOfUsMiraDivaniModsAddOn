using MiraAPI.Modifiers;

namespace DivaniMods.Modifiers.Neutral.NeutralEvil;

/// <summary>
/// Runtime-only marker on the taunted killer (meeting tint + ⊕ for the innocent / ghosts).
/// Not lobby-assignable; <see cref="HideOnUi"/> keeps it off modifier lists.
/// Cleared after the first post-taunt meeting resolves (exile win or window expiry).
/// </summary>
public sealed class InnocentTargetModifier(byte innocentPlayerId) : BaseModifier
{
    public byte InnocentPlayerId => innocentPlayerId;

    public override string ModifierName => "Innocent Target";
    public override bool HideOnUi => true;
}
