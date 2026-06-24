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

namespace DivaniMods.Modifiers.Game.Universal;

public class ShuffleModifier : UniversalGameModifier, IColoredModifier, IWikiDiscoverable, IButtonModifier
{
    public static readonly Color ShuffleColor = new Color32(0, 255, 30, 255);

    public override string ModifierName => "Shuffle";
    public override string LocaleKey => "Shuffle";
    public override string IntroInfo => "Shuffle all players' positions!";
    public override ModifierFaction FactionType => ModifierFaction.UniversalUtility;
    public override Color FreeplayFileColor => ShuffleColor;
    public Color ModifierColor => ShuffleColor;
    public override LoadableAsset<Sprite>? ModifierIcon => DivaniAssets.ShuffleIcon;
    
    private int _usesRemaining = -1;
    
    public int UsesRemaining
    {
        get
        {
            if (_usesRemaining < 0)
            {
                _usesRemaining = (int)OptionGroupSingleton<ShuffleOptions>.Instance.ShuffleUses.Value;
            }
            return _usesRemaining;
        }
        set => _usesRemaining = value;
    }
    
    public override string GetDescription() => $"Shuffle all players' positions! ({UsesRemaining} uses left)";

    public string GetAdvancedDescription() => "Shuffle all players' positions!" + MiscUtils.AppendOptionsText(GetType());

    [HideFromIl2Cpp] public List<CustomButtonWikiDescription> Abilities { get; } =
    [
        new("Shuffle", "Teleport every living player to a random other player's position.", DivaniAssets.ShuffleButton)
    ];
    
    public override int GetAssignmentChance() =>
        (int)OptionGroupSingleton<UniversalModifierOptions>.Instance.ShuffleChance.Value;
    
    public override int GetAmountPerGame() =>
        (int)OptionGroupSingleton<UniversalModifierOptions>.Instance.ShuffleAmount.Value;
    
    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return base.IsModifierValidOn(role) &&
            !role.Player.GetModifierComponent().HasModifier<GameModifier>(true, x => x is IButtonModifier);
    }
    
    public override void OnActivate()
    {
        _usesRemaining = (int)OptionGroupSingleton<ShuffleOptions>.Instance.ShuffleUses.Value;
    }
}
