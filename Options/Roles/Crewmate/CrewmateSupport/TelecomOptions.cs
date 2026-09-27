using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using DivaniMods.Roles.Crewmate.CrewmateSupport;

namespace DivaniMods.Options;

public enum TelecomTargetSelectionOptions
{
    MidRound,
    Meeting
}

public class TelecomOptions : AbstractRoleOptionGroup<TelecomRole>
{
    public override string GroupName => MiraLocaleManager.Get("DivaniMods.Role.Telecom", "Telecom");

    public ModdedToggleOption Anonymous { get; } = new(MiraLocaleManager.Get("DivaniMods.Options.Telecom.Anonymous"), true);

    public ModdedEnumOption TargetSelection { get; } = new(
        "Target Selection", (int)TelecomTargetSelectionOptions.MidRound, typeof(TelecomTargetSelectionOptions),
        [
            MiraLocaleManager.Get("DivaniMods.Options.Telecom.TargetSelection.MidRound"),
            MiraLocaleManager.Get("DivaniMods.Options.Telecom.TargetSelection.Meeting")
        ]
        );

    public ModdedNumberOption TransmissionDelay { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.Telecom.TransmissionDelay"), 3f, 0f, 10f, 1f, MiraNumberSuffixes.Seconds)
    {
        Visible = () => OptionGroupSingleton<TelecomOptions>.Instance.TargetSelection.Value ==
                        (int)TelecomTargetSelectionOptions.MidRound,
    };

    public ModdedNumberOption TransmissionCooldown { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.Telecom.TransmissionCooldown"), 25f, 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds)
    {
        Visible = () => OptionGroupSingleton<TelecomOptions>.Instance.TargetSelection.Value ==
                        (int)TelecomTargetSelectionOptions.MidRound,
    };
}
