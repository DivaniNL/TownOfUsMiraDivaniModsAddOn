using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using DivaniMods.Roles.Crewmate.CrewmatePower;

namespace DivaniMods.Options;

public class WorkhorseOptions : AbstractRoleOptionGroup<WorkhorseRole>
{
    public override string GroupName => "Workhorse";

    public ModdedNumberOption ExtraLongTasks { get; } = new(
        "Extra Long tasks", 2f, 1f, 5f, 1f, MiraNumberSuffixes.None);

    public ModdedNumberOption ExtraShortTasks { get; } = new(
        "Extra Short tasks", 3f, 1f, 5f, 1f, MiraNumberSuffixes.None);

    public ModdedNumberOption ExtraCommonTasks { get; } = new(
        "Extra Common tasks", 1f, 1f, 5f, 1f, MiraNumberSuffixes.None);

    public ModdedToggleOption NotifyEvilsOnFirstList { get; } = new(
        "Notify Evils Of Finished Initial Task List", true);

    public ModdedNumberOption ExtraTasksLeftWhenRevealed { get; } = new(
        "Extra Tasks Left When Revealed", 2f, 1f, 15f, 1f, MiraNumberSuffixes.None);

    public ModdedToggleOption ContinuesGame { get; } = new(
        "Workhorse Continues Game", false);
}