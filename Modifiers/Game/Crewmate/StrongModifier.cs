using MiraAPI.GameOptions;
using MiraAPI.Utilities.Assets;
using DivaniMods.Assets;
using DivaniMods.Options;
using TownOfUs.Interfaces;
using TownOfUs.Modifiers.Game;
using TownOfUs.Modules.Wiki;
using TownOfUs.Utilities;
using UnityEngine;

namespace DivaniMods.Modifiers.Game.Crewmate;

public class StrongModifier : TouGameModifier, IColoredModifier, IWikiDiscoverable
{
    public static readonly Color StrongColor = new Color32(150, 120, 90, 255);

    public override string ModifierName => "Strong";
    public override string LocaleKey => "Strong";
    public override string IntroInfo => "You cannot be guessed in meetings.";
    public override ModifierFaction FactionType => ModifierFaction.CrewmatePassive;
    public override Color FreeplayFileColor => StrongColor;
    public Color ModifierColor => StrongColor;
    public override LoadableAsset<Sprite>? ModifierIcon => DivaniAssets.SkilledIcon;

    public override string GetDescription() =>
        "You cannot be guessed in meetings.";

    public string GetAdvancedDescription() => GetDescription() + MiscUtils.AppendOptionsText(GetType());

    public override int GetAssignmentChance() =>
        (int)OptionGroupSingleton<CrewmateModifierOptions>.Instance.StrongChance.Value;

    public override int GetAmountPerGame() =>
        (int)OptionGroupSingleton<CrewmateModifierOptions>.Instance.StrongAmount.Value;

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return role.IsCrewmate() && base.IsModifierValidOn(role);
    }

    public override void OnActivate()
    {
    }
}
