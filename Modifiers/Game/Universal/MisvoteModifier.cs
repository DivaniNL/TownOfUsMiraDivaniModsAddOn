using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using DivaniMods.Assets;
using DivaniMods.Options;
using DivaniMods.Roles.Impostor.ImpostorSupport;
using TownOfUs.Interfaces;
using TownOfUs.Modifiers.Game;
using TownOfUs.Modules.Wiki;
using TownOfUs.Utilities;
using UnityEngine;

namespace DivaniMods.Modifiers.Game.Universal;

public sealed class MisvoteModifier : UniversalGameModifier, IColoredModifier, IWikiDiscoverable
{
    public static readonly Color MisvoteColor = new Color32(180, 180, 180, 255);

    public override string ModifierName => "Misvote";
    public override string LocaleKey => "Misvote";
    public override string IntroInfo => "Your vote is random every meeting.";
    public override ModifierFaction FactionType => ModifierFaction.UniversalPassive;
    public override Color FreeplayFileColor => MisvoteColor;
    public Color ModifierColor => MisvoteColor;
    public override LoadableAsset<Sprite>? ModifierIcon => DivaniAssets.MisvoteIcon;

    public override string GetDescription() =>
        "Your vote is random every meeting. You vote normally, but the vote(s) - " +
        "even a Skip - are transferred to a random players each meeting.";

    public string GetAdvancedDescription() => GetDescription() + MiscUtils.AppendOptionsText(GetType());

    public override int GetAssignmentChance() =>
        (int)OptionGroupSingleton<UniversalModifierOptions>.Instance.MisvoteChance.Value;

    public override int GetAmountPerGame() =>
        (int)OptionGroupSingleton<UniversalModifierOptions>.Instance.MisvoteAmount.Value;

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return base.IsModifierValidOn(role) && role is not CouncillorRole;
    }
}
