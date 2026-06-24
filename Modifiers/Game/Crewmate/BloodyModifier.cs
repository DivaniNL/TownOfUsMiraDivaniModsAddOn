using Il2CppInterop.Runtime.Attributes;
using AmongUs.GameOptions;
using MiraAPI.GameOptions;
using MiraAPI.Roles;
using MiraAPI.Utilities.Assets;
using DivaniMods.Assets;
using DivaniMods.Options;
using TownOfUs.Interfaces;
using TownOfUs.Modifiers.Game;
using TownOfUs.Modules.Wiki;
using TownOfUs.Utilities;
using UnityEngine;

namespace DivaniMods.Modifiers.Game.Crewmate;

public sealed class BloodyModifier : TouGameModifier, IColoredModifier, IWikiDiscoverable
{
    public static readonly Color ModifierUiColor = Palette.ImpostorRed;

    public override string ModifierName => "Bloody";
    public override string LocaleKey => "Bloody";
    public override string IntroInfo => "Your killer leaves red footprints upon death.";
    public override ModifierFaction FactionType => ModifierFaction.CrewmatePostmortem;
    public override Color FreeplayFileColor => ModifierUiColor;
    public Color ModifierColor => ModifierUiColor;
    public override LoadableAsset<Sprite>? ModifierIcon => DivaniAssets.BloodyIcon;

    public override string GetDescription()
    {
        return "When you are killed, your killer leaves red footprints for a short period of time.";
    }

    public string GetAdvancedDescription() => GetDescription() + MiscUtils.AppendOptionsText(GetType());

    public override int GetAssignmentChance() =>
        (int)OptionGroupSingleton<CrewmateModifierOptions>.Instance.BloodyChance.Value;

    public override int GetAmountPerGame() =>
        (int)OptionGroupSingleton<CrewmateModifierOptions>.Instance.BloodyAmount.Value;

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return base.IsModifierValidOn(role) && role.IsCrewmate();
    }

    [HideFromIl2Cpp] public List<CustomButtonWikiDescription> Abilities { get; } = [];
}
