using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using DivaniMods.Roles.Crewmate.CrewmatePower;

namespace DivaniMods.Options;

public enum DreamerReimagineRestriction
{
    CrewmateKilling,
    CrewmatePower,
    Nothing,
}

public class DreamerOptions : AbstractOptionGroup<DreamerRole>
{
    public override string GroupName => "Dreamer";

    public ModdedEnumOption CannotReimagineInto { get; } = new(
        "Dreamer Cannot Reimagine Into", (int)DreamerReimagineRestriction.Nothing,
        typeof(DreamerReimagineRestriction),
        ["Crewmate Killing", "Crewmate Power", "Nothing"]);

    public ModdedToggleOption NotifyNonCrewOnAttempt { get; } =
        new("Non-Crew Are Notified On Attempt", false);

    public ModdedToggleOption NotifyDreamerOnFail { get; } =
        new("Dreamer Notified On Failed Dream", true);

    public ModdedNumberOption InsomniaRounds { get; } = new(
        "Insomnia Lasts For Rounds", 1f, 1f, 3f, 1f, MiraNumberSuffixes.None);
}
