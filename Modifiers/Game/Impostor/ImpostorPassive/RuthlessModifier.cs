using MiraAPI.GameOptions;
using MiraAPI.Utilities.Assets;
using MiraAPI.Translation;
using DivaniMods.Assets;
using DivaniMods.Options;
using TownOfUs.Interfaces;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Game;
using TownOfUs.Modules.Wiki;
using TownOfUs.Utilities;
using UnityEngine;

namespace DivaniMods.Modifiers.Game.Impostor.ImpostorPassive;

public class RuthlessModifier : TouGameModifier, IWikiDiscoverable
{
    public static readonly Color RuthlessColor = Palette.ImpostorRoleHeaderRed;
    public override ModifierUiConfiguration Configuration => new(
        RuthlessColor,
        TmpSpriteUtils.CreateSpriteAsset(DivaniAssets.RuthlessIcon.LoadAsset(),
            "DivaniMod.Modifier.Impostor.Ruthless", 1.45f));
    public override string ModifierName => MiraLocaleManager.Get("DivaniMods.Modifier.Ruthless", "Ruthless");
    public override string IntroInfo => MiraLocaleManager.Get("DivaniMods.Modifier.Ruthless.IntroInfo");
    public override ModifierFaction FactionType => ModifierFaction.ImpostorPassive;
    public override Color FreeplayFileColor => RuthlessColor;
    public Color ModifierColor => RuthlessColor;

    public override LoadableAsset<Sprite>? ModifierIcon => DivaniAssets.RuthlessIcon;
    
    public override string GetDescription() => MiraLocaleManager.Get("DivaniMods.Modifier.Ruthless.Description");

    public string GetAdvancedDescription() => GetDescription() + MiscUtils.AppendOptionsText(GetType());
    
    public override int GetAssignmentChance() =>
        (int)OptionGroupSingleton<ImpostorModifierOptions>.Instance.RuthlessChance.Value;
    
    public override int GetAmountPerGame() =>
        (int)OptionGroupSingleton<ImpostorModifierOptions>.Instance.RuthlessAmount.Value;
    
    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return base.IsModifierValidOn(role) && role.TeamType == RoleTeamTypes.Impostor;
    }
    
    public override void OnActivate()
    {
    }
}
