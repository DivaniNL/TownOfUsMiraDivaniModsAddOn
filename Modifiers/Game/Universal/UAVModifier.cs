using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using DivaniMods.Assets;
using DivaniMods.Options;
using TownOfUs.Interfaces;
using TownOfUs.Modifiers.Game;
using TownOfUs.Modules.Wiki;
using TownOfUs.Utilities;
using UnityEngine;
using MiraAPI.Modifiers.Types;

namespace DivaniMods.Modifiers.Game.Universal;

public class UAVModifier : UniversalGameModifier, IColoredModifier, IWikiDiscoverable, IButtonModifier
{
    public static readonly Color UavColor = new Color32(179, 117, 117, 255);

    public override string ModifierName => "UAV";
    public override string LocaleKey => "UAV";
    public override string IntroInfo => "Call in a UAV to reveal everyone on the map!";
    public override ModifierFaction FactionType => ModifierFaction.UniversalUtility;
    public override Color FreeplayFileColor => UavColor;
    public Color ModifierColor => UavColor;
    public override LoadableAsset<Sprite>? ModifierIcon => DivaniAssets.UavIcon;

    public override string GetDescription() =>
        "Call in a UAV: while active, open the map to see everyone walking around.";

    public string GetAdvancedDescription() =>
        "Call in a UAV: while active, open the map to see everyone walking around." +
        MiscUtils.AppendOptionsText(GetType());

    [HideFromIl2Cpp] public List<CustomButtonWikiDescription> Abilities { get; } =
    [
        new("Call UAV", "Call in a UAV to see your shipmates' locations.", DivaniAssets.UavButton)
    ];

    public override int GetAssignmentChance() =>
        (int)OptionGroupSingleton<UniversalModifierOptions>.Instance.UavChance.Value;

    public override int GetAmountPerGame() =>
        (int)OptionGroupSingleton<UniversalModifierOptions>.Instance.UavAmount.Value;

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return base.IsModifierValidOn(role) &&
            !role.Player.GetModifierComponent().HasModifier<GameModifier>(true, x => x is IButtonModifier);
    }

    private int _usesRemaining = -1;

    public int UsesRemaining
    {
        get
        {
            if (_usesRemaining < 0)
            {
                _usesRemaining = (int)OptionGroupSingleton<UAVOptions>.Instance.UavUses.Value;
            }

            return _usesRemaining;
        }
        set => _usesRemaining = value;
    }

    public override void OnActivate()
    {
        _usesRemaining = (int)OptionGroupSingleton<UAVOptions>.Instance.UavUses.Value;
    }
}
