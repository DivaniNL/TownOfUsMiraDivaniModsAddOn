using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using DivaniMods.Roles.Crewmate.CrewmateInvestigative;
using TownOfUs.Roles.Crewmate;

namespace DivaniMods.Options;

public class ChameleonOptions : AbstractOptionGroup<ChameleonRole>
{
    public override string GroupName => "Chameleon";

    public ModdedNumberOption MaxCamouflages { get; } = new(
        "Max Camouflages", 2f, 1f, 5f, 1f, MiraNumberSuffixes.None);

    public ModdedNumberOption CamouflageCooldown { get; } = new(
        "Camouflage Cooldown", 20f, 5f, 60f, 2.5f, MiraNumberSuffixes.Seconds);

    public ModdedNumberOption CamouflageDuration { get; } = new(
        "Camouflage Duration", 7f, 1f, 60f, 0.25f, MiraNumberSuffixes.Seconds);

    [ModdedToggleOption("Can vent while invis")]
    public bool CanVent { get; set; } = false;
}