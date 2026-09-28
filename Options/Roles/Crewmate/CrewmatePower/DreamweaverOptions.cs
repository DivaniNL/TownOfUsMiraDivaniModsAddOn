using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using DivaniMods.Roles.Crewmate.CrewmatePower;

namespace DivaniMods.Options;

public enum DreamweaverOnDreamBreakMaxRoleCount
{
    ApplyRandom,
    DreamFail,
}

public class DreamweaverOptions : AbstractOptionGroup<DreamweaverRole>
{
    public override string GroupName => "Dreamweaver";

    public ModdedToggleOption CanCastCrewKilling { get; } =
        new("Can cast to be Crewmate Killing", false);
    public ModdedToggleOption CanCastCrewPower { get; } =
        new("Can cast to be Crewmate Power", false);
    public ModdedToggleOption CanCastCrewInves { get; } =
        new("Can cast to be Crewmate Investigative", false);
    public ModdedToggleOption NotifyTargetOnAttempt { get; } =
        new("Target Is Notified On Failed Attempt", false);

    public ModdedToggleOption NotifyDreamweaverOnFail { get; } =
        new("Dreamweaver Notified On Failed Dream", false);
    
    public ModdedToggleOption FailDreamOnNoChange { get; } =
        new ("Fail Casting of Dream If Target And Dream Role Are Same", true);

    public ModdedNumberOption InsomniaRounds { get; } = new(
        "Rounds needed to Cast again", 1f, 1f, 3f, 1f, MiraNumberSuffixes.None);
}
