using MiraAPI.GameOptions;
using MiraAPI.Translation;
using MiraAPI.Utilities.Assets;
using DivaniMods.Assets;
using DivaniMods.Options;
using TownOfUs.Interfaces;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Game;
using TownOfUs.Modules.Wiki;
using TownOfUs.Utilities;
using UnityEngine;

namespace DivaniMods.Modifiers.Game.Universal;

public class FragileModifier : UniversalGameModifier, IWikiDiscoverable
{
    public static readonly Color FragileColor = new Color32(251, 252, 225, 255);
    public override ModifierUiConfiguration Configuration => new(
        FragileColor,
        TmpSpriteUtils.CreateSpriteAsset(DivaniAssets.FragileIcon.LoadAsset(),
            "DivaniMod.Modifier.Universal.Fragile", 1.45f));

    public override string ModifierName => MiraLocaleManager.Get("DivaniMods.Modifier.Fragile", "Fragile");
    public override string IntroInfo => MiraLocaleManager.Get("DivaniMods.Modifier.Fragile.IntroInfo");
    public override ModifierFaction FactionType => ModifierFaction.UniversalPassive;
    public override Color FreeplayFileColor => FragileColor;
    public Color ModifierColor => FragileColor;
    public override LoadableAsset<Sprite>? ModifierIcon => DivaniAssets.FragileIcon;
    
    public override string GetDescription()
    {
        var chance = OptionGroupSingleton<FragileOptions>.Instance.ChanceToBreak.Value;

        return MiraLocaleManager.Get("DivaniMods.Modifier.Fragile.Description")
            .Replace("<chance>", chance.ToString("0"));
    }

    public string GetAdvancedDescription() => GetDescription() + MiscUtils.AppendOptionsText(GetType());
    
    public override int GetAssignmentChance() =>
        (int)OptionGroupSingleton<UniversalModifierOptions>.Instance.FragileChance.Value;
    
    public override int GetAmountPerGame() =>
        (int)OptionGroupSingleton<UniversalModifierOptions>.Instance.FragileAmount.Value;

    public override void OnActivate()
    {
    }
}
