using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using DivaniMods.Roles;

namespace DivaniMods.Options;

public class OpportunistOptions : AbstractOptionGroup<OpportunistRole>
{
    public override string GroupName => "Opportunist";

    [ModdedNumberOption("Required Number of Votes", 2f, 20f, 1f)]
    public float VotesNeeded { get; set; } = 10f;
}
