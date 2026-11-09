using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using DivaniMods.Roles;

namespace DivaniMods.Options;

public class StalkerOptions : AbstractOptionGroup<StalkerRole>
{
    public override string GroupName => "Stalker";

    [ModdedToggleOption("DivaniMods.Options.Stalker.CanReadLoverChat")]
    public bool CanReadLoverChat { get; set; } = true;

    [ModdedToggleOption("DivaniMods.Options.Stalker.WrongGuessExiles")]
    public bool WrongGuessExiles { get; set; } = true;
}
