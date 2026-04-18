using MiraAPI.GameOptions;
using MiraAPI.Utilities.Assets;
using DivaniMods.Assets;
using DivaniMods.Options;
using TownOfUs.Interfaces;
using TownOfUs.Modifiers.Game;
using UnityEngine;

namespace DivaniMods.Modifiers;

public class ShuffleModifier : TouGameModifier, IColoredModifier
{
    public override string ModifierName => "Shuffle";
    public override string LocaleKey => "Shuffle";
    public override ModifierFaction FactionType => ModifierFaction.Universal;
    public override Color FreeplayFileColor => new Color32(0, 255, 30, 255);
    public Color ModifierColor => new Color32(0, 255, 30, 255);
    public override LoadableAsset<Sprite>? ModifierIcon => DivaniAssets.ShuffleIcon;
    
    private int _usesRemaining = -1;
    
    public int UsesRemaining
    {
        get
        {
            if (_usesRemaining < 0)
            {
                _usesRemaining = (int)OptionGroupSingleton<ShuffleOptions>.Instance.ShuffleUses.Value;
            }
            return _usesRemaining;
        }
        set => _usesRemaining = value;
    }
    
    public override string GetDescription() => $"Shuffle all players' positions! ({UsesRemaining} uses left)";
    
    public override int GetAssignmentChance() => 
        (int)OptionGroupSingleton<ShuffleOptions>.Instance.ShuffleChance.Value;
    
    public override int GetAmountPerGame() => 
        (int)OptionGroupSingleton<ShuffleOptions>.Instance.ShuffleAmount;
    
    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return base.IsModifierValidOn(role);
    }
    
    public override void OnActivate()
    {
        _usesRemaining = (int)OptionGroupSingleton<ShuffleOptions>.Instance.ShuffleUses.Value;
        DivaniPlugin.Instance.Log.LogInfo($"Shuffle modifier activated! Uses: {_usesRemaining}");
    }
}
