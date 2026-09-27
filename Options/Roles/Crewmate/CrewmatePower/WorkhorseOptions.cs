using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using DivaniMods.Roles.Crewmate.CrewmatePower;

namespace DivaniMods.Options;

public class WorkhorseOptions : AbstractRoleOptionGroup<WorkhorseRole>
{
    public override string GroupName => MiraLocaleManager.Get("DivaniMods.Role.Workhorse", "Workhorse");

    public ModdedNumberOption ExtraLongTasks { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.Workhorse.ExtraLongTasks"), 2f, 1f, 5f, 1f, MiraNumberSuffixes.None);

    public ModdedNumberOption ExtraShortTasks { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.Workhorse.ExtraShortTasks"), 3f, 1f, 5f, 1f, MiraNumberSuffixes.None);

    public ModdedNumberOption ExtraCommonTasks { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.Workhorse.ExtraCommonTasks"), 1f, 1f, 5f, 1f, MiraNumberSuffixes.None);

    public ModdedToggleOption NotifyEvilsOnFirstList { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.Workhorse.NotifyEvilsOnFirstList"), true);

    public ModdedNumberOption ExtraTasksLeftWhenRevealed { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.Workhorse.ExtraTasksLeftWhenRevealed"), 2f, 1f, 15f, 1f, MiraNumberSuffixes.None);

    public ModdedToggleOption ContinuesGame { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.Workhorse.ContinuesGame"), false);
}