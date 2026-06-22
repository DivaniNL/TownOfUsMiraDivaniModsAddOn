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

public enum DreamerOnDreamBreakMaxRoleCount
{
    ApplyRandom,
    DreamRedo,
    DreamFail,
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
        new("Dreamer Notified On Failed Dream", false);

    public ModdedNumberOption InsomniaRounds { get; } = new(
        "Insomnia Lasts For Rounds", 1f, 1f, 3f, 1f, MiraNumberSuffixes.None);

    public ModdedToggleOption RespectMaxRoleCount { get; } = new(
        "Respect Max Role Count On Reimagine?", true);
    
    public ModdedEnumOption OnMaxRoleCountBroken { get; } = new(
        "If Dream Breaks Max Role Count", (int)DreamerOnDreamBreakMaxRoleCount.ApplyRandom,
        typeof(DreamerOnDreamBreakMaxRoleCount),
        ["Give Random Role", "Choose New Dream", "Dream Fails"])
    {
        Visible = () => OptionGroupSingleton<DreamerOptions>.Instance.RespectMaxRoleCount.Value
    };
}
