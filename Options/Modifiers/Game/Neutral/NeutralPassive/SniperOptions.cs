using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using DivaniMods.Modifiers.Game.Neutral.NeutralPassive;
using TownOfUs.Options;
using UnityEngine;

namespace DivaniMods.Options;

public class SniperOptions : AbstractTouModifierOptionGroup<SniperModifier>
{
    public override Func<bool> GroupVisible => () => RoleOptions.IsClassicRoleAssignment;
    public override string GroupName => MiraLocaleManager.Get("DivaniMods.Modifier.Sniper", "Sniper");
    public override Color GroupColor => SniperModifier.SniperColor;
    public override uint GroupPriority => 50;

    public ModdedNumberOption KillDistanceMultiplier { get; } =
        new(MiraLocaleManager.Get("DivaniMods.Options.Sniper.KillDistanceMultiplier"), 1.5f, 1.1f, 2.0f, 0.1f, MiraNumberSuffixes.Multiplier, "0.0");
}
