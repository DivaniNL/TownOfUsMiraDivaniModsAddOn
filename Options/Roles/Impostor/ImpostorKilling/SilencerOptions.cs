using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using DivaniMods.Roles.Impostor.ImpostorKilling;

namespace DivaniMods.Options;

public class SilencerOptions : AbstractRoleOptionGroup<SilencerRole>
{
    public override string GroupName => MiraLocaleManager.Get("DivaniMods.Role.Silencer", "Silencer");

    public ModdedNumberOption SecondsPerKill { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.Silencer.SecondsPerKill"), 25f, 10f, 40f, 5f, MiraNumberSuffixes.Seconds);

    public ModdedNumberOption MinimumVotingTime { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.Silencer.MinimumVotingTime"), 10f, 5f, 25f, 5f, MiraNumberSuffixes.Seconds);

    [ModdedToggleOption("DivaniMods.Options.Silencer.NormalVotingTimeWhenDead")]
    public bool NormalVotingTimeWhenDead { get; set; } = true;
}
