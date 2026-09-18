using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using DivaniMods.Roles.Impostor.ImpostorPower;
using TownOfUs.Extensions;

namespace DivaniMods.Options;

public class RecruiterOptions : AbstractRoleOptionGroup<RecruiterRole>
{
    public override string GroupName => MiraLocaleManager.Get("DivaniMods.Role.Recruiter", "Recruiter");

    public ModdedToggleOption RecruitedBecomesAssassin { get; } =
        new(MiraLocaleManager.Get("DivaniMods.Options.Recruiter.RecruitedBecomesAssassin"), true);

    public ModdedToggleOption RecruiterCanChangeRole { get; } =
        new(MiraLocaleManager.Get("DivaniMods.Options.Recruiter.RecruiterCanChangeRole"), true);

    public ModdedToggleOption RemoveExistingRoles { get; } =
        new(MiraLocaleManager.Get("DivaniMods.Options.Recruiter.RemoveExistingRoles"), true);

    public ModdedEnumOption RecruitGuess { get; } = new(MiraLocaleManager.Get("DivaniMods.Options.Recruiter.RecruitGuess"),
        (int)CacheRoleGuess.ActiveRole, typeof(CacheRoleGuess),
        [
            MiraLocaleManager.Get("DivaniMods.Options.Recruiter.RecruitGuess.OriginalRole"),
            MiraLocaleManager.Get("DivaniMods.Options.Recruiter.RecruitGuess.NewRole"),
            MiraLocaleManager.Get("DivaniMods.Options.Recruiter.RecruitGuess.OriginalOrNewRole")
        ]
        );
}
