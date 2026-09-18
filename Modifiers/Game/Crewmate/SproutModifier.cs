using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;
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

namespace DivaniMods.Modifiers.Game.Crewmate;

public class SproutModifier : TouGameModifier, IWikiDiscoverable, IButtonModifier
{
    public static readonly Color SproutColor = new Color32(124, 200, 90, 255);
    public override ModifierUiConfiguration Configuration => new(
        SproutColor,
        TmpSpriteUtils.CreateSpriteAsset(DivaniAssets.SproutIcon.LoadAsset(),
            "DivaniMod.Modifier.Crewmate.Sprout", 1.45f));

    public override string ModifierName => MiraLocaleManager.Get("DivaniMods.Modifier.Sprout", "Sprout");
    public override string IntroInfo => MiraLocaleManager.Get("DivaniMods.Modifier.Sprout.IntroInfo");
    public override ModifierFaction FactionType => ModifierFaction.CrewmateUtility;
    public override Color FreeplayFileColor => SproutColor;
    public Color ModifierColor => SproutColor;
    public override LoadableAsset<Sprite>? ModifierIcon => DivaniAssets.SproutIcon;

    public override string GetDescription() =>
        MiraLocaleManager.Get("DivaniMods.Modifier.Sprout.Description");

    public string GetAdvancedDescription() => GetDescription() + MiscUtils.AppendOptionsText(GetType());

    [HideFromIl2Cpp] public List<CustomButtonWikiDescription> Abilities { get; } =
    [
        new(
            MiraLocaleManager.Get("DivaniMods.Modifier.Sprout.Ability.Collect"),
            MiraLocaleManager.Get("DivaniMods.Modifier.Sprout.Ability.Collect.Description"),
            DivaniAssets.SproutCollectButton
        )
    ];

    public override int GetAssignmentChance() =>
        (int)OptionGroupSingleton<CrewmateModifierOptions>.Instance.SproutChance.Value;

    public override int GetAmountPerGame() =>
        (int)OptionGroupSingleton<CrewmateModifierOptions>.Instance.SproutAmount.Value;

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return role.IsCrewmate() && base.IsModifierValidOn(role) &&
            !role.Player.GetModifierComponent().HasModifier<GameModifier>(true, x => x is IButtonModifier);
    }

    public override void OnActivate()
    {
    }
}
