using MiraAPI.GameOptions;
using MiraAPI.Utilities.Assets;
using DivaniMods.Assets;
using DivaniMods.Options;
using TownOfUs.Interfaces;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Game;
using TownOfUs.Modules.Wiki;
using TownOfUs.Utilities;
using UnityEngine;

namespace DivaniMods.Modifiers.Game.Impostor.ImpostorPassive;

public class NullifiedModifier : TouGameModifier, IColoredModifier, IWikiDiscoverable
{
    public static readonly Color NullifiedColor = Palette.ImpostorRoleHeaderRed;
    public override string ModifierName => "Nullified";
    public override string LocaleKey => "Nullified";
    public override string IntroInfo => "You are immune to kill debuffs.";
    public override ModifierFaction FactionType => ModifierFaction.ImpostorPassive;
    public override Color FreeplayFileColor => NullifiedColor;
    public Color ModifierColor => NullifiedColor;

    public override LoadableAsset<Sprite>? ModifierIcon => DivaniAssets.NullifiedIcon;

    public override string GetDescription()
    {
        var desc = "You are immune to kill debuffs (Bait, Frosty, Diseased, Bloody, Aftermath, Noisemaker, Bear Trap).";
        if (OptionGroupSingleton<NullifiedOptions>.Instance.SilencesCelebrity)
        {
            desc += " Killing a Celebrity does not announce their death.";
        }

        return desc;
    }

    public string GetAdvancedDescription() => GetDescription() + MiscUtils.AppendOptionsText(GetType());

    public override int GetAssignmentChance() =>
        (int)OptionGroupSingleton<ImpostorModifierOptions>.Instance.NullifiedChance.Value;

    public override int GetAmountPerGame() =>
        (int)OptionGroupSingleton<ImpostorModifierOptions>.Instance.NullifiedAmount.Value;

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return role.TeamType == RoleTeamTypes.Impostor;
    }

    public override void OnActivate()
    {
    }
}
