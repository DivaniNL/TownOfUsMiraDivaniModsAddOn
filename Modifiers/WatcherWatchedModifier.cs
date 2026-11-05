using MiraAPI.Utilities.Assets;
using MiraAPI.Translation;
using TownOfUs.Modifiers;
using UnityEngine;

namespace DivaniMods.Modifiers;

public sealed class WatcherWatchedModifier : DisabledModifier
{
    public static bool KillButtonContext { get; set; }

    public override string ModifierName => MiraLocaleManager.Get("DivaniMods.Modifier.WatcherWatched", "Watched"); //Freeplay
    public override bool HideOnUi => true;
    public override LoadableAsset<Sprite>? ModifierIcon => null;

    public override bool CanUseAbilities => KillButtonContext;
    public override bool CanUseConsoles => true;
    public override bool CanReport => false;
    public override bool CanOpenMap => true;
    public override bool IsConsideredAlive => true;
    public override bool CanBeInteractedWith => true;
}
