using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using DivaniMods.Roles.Impostor.ImpostorPower;

namespace DivaniMods.Options;

public class RecruiterOptions : AbstractRoleOptionGroup<RecruiterRole>
{
    public override string GroupName => "Recruiter";

    public ModdedToggleOption RecruitedBecomesAssassin { get; } =
        new("Recruited Impostor Becomes Assassin", false);
}
