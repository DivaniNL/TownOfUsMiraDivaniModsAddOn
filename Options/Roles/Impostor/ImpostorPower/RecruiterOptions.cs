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

    public ModdedToggleOption RecruitRemoveExistingRoles { get; } =
        new("Recruit Can't Pick Existing Impostor Roles", true);

    public ModdedEnumOption RecruitGuess { get; } = new("Recruit Must Be Guessed As",
        (int)CacheRoleGuess.ActiveRole, typeof(CacheRoleGuess),
        ["Recruit", "New Role", "Recruit or New Role"]);
}
