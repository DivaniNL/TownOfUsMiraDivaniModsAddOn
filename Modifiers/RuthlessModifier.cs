using MiraAPI.GameOptions;
using MiraAPI.Utilities.Assets;
using DivaniMods.Assets;
using DivaniMods.Options;
using TownOfUs.Interfaces;
using TownOfUs.Modifiers.Game;
using UnityEngine;

namespace DivaniMods.Modifiers;

public class RuthlessModifier : TouGameModifier, IColoredModifier
{
    public override string ModifierName => "Ruthless";
    public override string LocaleKey => "Ruthless";
    public override ModifierFaction FactionType => ModifierFaction.ImpostorUtility;
    public override Color FreeplayFileColor => Palette.ImpostorRed;
    public Color ModifierColor => Palette.ImpostorRed;
    public override LoadableAsset<Sprite>? ModifierIcon => DivaniAssets.RuthlessIcon;
    
    public override string GetDescription() => "Your kills bypass Medic shields, GA protection, and Survivor vests.";
    
    public override int GetAssignmentChance() => 
        (int)OptionGroupSingleton<RuthlessOptions>.Instance.RuthlessChance.Value;
    
    public override int GetAmountPerGame() => 
        (int)OptionGroupSingleton<RuthlessOptions>.Instance.RuthlessAmount;
    
    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return role.TeamType == RoleTeamTypes.Impostor;
    }
    
    public override void OnActivate()
    {
        DivaniPlugin.Instance.Log.LogInfo("Ruthless modifier activated!");
    }
}
