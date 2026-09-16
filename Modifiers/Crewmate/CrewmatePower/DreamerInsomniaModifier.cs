using MiraAPI.Modifiers;

namespace DivaniMods.Modifiers.Crewmate.CrewmatePower;

public sealed class DreamerInsomniaModifier(int rounds) : BaseModifier
{
    public override string ModifierName => "Insomnia";
    public override bool HideOnUi => true;

    public int RoundsLeft { get; set; } = rounds;

    public override void OnDeath(DeathReason reason)
    {
        ModifierComponent?.RemoveModifier(this);
    }
}
