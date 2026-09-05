using System.Linq;
using MiraAPI.GameEnd;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using DivaniMods.Assets;
using DivaniMods.GameOver;
using DivaniMods.Options;
using TownOfUs.Interfaces;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Game;
using TownOfUs.Modules.Components;
using TownOfUs.Modules.Wiki;
using TownOfUs.Modules.Localization;
using MiraAPI.Translation;
using TownOfUs.Roles.Impostor;
using TownOfUs.Utilities;
using UnityEngine;

namespace DivaniMods.Modifiers.Game.Alliance;

public sealed class BetrayerModifier : AllianceGameModifier, IWikiDiscoverable, IContinuesGame
{
    public static readonly Color BetrayerColor = new Color32(186, 113, 255, 255);
    public override ModifierUiConfiguration Configuration => new(
        BetrayerColor,
        TmpSpriteUtils.CreateSpriteAsset(DivaniAssets.BetrayerIcon.LoadAsset(),
            "DivaniMod.Modifier.Alliance.Betrayer", 1.45f));

    public override string ModifierName => "Betrayer";
    public override string IntroInfo => "Ditch the knife for a wand";
    public override string Symbol => "⁉";
    public string ShortName => "Bet";
    public override bool DoesTasks => false;
    public override bool GetsPunished => false;
    public override ModifierFaction FactionType => ModifierFaction.ImpostorAlliance;
    public override AlliedFaction TrueFactionType => AlliedFaction.NeutralKiller;
    public override Color FreeplayFileColor => BetrayerColor;
    public Color ModifierColor => BetrayerColor;
    public override LoadableAsset<Sprite>? ModifierIcon => DivaniAssets.BetrayerIcon;

    public bool ContinuesGame => true;

    public List<CustomButtonWikiDescription> Abilities { get; } = [];

    public override string GetDescription() =>
        "You no longer win with the Impostors.\n" +
        "Outlive them and every other killer,\nthen finish off the rest to win alone.";

    public string GetAdvancedDescription() =>
        "The Betrayer is an Impostor that has turned on their own team. " +
        "They win like a Neutral Killer: they must be the last killer alive and reach kill majority. " +
        "The Impostors cannot win by majority while a Betrayer lives, and the Betrayer can kill fellow Impostors."
        + MiscUtils.AppendOptionsText(GetType());

    public override int GetAssignmentChance() =>
        (int)OptionGroupSingleton<AllianceModifierOptions>.Instance.BetrayerChance.Value;

    public override int GetAmountPerGame() =>
        (int)OptionGroupSingleton<AllianceModifierOptions>.Instance.BetrayerAmount.Value;

    public override int Priority() => 5;

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return base.IsModifierValidOn(role) && role.TeamType == RoleTeamTypes.Impostor;
    }

    public static bool WinConditionMet()
    {
        var betrayers = ModifierUtils.GetActiveModifiers<BetrayerModifier>()
            .Where(x => x.Player != null && !x.Player.HasDied())
            .ToList();

        if (betrayers.Count == 0)
        {
            return false;
        }

        var otherKillers = MiscUtils.KillersAliveCount - betrayers.Count;
        if (otherKillers > 0)
        {
            return false;
        }

        return betrayers.Count >= Helpers.GetAlivePlayers().Count - betrayers.Count;
    }

    public static PlayerControl? GetHexBombBetrayer()
    {
        var spellslinger = CustomRoleUtils.GetActiveRolesOfType<SpellslingerRole>().FirstOrDefault();

        if (spellslinger?.Player == null || spellslinger.Player.HasDied() ||
            !spellslinger.Player.HasModifier<BetrayerModifier>())
        {
            return null;
        }

        return spellslinger.Player;
    }

    public static bool HexBombWinConditionMet()
    {
        var sabotage = HexBombSabotageSystem.Instance;

        if (sabotage == null || sabotage.Stage != HexBombStage.Finished ||
            !SpellslingerRole.EveryoneHexed())
        {
            return false;
        }

        return GetHexBombBetrayer() != null;
    }

    public override bool? DidWin(GameOverReason reason)
    {
        if (reason == CustomGameOver.GameOverReason<BetrayerGameOver>())
        {
            return !Player.HasDied();
        }

        return false;
    }
}
