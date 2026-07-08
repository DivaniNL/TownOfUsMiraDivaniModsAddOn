using MiraAPI.GameOptions;
using MiraAPI.Utilities.Assets;
using DivaniMods.Assets;
using DivaniMods.Options;
using TownOfUs.Interfaces;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Game;
using TownOfUs.Modules.Wiki;
using TownOfUs.Utilities;
using UnityEngine;

namespace DivaniMods.Modifiers.Game.Crewmate;

public class BlindspotModifier : TouGameModifier, IColoredModifier, IWikiDiscoverable
{
    public static readonly Color BlindspotColor = new Color32(128, 126, 124, 255);

    public override string ModifierName => "Blindspot";
    public override string LocaleKey => "Blindspot";
    public override string IntroInfo => "You watch camera's without lights.";
    public override ModifierFaction FactionType => ModifierFaction.CrewmateUtility;
    public override Color FreeplayFileColor => BlindspotColor;
    public Color ModifierColor => BlindspotColor;
    public override LoadableAsset<Sprite>? ModifierIcon => DivaniAssets.BlindspotIcon;
    
    public override string GetDescription() => "Camera lights don't activate when you use cameras.";

    public string GetAdvancedDescription() => GetDescription() + MiscUtils.AppendOptionsText(GetType());
    
    public override int GetAssignmentChance() =>
        (int)OptionGroupSingleton<CrewmateModifierOptions>.Instance.BlindspotChance.Value;
    
    public override int GetAmountPerGame() =>
        (int)OptionGroupSingleton<CrewmateModifierOptions>.Instance.BlindspotAmount.Value;
    
    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return role.IsCrewmate() && base.IsModifierValidOn(role);
    }
    
    public override void OnActivate()
    {
    }
}
