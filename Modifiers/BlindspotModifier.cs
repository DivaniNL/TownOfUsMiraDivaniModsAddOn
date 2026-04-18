using MiraAPI.GameOptions;
using MiraAPI.Utilities.Assets;
using DivaniMods.Assets;
using DivaniMods.Options;
using TownOfUs.Interfaces;
using TownOfUs.Modifiers.Game;
using UnityEngine;

namespace DivaniMods.Modifiers;

public class BlindspotModifier : TouGameModifier, IColoredModifier
{
    public override string ModifierName => "Blindspot";
    public override string LocaleKey => "Blindspot";
    public override ModifierFaction FactionType => ModifierFaction.CrewmateUtility;
    public override Color FreeplayFileColor => new Color32(128, 126, 124, 255);
    public Color ModifierColor => new Color32(128, 126, 124, 255);
    public override LoadableAsset<Sprite>? ModifierIcon => DivaniAssets.BlindspotIcon;
    
    public override string GetDescription() => "Camera lights don't activate when you use cameras.";
    
    public override int GetAssignmentChance() => 
        (int)OptionGroupSingleton<BlindspotOptions>.Instance.BlindspotChance.Value;
    
    public override int GetAmountPerGame() => 
        (int)OptionGroupSingleton<BlindspotOptions>.Instance.BlindspotAmount;
    
    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return role.TeamType == RoleTeamTypes.Crewmate && base.IsModifierValidOn(role);
    }
    
    public override void OnActivate()
    {
        DivaniPlugin.Instance.Log.LogInfo("Blindspot modifier activated!");
    }
}
