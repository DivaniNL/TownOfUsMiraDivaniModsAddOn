using MiraAPI.GameOptions;
using DivaniMods.Modifiers;

namespace DivaniMods.Options;

public class BlindspotOptions : AbstractOptionGroup<BlindspotModifier>
{
    public override Func<bool> GroupVisible => () => false;
    public override string GroupName => "Blindspot";
}
