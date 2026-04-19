using MiraAPI.GameOptions;
using MiraAPI.Utilities.Assets;
using DivaniMods.Assets;
using DivaniMods.Options;
using TownOfUs.Interfaces;
using TownOfUs.Modifiers.Game;
using TownOfUs.Modules.Wiki;
using TownOfUs.Utilities;
using UnityEngine;

namespace DivaniMods.Modifiers;

public class FragileModifier : TouGameModifier, IColoredModifier, IWikiDiscoverable
{
    public override string ModifierName => "Fragile";
    public override string LocaleKey => "Fragile";
    public override ModifierFaction FactionType => ModifierFaction.UniversalPassive;
    public override Color FreeplayFileColor => new Color32(251, 252, 225, 255);
    public Color ModifierColor => new Color32(251, 252, 225, 255);
    public override LoadableAsset<Sprite>? ModifierIcon => DivaniAssets.FragileIcon;
    
    public override string GetDescription()
    {
        var chance = OptionGroupSingleton<FragileOptions>.Instance.ChanceToBreak.Value;
        return $"You have a {chance:0}% chance to break if any player interacts with you!";
    }

    public string GetAdvancedDescription() => GetDescription() + MiscUtils.AppendOptionsText(GetType());
    
    public override int GetAssignmentChance() => 
        (int)OptionGroupSingleton<FragileOptions>.Instance.FragileChance.Value;
    
    public override int GetAmountPerGame() => 
        (int)OptionGroupSingleton<FragileOptions>.Instance.FragileAmount;
    
    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return base.IsModifierValidOn(role);
    }
    
    public override void OnActivate()
    {
        DivaniPlugin.Instance.Log.LogInfo("Fragile modifier activated!");
    }
}
