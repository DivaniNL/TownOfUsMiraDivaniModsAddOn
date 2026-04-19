using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using DivaniMods.Assets;
using DivaniMods.Options;
using TownOfUs.Modifiers.Game;
using TownOfUs.Modules.Wiki;
using TownOfUs.Utilities;
using UnityEngine;

namespace DivaniMods.Modifiers;

/// <summary>
/// Universal modifier: the player votes normally but their vote - including
/// Skip - is silently redirected to a random alive player every meeting.
/// The re-roll runs in <see cref="DivaniMods.Patches.MisvoteVotePatches"/>.
/// </summary>
public sealed class MisvoteModifier : UniversalGameModifier, IWikiDiscoverable
{
    public override string ModifierName => "Misvote";
    public override string LocaleKey => "Misvote";
    public override ModifierFaction FactionType => ModifierFaction.UniversalPassive;
    public override Color FreeplayFileColor => new Color32(180, 180, 180, 255);
    public override LoadableAsset<Sprite>? ModifierIcon => DivaniAssets.MisvoteIcon;

    public override string GetDescription() =>
        "Your vote is random every meeting. You vote normally, but the vote - " +
        "even a Skip - is transferred to a random alive player.";

    public string GetAdvancedDescription() => GetDescription() + MiscUtils.AppendOptionsText(GetType());

    public override int GetAssignmentChance() =>
        (int)OptionGroupSingleton<MisvoteOptions>.Instance.MisvoteChance.Value;

    public override int GetAmountPerGame() =>
        (int)OptionGroupSingleton<MisvoteOptions>.Instance.MisvoteAmount;
}
