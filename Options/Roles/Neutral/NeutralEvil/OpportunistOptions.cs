using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using DivaniMods.Roles.Neutral.NeutralEvil;
using MiraAPI.GameOptions.Attributes;

namespace DivaniMods.Options;

public class OpportunistOptions : AbstractRoleOptionGroup<OpportunistRole>
{
    public override string GroupName => MiraLocaleManager.Get("DivaniMods.Role.Opportunist", "Opportunist");

    public ModdedNumberOption VotesNeeded { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.Opportunist.VotesNeeded"), 15f, 2f, 20f, 1f, MiraNumberSuffixes.None);

    public ModdedNumberOption MaxVotesPerMeeting { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.Opportunist.MaxVotesPerMeeting"), 5f, 1f, 10f, 1f, MiraNumberSuffixes.None);

    public ModdedToggleOption CanUseWildcard { get; } = new(MiraLocaleManager.Get("DivaniMods.Options.Opportunist.CanUseWildcard"), false);

    [ModdedEnumOption("DivaniMods.Options.Opportunist.WinOutcome", typeof(NeutralEvilWinOutcome), 
        [
            "DivaniMods.Options.Opportunist.WinOutcome.EndsGame",
            "DivaniMods.Options.Opportunist.WinOutcome.KillOnePlayer",
            "DivaniMods.Options.Opportunist.WinOutcome.Nothing"
        ]
    )]
    public NeutralEvilWinOutcome WinOutcome { get; set; } = NeutralEvilWinOutcome.EndsGame;
}
