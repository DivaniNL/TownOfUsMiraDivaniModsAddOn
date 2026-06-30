using MiraAPI.Utilities.Assets;
using TownOfUs.Modifiers;
using UnityEngine;

namespace DivaniMods.Modifiers;

public sealed class WatcherWatchedModifier : DisabledModifier
{
    public override string ModifierName => "Watched";
    public override bool HideOnUi => true;
    public override LoadableAsset<Sprite>? ModifierIcon => null;

    public override bool CanUseAbilities => false;
    public override bool CanReport => false;
    public override bool CanOpenMap => true;
    public override bool IsConsideredAlive => true;
    public override bool CanBeInteractedWith => true;
}
