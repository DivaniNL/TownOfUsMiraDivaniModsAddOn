using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using DivaniMods.Roles.Impostor.ImpostorSupport;

namespace DivaniMods.Options;

public class RecruiterOptions : AbstractOptionGroup<RecruiterRole>
{
    public override string GroupName => "Recruiter";

    /// <summary>
    /// When enabled, recruited crew become vanilla Impostor plus Impostor Assassin modifier (same idea as Amnesiac remembering an Imp with assassin options on).
    /// When disabled (default), they stay a plain vanilla Impostor (follow-up RPC strips any Impostor Assassin TOU may have applied).
    /// </summary>
    public ModdedToggleOption RecruitedBecomesAssassin { get; } =
        new("Recruited Impostor Becomes Assassin", false);
}
