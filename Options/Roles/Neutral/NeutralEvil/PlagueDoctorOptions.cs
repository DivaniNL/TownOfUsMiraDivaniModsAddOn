using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using DivaniMods.Roles.Neutral.NeutralEvil;

namespace DivaniMods.Options;

public class PlagueDoctorOptions : AbstractRoleOptionGroup<PlagueDoctorRole>
{
    public override string GroupName => "Plague Doctor";

    public ModdedNumberOption InfectCooldown { get; } = new(
        "Infect Cooldown", 25f, 5f, 60f, 2.5f, MiraNumberSuffixes.Seconds);

    public ModdedNumberOption MaxInfections { get; } = new(
        "Max Direct Infections", 2f, 1f, 5f, 1f, MiraNumberSuffixes.None);

    public ModdedNumberOption InfectDistance { get; } = new(
        "Infection Distance", 1f, 0.4f, 2f, 0.2f, MiraNumberSuffixes.Multiplier);

    public ModdedNumberOption InfectDuration { get; } = new(
        "Infection Duration", 10f, 1f, 30f, 1f, MiraNumberSuffixes.Seconds);

    public ModdedNumberOption ImmunityTime { get; } = new(
        "Post-Meeting Immunity", 10f, 0f, 30f, 2.5f, MiraNumberSuffixes.Seconds);

    [ModdedToggleOption("Can Use Vents")]
    public bool CanVent { get; set; } = false;

    [ModdedToggleOption("Turn into Amnesiac after you can no longer win")]
    public bool TurnIntoAmne { get; set; } = true;

    [ModdedToggleOption("Can Win While Dead")]
    public bool CanWinDead { get; set; } = false;

    public ModdedToggleOption InfectKiller { get; } = new("Infect Killer On Death", false)
    {
        Visible = () => OptionGroupSingleton<PlagueDoctorOptions>.Instance.CanWinDead,
    };

    [ModdedToggleOption("Notify Players When Infection Is Close")]
    public bool NotifyPlayersWhenInfectionClose { get; set; } = true;

    public ModdedNumberOption NotifyWhenUninfectedLeft { get; } = new(
        "Notify When Uninfected Players Left",
        3,
        1,
        14,
        1,
        MiraNumberSuffixes.None)
    {
        Visible = () => OptionGroupSingleton<PlagueDoctorOptions>.Instance.NotifyPlayersWhenInfectionClose,
    };
}
