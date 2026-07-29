using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using TownOfUs.Modules.Localization;
using DivaniMods.Roles.Neutral.NeutralKilling;
using UnityEngine;
using MiraAPI.GameOptions.OptionTypes;

namespace DivaniMods.Options;

public sealed class MonsterOptions : AbstractOptionGroup<MonsterRole>
{
    public override string GroupName => TouLocale.Get("MonsterRole", "Monster");
    public override Color GroupColor => MonsterRole.MonsterColor;
    
    public ModdedToggleOption CanVent { get; } = new("Monster Can Vent", false);

    [ModdedToggleOption("Devour Animation Visible To Everyone")]
    public bool DevourAnimVisibleToEveryone { get; set; } = false;

    [ModdedNumberOption("Devour Cooldown", 5f, 120f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float DevourCooldown { get; set; } = 20f;

    [ModdedNumberOption("Devour Cooldown Increase Per Victim", 0f, 30f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float DevourCooldownIncreasePerDevour { get; set; } = 5f;

    // 0 = Infinite
    [ModdedNumberOption("Max Num of Players in Stomach", 0f, 15f, 1f, MiraNumberSuffixes.None, "0", true)]
    public float MaxDevouredPerRound { get; set; } = 0f;
}