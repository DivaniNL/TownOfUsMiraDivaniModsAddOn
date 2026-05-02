using MiraAPI.GameOptions;
using DivaniMods.Modifiers;

namespace DivaniMods.Options;

public class MisvoteOptions : AbstractOptionGroup<MisvoteModifier>
{
    public override Func<bool> GroupVisible => () => false;
    public override string GroupName => "Misvote";
}
