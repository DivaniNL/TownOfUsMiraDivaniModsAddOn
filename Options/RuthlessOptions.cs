using MiraAPI.GameOptions;
using DivaniMods.Modifiers;

namespace DivaniMods.Options;

public class RuthlessOptions : AbstractOptionGroup<RuthlessModifier>
{
    public override Func<bool> GroupVisible => () => false;
    public override string GroupName => "Ruthless";
}
