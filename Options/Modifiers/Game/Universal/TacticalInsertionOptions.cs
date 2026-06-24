using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using DivaniMods.Modifiers.Game.Universal;
using TownOfUs.Options;
using UnityEngine;

namespace DivaniMods.Options;

public class TacticalInsertionOptions : AbstractOptionGroup<TacticalInsertionModifier>
{
    public override Func<bool> GroupVisible => () => OptionGroupSingleton<RoleOptions>.Instance.IsClassicRoleAssignment;
    public override string GroupName => "Tactical Insertion";
    public override Color GroupColor => TacticalInsertionModifier.TacticalColor;
    public override uint GroupPriority => 36;

    public ModdedNumberOption Uses { get; } =
        new("Tactical Insertion Uses", 1f, 1f, 5f, 1f, MiraNumberSuffixes.None);

    public ModdedNumberOption Cooldown { get; } =
        new("Tactical Insertion Cooldown", 25f, 10f, 60f, 5f, MiraNumberSuffixes.Seconds);
}
