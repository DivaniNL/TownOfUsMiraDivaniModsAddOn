using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using DivaniMods.Roles.Neutral.NeutralOutlier;

namespace DivaniMods.Options;

public class OpportunistOptions : AbstractOptionGroup<OpportunistRole>
{
    public override string GroupName => "Opportunist";

    public ModdedNumberOption VotesNeeded { get; } = new(
        "Required Number of Votes", 15f, 2f, 20f, 1f, MiraNumberSuffixes.None);

    public ModdedToggleOption CanUseWildcard { get; } = new("Opportunist can use Wildcard", true);
}
