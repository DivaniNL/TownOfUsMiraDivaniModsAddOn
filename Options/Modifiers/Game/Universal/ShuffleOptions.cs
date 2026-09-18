using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using DivaniMods.Modifiers.Game.Universal;
using TownOfUs.Options;
using UnityEngine;

namespace DivaniMods.Options;

public class ShuffleOptions : AbstractTouModifierOptionGroup<ShuffleModifier>
{
    public override Func<bool> GroupVisible => () => RoleOptions.IsClassicRoleAssignment;
    public override string GroupName => MiraLocaleManager.Get("DivaniMods.Modifier.Shuffle", "Shuffle");
    public override Color GroupColor => ShuffleModifier.ShuffleColor;
    public override uint GroupPriority => 37;

    public ModdedNumberOption ShuffleUses { get; } =
        new(MiraLocaleManager.Get("DivaniMods.Options.Shuffle.ShuffleUses"), 1f, 0, 5f, 1f, MiraNumberSuffixes.None);

    public ModdedNumberOption ShuffleCooldown { get; } =
        new(MiraLocaleManager.Get("DivaniMods.Options.Shuffle.ShuffleCooldown"), 30f, 10f, 60f, 5f, MiraNumberSuffixes.Seconds);
    
    public ModdedToggleOption ShuffleCorpsesOption { get; } =
        new(MiraLocaleManager.Get("DivaniMods.Options.Shuffle.ShuffleDeadBodies"), true);

    public bool ShuffleCorpses => ShuffleCorpsesOption.Value;
}
