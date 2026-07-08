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
using MiraAPI.Modifiers;

namespace DivaniMods.Modifiers.Game.Universal;

public class ArmoredModifier : UniversalGameModifier, IColoredModifier, IWikiDiscoverable
{
    public static readonly Color ArmoredColor = new Color32(0xdf, 0xce, 0x52, 0xff);

    public override string ModifierName => "Armored";
    public override string LocaleKey => "Armored";
    public override string IntroInfo => "You survive a number of attacks.";
    public override ModifierFaction FactionType => ModifierFaction.UniversalPassive;
    public override Color FreeplayFileColor => ArmoredColor;
    public Color ModifierColor => ArmoredColor;
    public override LoadableAsset<Sprite>? ModifierIcon => DivaniAssets.ArmoredIcon;

    public int MaxAttacks { get; private set; }
    public int AttacksRemaining { get; set; }
    public int AttacksSurvived => MaxAttacks - AttacksRemaining;
    public int DisplayedAttacksSurvived { get; set; }

    public void RefreshDisplayedAttacks() => DisplayedAttacksSurvived = AttacksSurvived;

    public override string GetDescription()
    {
        var max = MaxAttacks > 0 ? MaxAttacks : (int)OptionGroupSingleton<ArmoredOptions>.Instance.AttacksToSurvive.Value;
        return $"You survive {max} attack{(max == 1 ? "" : "s")}.\nSurvived attacks {DisplayedAttacksSurvived} / {max}";
    }

    public string GetAdvancedDescription() => GetDescription() + MiscUtils.AppendOptionsText(GetType());

    public override int GetAssignmentChance() =>
        (int)OptionGroupSingleton<UniversalModifierOptions>.Instance.ArmoredChance.Value;

    public override int GetAmountPerGame() =>
        (int)OptionGroupSingleton<UniversalModifierOptions>.Instance.ArmoredAmount.Value;

    public override void OnActivate()
    {
        MaxAttacks = (int)OptionGroupSingleton<ArmoredOptions>.Instance.AttacksToSurvive.Value;
        AttacksRemaining = MaxAttacks;

        if (AttacksRemaining > 0 && !Player.HasModifier<ArmoredShieldModifier>())
        {
            Player.AddModifier<ArmoredShieldModifier>();
        }
    }

    public override void OnDeath(DeathReason reason)
    {
        if (Player != null && Player.HasModifier<ArmoredShieldModifier>())
        {
            Player.RemoveModifier<ArmoredShieldModifier>();
        }

        ModifierComponent?.RemoveModifier(this);
    }
}
