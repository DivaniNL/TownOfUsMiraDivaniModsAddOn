using System.Collections.Generic;
using AmongUs.GameOptions;
using MiraAPI.GameOptions;
using MiraAPI.Utilities.Assets;
using DivaniMods.Assets;
using DivaniMods.Options;
using DivaniMods.Roles.Crewmate.CrewmateKilling;
using TownOfUs.Interfaces;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Game;
using TownOfUs.Modules.Wiki;
using TownOfUs.Utilities;
using UnityEngine;

namespace DivaniMods.Modifiers.Game.Universal;

public class MementoModifier : UniversalGameModifier, IColoredModifier, IWikiDiscoverable
{
    public static readonly Color MementoColor = new Color32(0x61, 0x78, 0xED, 255);

    public static readonly Dictionary<byte, RoleTypes> RoleBeforeDeath = new();

    public override string ModifierName => "Memento";
    public override string LocaleKey => "Memento";
    public override string IntroInfo => "Your role is revealed to everyone in meetings upon death.";
    public override ModifierFaction FactionType => ModifierFaction.UniversalPostmortem;
    public override Color FreeplayFileColor => MementoColor;
    public Color ModifierColor => MementoColor;
    public override LoadableAsset<Sprite>? ModifierIcon => DivaniAssets.MementoIcon;

    public override string GetDescription()
    {
        return "When you die, your role is revealed to everyone in meetings for the rest of the game.";
    }

    public string GetAdvancedDescription() => GetDescription() + MiscUtils.AppendOptionsText(GetType());

    public override int GetAssignmentChance() =>
        (int)OptionGroupSingleton<UniversalModifierOptions>.Instance.MementoChance.Value;

    public override int GetAmountPerGame() =>
        (int)OptionGroupSingleton<UniversalModifierOptions>.Instance.MementoAmount.Value;

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return base.IsModifierValidOn(role) && role is not RetributionistRole;
    }

    public override void OnActivate()
    {
    }

    public override void FixedUpdate()
    {
        if (Player != null && Player.Data != null && !Player.Data.IsDead && Player.Data.Role != null)
        {
            RoleBeforeDeath[Player.PlayerId] = Player.Data.Role.Role;
        }
    }

    public static RoleBehaviour? ResolveRoleBeforeDeath(byte playerId)
    {
        if (!RoleBeforeDeath.TryGetValue(playerId, out var roleType))
        {
            return null;
        }

        if (RoleManager.Instance == null)
        {
            return null;
        }

        return RoleManager.Instance.GetRole(roleType);
    }
}
