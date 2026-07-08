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

public class SkilledModifier : TouGameModifier, IColoredModifier, IWikiDiscoverable
{
    public static readonly Color SkilledColor = new Color32(78, 94, 186, 255); // #4E5EBA

    public override string ModifierName => "Skilled";
    public override string LocaleKey => "Skilled";
    public override string IntroInfo => "You can fix two-part sabotages alone.";
    public override ModifierFaction FactionType => ModifierFaction.CrewmateUtility;
    public override Color FreeplayFileColor => SkilledColor;
    public Color ModifierColor => SkilledColor;
    public override LoadableAsset<Sprite>? ModifierIcon => DivaniAssets.SkilledIcon;

    public override string GetDescription() =>
        "You can fix two-part sabotages on your own!";

    public string GetAdvancedDescription() => GetDescription() + MiscUtils.AppendOptionsText(GetType());

    public override int GetAssignmentChance() =>
        (int)OptionGroupSingleton<CrewmateModifierOptions>.Instance.SkilledChance.Value;

    public override int GetAmountPerGame() =>
        (int)OptionGroupSingleton<CrewmateModifierOptions>.Instance.SkilledAmount.Value;

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return role.IsCrewmate() && base.IsModifierValidOn(role) &&
            !ModifierExclusions.ConflictsWithOwned(role.Player, this);
    }

    public override void OnActivate()
    {
    }
}
