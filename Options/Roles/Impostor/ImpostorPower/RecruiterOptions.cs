using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using DivaniMods.Roles.Impostor.ImpostorPower;
using TownOfUs.Extensions;

namespace DivaniMods.Options;

public class RecruiterOptions : AbstractRoleOptionGroup<RecruiterRole>
{
    public override string GroupName => "Recruiter";

    public ModdedToggleOption RecruitedBecomesAssassin { get; } =
        new("Recruited Impostor Becomes Assassin", true);

    public ModdedToggleOption RecruiterCanChangeRole { get; } =
        new("Recruiter Can Choose A New Role After Recruiting", true);

    public ModdedToggleOption RemoveExistingRoles { get; } =
        new("Role Change Can't Pick Existing Impostor Roles", true);

    public ModdedEnumOption RecruitGuess { get; } = new("Changed Role Must Be Guessed As",
        (int)CacheRoleGuess.ActiveRole, typeof(CacheRoleGuess),
        ["Original Role", "New Role", "Original Or New Role"]);
}
