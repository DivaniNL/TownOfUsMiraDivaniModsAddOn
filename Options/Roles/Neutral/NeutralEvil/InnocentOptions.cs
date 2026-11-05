using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using DivaniMods.Roles.Neutral.NeutralEvil;

namespace DivaniMods.Options;

public class InnocentOptions : AbstractRoleOptionGroup<InnocentRole>
{
    public override string GroupName => MiraLocaleManager.Get("DivaniMods.Role.Innocent", "Innocent");
    public ModdedNumberOption TauntCooldown { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.Innocent.TauntCooldown"), 25f, 5f, 60f, 2.5f, MiraNumberSuffixes.Seconds);

    [ModdedToggleOption("DivaniMods.Options.Innocent.CanTauntFirstRound")]
    public bool CanTauntFirstRound { get; set; } = false;

    [ModdedToggleOption("DivaniMods.Options.Innocent.TauntedPlayerCanReportBody")]
    public bool TauntedPlayerCanReportBody { get; set; } = false;

    [ModdedToggleOption("DivaniMods.Options.Innocent.TauntBreaksShields")]
    public bool TauntBreaksShields { get; set; } = true;

    [ModdedEnumOption("DivaniMods.Options.Innocent.WinOutcome", typeof(NeutralEvilWinOutcome), 
        [
            "DivaniMods.Options.Innocent.WinOutcome.EndsGame",
            "DivaniMods.Options.Innocent.WinOutcome.KillOnePlayer",
            "DivaniMods.Options.Innocent.WinOutcome.Nothing"
        ]
    )]
    public NeutralEvilWinOutcome WinOutcome { get; set; } = NeutralEvilWinOutcome.EndsGame;
}
