using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using DivaniMods.Roles;

namespace DivaniMods.Options;

public class StalkerOptions : AbstractOptionGroup<StalkerRole>
{
    public override string GroupName => "Stalker";

    [ModdedToggleOption("Anonymous Lover Chat Access")]
    public bool CanReadLoverChat { get; set; } = true;

    [ModdedToggleOption("Wrong Guess Exiles Stalker")]
    public bool WrongGuessExiles { get; set; } = true;
}
