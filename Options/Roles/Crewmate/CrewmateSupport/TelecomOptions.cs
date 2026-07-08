using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using DivaniMods.Roles.Crewmate.CrewmateSupport;

namespace DivaniMods.Options;

public enum TelecomTargetSelectionOptions
{
    MidRound,
    Meeting
}

public class TelecomOptions : AbstractOptionGroup<TelecomRole>
{
    public override string GroupName => "Telecom";

    public ModdedToggleOption Anonymous { get; } = new("Telecom Is Anonymous", true);

    public ModdedEnumOption TargetSelection { get; } = new(
        "Target Selection", (int)TelecomTargetSelectionOptions.MidRound, typeof(TelecomTargetSelectionOptions),
        ["Mid-Round", "Meeting"]);

    public ModdedNumberOption TransmissionDelay { get; } = new(
        "Transmission Delay", 3f, 0f, 10f, 1f, MiraNumberSuffixes.Seconds)
    {
        Visible = () => OptionGroupSingleton<TelecomOptions>.Instance.TargetSelection.Value ==
                        (int)TelecomTargetSelectionOptions.MidRound,
    };

    public ModdedNumberOption TransmissionCooldown { get; } = new(
        "Transmission Cooldown", 25f, 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds)
    {
        Visible = () => OptionGroupSingleton<TelecomOptions>.Instance.TargetSelection.Value ==
                        (int)TelecomTargetSelectionOptions.MidRound,
    };
}
