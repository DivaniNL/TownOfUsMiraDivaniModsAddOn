using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using DivaniMods.Roles.Crewmate.CrewmatePower;

namespace DivaniMods.Options;

public class MentorOptions : AbstractOptionGroup<MentorRole>
{
    public override string GroupName => MiraLocaleManager.Get("DivaniMods.Role.Mentor", "Mentor");

    public ModdedToggleOption CanTeachCrewKilling { get; } =
        new(MiraLocaleManager.Get("DivaniMods.Options.Mentor.CanTeachCrewKilling", "Can Teach To Be Crewmate Killing"), false);
    public ModdedToggleOption CanTeachCrewPower { get; } =
        new(MiraLocaleManager.Get("DivaniMods.Options.Mentor.CanTeachCrewPower", "Can Teach To Be Crewmate Power"), false);
    public ModdedToggleOption CanTeachCrewInves { get; } =
        new(MiraLocaleManager.Get("DivaniMods.Options.Mentor.CanTeachCrewInves", "Can Teach To Be Crewmate Investigative"), false);
    public ModdedToggleOption NotifyTargetOnAttempt { get; } =
        new(MiraLocaleManager.Get("DivaniMods.Options.Mentor.NotifyTargetOnAttempt", "Target Is Notified On Failed Attempt"), false);

    public ModdedToggleOption CanTeachRoundOne { get; } =
        new(MiraLocaleManager.Get("DivaniMods.Options.Mentor.CanTeachRoundOne", "Can Teach In First Meeting"), true);

    public ModdedNumberOption TasksNeededToTeachAgain { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.Mentor.TasksNeededToTeachAgain", "Tasks Needed To Teach Again"), 0f, 0f, 10f, 1f, MiraNumberSuffixes.None);

    public ModdedToggleOption CanTeachOnSamePlayerAgain { get; } =
        new(MiraLocaleManager.Get("DivaniMods.Options.Mentor.CanTeachOnSamePlayerAgain", "Can Teach The Same Player More Than Once"), true);

    public ModdedNumberOption InsomniaRounds { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.Mentor.InsomniaRounds", "Rounds Needed To Teach Again"), 1f, 1f, 3f, 1f, MiraNumberSuffixes.None);
        {
            Visible = () => OptionGroupSingleton<MentorOptions>.Instance.CanTeachOnSamePlayerAgain
        };
    
    public ModdedToggleOption NotifyTargetOnAttempt { get; } =
        new("DivaniMods.Options.Mentor.NotifyEvilTargetOnAttempt", true);
    
    public ModdedToggleOption NotifyTargetOfRoleOnAttempt { get; } =
        new("DivaniMods.Options.Mentor.NotifyEvilTargetOfRole", true)
        {
            Visible = () => OptionGroupSingleton<MentorOptions>.Instance.NotifyTargetOnAttempt
        };
}
