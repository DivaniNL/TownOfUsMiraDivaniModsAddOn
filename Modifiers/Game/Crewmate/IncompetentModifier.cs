using MiraAPI.GameOptions;
using MiraAPI.Utilities.Assets;
using DivaniMods.Assets;
using DivaniMods.Options;
using DivaniMods.Utilities;
using TownOfUs.Interfaces;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Game;
using TownOfUs.Modules.Wiki;
using TownOfUs.Utilities;
using UnityEngine;

namespace DivaniMods.Modifiers.Game.Crewmate;

public class IncompetentModifier : TouGameModifier, IColoredModifier, IWikiDiscoverable
{
    public static readonly Color IncompetentColor = new Color32(119, 90, 112, 255);

    public override string ModifierName => "Incompetent";
    public override string LocaleKey => "Incompetent";
    public override string IntroInfo => "You cannot fix sabotages.";
    public override ModifierFaction FactionType => ModifierFaction.CrewmatePassive;
    public override bool HideFromGuessing => true;
    public override Color FreeplayFileColor => IncompetentColor;
    public Color ModifierColor => IncompetentColor;
    public override LoadableAsset<Sprite>? ModifierIcon => DivaniAssets.IncompetentIcon;

    public override string GetDescription() =>
        "You are unable to fix sabotages. Sabotage consoles won't light up or work for you.";

    public string GetAdvancedDescription() => GetDescription() + MiscUtils.AppendOptionsText(GetType());

    public override int GetAssignmentChance() =>
        (int)OptionGroupSingleton<CrewmateModifierOptions>.Instance.IncompetentChance.Value;

    public override int GetAmountPerGame() =>
        (int)OptionGroupSingleton<CrewmateModifierOptions>.Instance.IncompetentAmount.Value;

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return role.IsCrewmate() && base.IsModifierValidOn(role) &&
            !ModifierExclusions.ConflictsWithOwned(role.Player, this);
    }

    public override void OnActivate()
    {
    }
}
