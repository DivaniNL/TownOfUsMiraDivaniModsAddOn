using MiraAPI.Modifiers;
using MiraAPI.Translation;

namespace DivaniMods.Modifiers.Game.Universal;

public sealed class UAVActiveModifier : BaseModifier
{
    public override string ModifierName => MiraLocaleManager.Get("DivaniMods.Modifier.UAVActive", "UAV Active"); //For Freeplay
    public override bool HideOnUi => true;
}
