using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using DivaniMods.Roles;

namespace DivaniMods.Options;

public class InnocentOptions : AbstractOptionGroup<InnocentRole>
{
    public override string GroupName => "Innocent";

    [ModdedNumberOption("Taunt Cooldown", 5f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float TauntCooldown { get; set; } = 25f;

    [ModdedToggleOption("Can Taunt in First Round")]
    public bool CanTauntFirstRound { get; set; } = false;
}
