using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using DivaniMods.Roles.Neutral.NeutralEvil;

namespace DivaniMods.Options;

public class PlagueDoctorOptions : AbstractOptionGroup<PlagueDoctorRole>
{
    public override string GroupName => "Plague Doctor";
    [ModdedNumberOption("Infect Cooldown", 5, 60, 2.5f, MiraNumberSuffixes.Seconds)]
    public float InfectCooldown { get; set; } = 25;

    [ModdedNumberOption("Max Direct Infections", 1, 5, 1)]
    public float MaxInfections { get; set; } = 2;

    [ModdedNumberOption("Infection Distance", 0.4f, 2f, 0.2f, MiraNumberSuffixes.Multiplier)]
    public float InfectDistance { get; set; } = 0.6f;

    [ModdedNumberOption("Infection Duration", 1, 30, 1, MiraNumberSuffixes.Seconds)]
    public float InfectDuration { get; set; } = 5;

    [ModdedNumberOption("Post-Meeting Immunity", 0, 30, 2.5f, MiraNumberSuffixes.Seconds)]
    public float ImmunityTime { get; set; } = 10;

    [ModdedToggleOption("Can Use Vents")]
    public bool CanVent { get; set; } = false;

    [ModdedToggleOption("Turn into Amnesiac when cannot win")]
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
