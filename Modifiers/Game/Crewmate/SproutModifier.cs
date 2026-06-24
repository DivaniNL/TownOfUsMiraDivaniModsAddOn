using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;
using MiraAPI.Utilities.Assets;
using DivaniMods.Assets;
using DivaniMods.Options;
using TownOfUs.Interfaces;
using TownOfUs.Modifiers.Game;
using TownOfUs.Modules.Wiki;
using TownOfUs.Utilities;
using UnityEngine;

namespace DivaniMods.Modifiers.Game.Crewmate;

public class SproutModifier : TouGameModifier, IColoredModifier, IWikiDiscoverable, IButtonModifier
{
    public static readonly Color SproutColor = new Color32(124, 200, 90, 255);

    public override string ModifierName => "Sprout";
    public override string LocaleKey => "Sprout";
    public override string IntroInfo => "Collect a modifier from a dead body.";
    public override ModifierFaction FactionType => ModifierFaction.CrewmateUtility;
    public override Color FreeplayFileColor => SproutColor;
    public Color ModifierColor => SproutColor;
    public override LoadableAsset<Sprite>? ModifierIcon => DivaniAssets.SproutIcon;

    public override string GetDescription() =>
        "Use Collect near a dead body to gain one random modifier that player had. One time use.";

    public string GetAdvancedDescription() => GetDescription() + MiscUtils.AppendOptionsText(GetType());

    [HideFromIl2Cpp] public List<CustomButtonWikiDescription> Abilities { get; } =
    [
        new("Collect", "Use near a dead body to gain one random modifier that player had.", DivaniAssets.SproutCollectButton)
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
