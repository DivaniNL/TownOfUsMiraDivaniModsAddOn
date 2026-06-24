using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using DivaniMods.Roles.Impostor.ImpostorSupport;

namespace DivaniMods.Options;

public class CouncillorOptions : AbstractOptionGroup<CouncillorRole>
{
    public override string GroupName => "Councillor";

    [ModdedToggleOption("Gain the extra Votes from killing Knights and Mayors")]
    public bool GainsAllVotes { get; set; } = false;
}
