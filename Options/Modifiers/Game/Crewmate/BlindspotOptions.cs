using MiraAPI.Translation;
using DivaniMods.Modifiers.Game.Crewmate;
using TownOfUs.Options;

namespace DivaniMods.Options;
// No idea if this needed localization since I can't check in-game but did it anyway
public class BlindspotOptions : AbstractTouModifierOptionGroup<BlindspotModifier>
{
    public override Func<bool> GroupVisible => () => false;
    public override string GroupName => MiraLocaleManager.Get("DivaniMods.Modifier.Blindspot", "Blindspot");
    public override uint GroupPriority => 25;
}
