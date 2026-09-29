using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using DivaniMods.Roles.Crewmate.CrewmatePower;

namespace DivaniMods.Options;

public class MentorOptions : AbstractOptionGroup<MentorRole>
{
    public override string GroupName => "Mentor";

    public ModdedToggleOption CanTeachCrewKilling { get; } =
        new("Can teach to be Crewmate Killing", false);
    public ModdedToggleOption CanTeachCrewPower { get; } =
        new("Can teach to be Crewmate Power", false);
    public ModdedToggleOption CanTeachCrewInves { get; } =
        new("Can teach to be Crewmate Investigative", false);
    public ModdedToggleOption NotifyTargetOnAttempt { get; } =
        new("Target Is Notified On Failed Attempt", false);

    public ModdedToggleOption NotifyMentorOnFail { get; } =
        new("Mentor Notified On Failed Lesson", false);
    
    public ModdedToggleOption FailLessonOnNoChange { get; } =
        new ("Fail the Lesson If Target And Taught Role Are Same", true);

    public ModdedNumberOption InsomniaRounds { get; } = new(
        "Rounds needed to Teach again", 1f, 1f, 3f, 1f, MiraNumberSuffixes.None);

    public ModdedToggleOption CanTeachRoundOne { get; } =
        new("Can Teach In First Meeting", true);

    public ModdedNumberOption TasksNeededToTeachAgain { get; } = new(
        "Tasks Needed To Teach Again", 0f, 0f, 10f, 1f, MiraNumberSuffixes.None);

    public ModdedToggleOption CanTeachOnSamePlayerAgain { get; } =
        new("Can Teach On The Same Player More Than Once", true);
}
