using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using MiraAPI.Translation;
using DivaniMods.Roles.Neutral.NeutralKilling;
using UnityEngine;
using MiraAPI.GameOptions.OptionTypes;

namespace DivaniMods.Options;

public sealed class MonsterOptions : AbstractOptionGroup<MonsterRole>
{
    public override string GroupName => MiraLocaleManager.Get("DivaniMods.Role.Monster", "Monster");
    public override Color GroupColor => MonsterRole.MonsterColor;
    
    public ModdedToggleOption CanVent { get; } = new(MiraLocaleManager.Get("DivaniMods.Options.Monster.CanVent"), false);

    [ModdedToggleOption("DivaniMods.Options.Monster.DevourAnimVisibleToEveryone")]
    public bool DevourAnimVisibleToEveryone { get; set; } = false;

    [ModdedNumberOption("DivaniMods.Options.Monster.DevourCooldown", 5f, 120f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float DevourCooldown { get; set; } = 20f;

    [ModdedNumberOption("DivaniMods.Options.Monster.DevourCooldownIncreasePerDevour", 0f, 30f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float DevourCooldownIncreasePerDevour { get; set; } = 5f;

    // 0 = Infinite
    [ModdedNumberOption("DivaniMods.Options.Monster.MaxDevouredPerRound", 0f, 15f, 1f, MiraNumberSuffixes.None, "0", true)]
    public float MaxDevouredPerRound { get; set; } = 0f;
}