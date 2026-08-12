using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using DivaniMods.Roles.Impostor.ImpostorConcealing;

namespace DivaniMods.Options;

public class CunctatorOptions : AbstractRoleOptionGroup<CunctatorRole>
{
    public override string GroupName => "Cunctator";

    public ModdedNumberOption BodyDelay { get; } = new(
        "Body Spawn Delay", 10f, 1f, 30f, 1f, MiraNumberSuffixes.Seconds);
}
