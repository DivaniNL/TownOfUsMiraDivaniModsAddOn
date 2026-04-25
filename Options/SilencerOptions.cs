using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using DivaniMods.Roles;

namespace DivaniMods.Options;

public class SilencerOptions : AbstractOptionGroup<SilencerRole>
{
    public override string GroupName => "Silencer";

    [ModdedNumberOption("Seconds Cut Per Kill", 10, 40, 5, MiraNumberSuffixes.Seconds)]
    public float SecondsPerKill { get; set; } = 15;

    [ModdedNumberOption("Minimum Voting Time", 5, 25, 5, MiraNumberSuffixes.Seconds)]
    public float MinimumVotingTime { get; set; } = 10;
}
