using MiraAPI.GameOptions;
using MiraAPI.Utilities.Assets;
using DivaniMods.Assets;
using DivaniMods.Options;
using TownOfUs.Interfaces;
using TownOfUs.Modifiers.Game;
using TownOfUs.Modules.Wiki;
using TownOfUs.Utilities;
using UnityEngine;

namespace DivaniMods.Modifiers.Game.Impostor.ImpostorPassive;

public class RuthlessModifier : TouGameModifier, IColoredModifier, IWikiDiscoverable
{
    public static readonly Color RuthlessColor = Palette.ImpostorRoleHeaderRed;
    public override string ModifierName => "Ruthless";
    public override string LocaleKey => "Ruthless";
    public override ModifierFaction FactionType => ModifierFaction.ImpostorPassive;
    public override Color FreeplayFileColor => RuthlessColor;
    public Color ModifierColor => RuthlessColor;

    public override LoadableAsset<Sprite>? ModifierIcon => DivaniAssets.RuthlessIcon;
    
    public override string GetDescription() => "Your kills bypass shields. Veteran alerts still kill you.";

    public string GetAdvancedDescription() => GetDescription() + MiscUtils.AppendOptionsText(GetType());
    
    public override int GetAssignmentChance() =>
        (int)OptionGroupSingleton<ImpostorModifierOptions>.Instance.RuthlessChance.Value;
    
    public override int GetAmountPerGame() =>
        (int)OptionGroupSingleton<ImpostorModifierOptions>.Instance.RuthlessAmount;
    
    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return role.TeamType == RoleTeamTypes.Impostor;
    }
    
    public override void OnActivate()
    {
    }
}
