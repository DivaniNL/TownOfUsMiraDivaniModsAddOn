using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using DivaniMods.Roles.Impostor.ImpostorKilling;

namespace DivaniMods.Options;

public class SilencerOptions : AbstractRoleOptionGroup<SilencerRole>
{
    public override string GroupName => "Silencer";

    public ModdedNumberOption SecondsPerKill { get; } = new(
        "Seconds Cut Per Kill", 25f, 10f, 40f, 5f, MiraNumberSuffixes.Seconds);

    public ModdedNumberOption MinimumVotingTime { get; } = new(
        "Minimum Voting Time", 10f, 5f, 25f, 5f, MiraNumberSuffixes.Seconds);

    [ModdedToggleOption("Normal Voting Time When Dead")]
    public bool NormalVotingTimeWhenDead { get; set; } = true;
}
