using MiraAPI.GameOptions;
using DivaniMods.Modifiers.Game.Impostor.ImpostorPassive;

namespace DivaniMods.Options;

public class RuthlessOptions : AbstractOptionGroup<RuthlessModifier>
{
    public override Func<bool> GroupVisible => () => false;
    public override string GroupName => "Ruthless";
}
