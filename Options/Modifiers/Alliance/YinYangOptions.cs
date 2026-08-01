using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using DivaniMods.Modifiers.Game.Alliance;
using TownOfUs.Options;
using UnityEngine;

namespace DivaniMods.Options;

public enum LosingYinYangerBehavior
{
    Nothing,
    LeavesInShame,
}

public sealed class YinYangOptions : AbstractTouModifierOptionGroup<YinYangModifier>
{
    public override string GroupName => "Yin-Yang";
    public override Func<bool> GroupVisible => () => OptionGroupSingleton<RoleOptions>.Instance.IsClassicRoleAssignment;
    public override Color GroupColor => YinYangModifier.YinColor;
    public override uint GroupPriority => 14;

    public ModdedNumberOption MarkCooldown { get; } =
        new("Mark Cooldown", 25f, 5f, 50f, 2.5f, MiraNumberSuffixes.Seconds);

    public ModdedEnumOption LosingYinYanger { get; } =
        new("Losing Yin-Yanger", (int)LosingYinYangerBehavior.Nothing, typeof(LosingYinYangerBehavior),
            ["Nothing", "Leaves In Shame"]);

    public ModdedToggleOption RemoveIfAllied { get; } = new("Remove Yin-Yang If Allied", true);
}
