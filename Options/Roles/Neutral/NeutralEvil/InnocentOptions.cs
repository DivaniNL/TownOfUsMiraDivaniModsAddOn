using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using DivaniMods.Roles.Neutral.NeutralEvil;

namespace DivaniMods.Options;

public class InnocentOptions : AbstractOptionGroup<InnocentRole>
{
    public override string GroupName => "Innocent";

    public ModdedNumberOption TauntCooldown { get; } = new(
        "Taunt Cooldown", 25f, 5f, 60f, 2.5f, MiraNumberSuffixes.Seconds);

    [ModdedToggleOption("Can Taunt in First Round")]
    public bool CanTauntFirstRound { get; set; } = false;

    [ModdedToggleOption("Taunted Player Can Report Body")]
    public bool TauntedPlayerCanReportBody { get; set; } = false;
}
