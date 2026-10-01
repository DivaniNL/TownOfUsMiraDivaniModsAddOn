using MiraAPI.Modifiers;
using MiraAPI.Translation;

namespace DivaniMods.Modifiers.Crewmate.CrewmatePower;

public sealed class MentorInsomniaModifier(int rounds) : BaseModifier
{
    public override string ModifierName => MiraLocaleManager.Get("DivaniMods.Modifier.MentorInsomnia", "Insomnia");
    public override bool HideOnUi => true;

    public int RoundsLeft { get; set; } = rounds;

    public override void OnDeath(DeathReason reason)
    {
        ModifierComponent?.RemoveModifier(this);
    }
}
