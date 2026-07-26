using DivaniMods.Modifiers.Game.Crewmate;
using TownOfUs.Options;

namespace DivaniMods.Options;

public class BlindspotOptions : AbstractTouModifierOptionGroup<BlindspotModifier>
{
    public override Func<bool> GroupVisible => () => false;
    public override string GroupName => "Blindspot";
    public override uint GroupPriority => 25;
}
