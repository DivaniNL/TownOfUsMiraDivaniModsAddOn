using MiraAPI.GameOptions;
using MiraAPI.Utilities.Assets;
using DivaniMods.Options;
using TownOfUs.Assets;
using TownOfUs.Interfaces;
using TownOfUs.Modifiers.Game;
using TownOfUs.Modules.Wiki;
using TownOfUs.Utilities;
using UnityEngine;

namespace DivaniMods.Modifiers;

public sealed class BearTrapModifier : TouGameModifier, IColoredModifier, IWikiDiscoverable
{
    public static readonly Color BearTrapColor = new Color32(210, 125, 45, 255);

    public override string ModifierName => "Bear Trap";
    public override string LocaleKey => "BearTrap";
    public override ModifierFaction FactionType => ModifierFaction.CrewmatePostmortem;
    public override Color FreeplayFileColor => BearTrapColor;
    public Color ModifierColor => BearTrapColor;
    public override LoadableAsset<Sprite>? ModifierIcon => TouCrewAssets.TrapSprite;

    public override string GetDescription()
    {
        var duration = OptionGroupSingleton<BearTrapOptions>.Instance.FreezeDuration.Value;
        return $"When you are killed, your killer is frozen for {duration:0} seconds and cannot report your body.";
    }

    public string GetAdvancedDescription() => GetDescription() + MiscUtils.AppendOptionsText(GetType());

    public override int GetAssignmentChance() =>
        (int)OptionGroupSingleton<CrewmateModifierOptions>.Instance.BearTrapChance.Value;

    public override int GetAmountPerGame() =>
        (int)OptionGroupSingleton<CrewmateModifierOptions>.Instance.BearTrapAmount;

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return base.IsModifierValidOn(role) && role.TeamType == RoleTeamTypes.Crewmate;
    }

    public override void OnActivate()
    {
        DivaniPlugin.Instance.Log.LogInfo("Bear Trap modifier activated!");
    }
}