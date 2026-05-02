using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using DivaniMods.Modifiers;
using TownOfUs.Options;
using UnityEngine;

namespace DivaniMods.Options;

public class ShuffleOptions : AbstractOptionGroup<ShuffleModifier>
{
    public override Func<bool> GroupVisible => () => OptionGroupSingleton<RoleOptions>.Instance.IsClassicRoleAssignment;
    public override string GroupName => "Shuffle";
    public override Color GroupColor => ShuffleModifier.ShuffleColor;
    public override uint GroupPriority => 34;

    public ModdedNumberOption ShuffleUses { get; } =
        new("Shuffle Uses", 1f, 0, 5f, 1f, MiraNumberSuffixes.None);

    public ModdedNumberOption ShuffleCooldown { get; } =
        new("Shuffle Cooldown", 30f, 10f, 60f, 5f, MiraNumberSuffixes.Seconds);
    
    public ModdedToggleOption ShuffleDeadBodiesOption { get; } =
        new("Shuffle Dead Bodies", false);
    
    public bool ShuffleDeadBodies => ShuffleDeadBodiesOption.Value;
}
