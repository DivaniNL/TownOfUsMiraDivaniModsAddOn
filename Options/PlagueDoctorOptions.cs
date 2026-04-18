using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using DivaniMods.Roles;

namespace DivaniMods.Options;

public class PlagueDoctorOptions : AbstractOptionGroup<PlagueDoctorRole>
{
    public override string GroupName => "Plague Doctor";

    [ModdedNumberOption("Infect Cooldown", 5, 60, 2.5f, MiraNumberSuffixes.Seconds)]
    public float InfectCooldown { get; set; } = 25;

    [ModdedNumberOption("Max Direct Infections", 1, 5, 1)]
    public float MaxInfections { get; set; } = 2;

    [ModdedNumberOption("Infection Distance", 0.5f, 3f, 0.25f, MiraNumberSuffixes.Multiplier)]
    public float InfectDistance { get; set; } = 1.5f;

    [ModdedNumberOption("Infection Duration", 1, 30, 1, MiraNumberSuffixes.Seconds)]
    public float InfectDuration { get; set; } = 5;

    [ModdedNumberOption("Post-Meeting Immunity", 0, 30, 2.5f, MiraNumberSuffixes.Seconds)]
    public float ImmunityTime { get; set; } = 10;

    [ModdedToggleOption("Infect Killer On Death")]
    public bool InfectKiller { get; set; } = true;

    [ModdedToggleOption("Can Win While Dead")]
    public bool CanWinDead { get; set; } = true;

    [ModdedToggleOption("Can Use Vents")]
    public bool CanVent { get; set; } = false;
}
