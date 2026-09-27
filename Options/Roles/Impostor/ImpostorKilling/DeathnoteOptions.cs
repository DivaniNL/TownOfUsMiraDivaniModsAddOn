using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using DivaniMods.Roles.Impostor.ImpostorKilling;

namespace DivaniMods.Options;

public class DeathnoteOptions : AbstractRoleOptionGroup<DeathnoteRole>
{
    public override string GroupName => "Deathnote";

    public ModdedNumberOption NotesPerGame { get; } = new(
        "Notes per game", 3f, 1f, 5f, 1f, MiraNumberSuffixes.None);

    public ModdedNumberOption NoteCooldown { get; } = new(
        "Note Cooldown", 35f, 0f, 60f, 2.5f, MiraNumberSuffixes.Seconds);
}
