using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Translation;
using DivaniMods.Roles.Impostor.ImpostorSupport;

namespace DivaniMods.Options;

public class CouncillorOptions : AbstractRoleOptionGroup<CouncillorRole>
{
    public override string GroupName => MiraLocaleManager.Get("DivaniMods.Role.Councillor", "Councillor");

    [ModdedToggleOption("DivaniMods.Options.Councillor.GainsAllVotes")]
    public bool GainsAllVotes { get; set; } = false;
}
