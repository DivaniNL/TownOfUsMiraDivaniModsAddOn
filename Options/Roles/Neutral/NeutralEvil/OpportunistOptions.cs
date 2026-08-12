using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using DivaniMods.Roles.Neutral.NeutralEvil;

namespace DivaniMods.Options;

public class OpportunistOptions : AbstractRoleOptionGroup<OpportunistRole>
{
    public override string GroupName => "Opportunist";

    public ModdedNumberOption VotesNeeded { get; } = new(
        "Required Number of Votes", 15f, 2f, 20f, 1f, MiraNumberSuffixes.None);

    public ModdedNumberOption MaxVotesPerMeeting { get; } = new(
        "Max Votes Collected Per Meeting", 5f, 1f, 10f, 1f, MiraNumberSuffixes.None);

    public ModdedToggleOption CanUseWildcard { get; } = new("Opportunist can use Wildcard", false);
}
