using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using DivaniMods.Roles.Impostor.ImpostorKilling;
namespace DivaniMods.Options;

public class DeathnoteOptions : AbstractRoleOptionGroup<DeathnoteRole>
{
    public override string GroupName => "Deathnote";

    [ModdedToggleOption("DivaniMods.Options.Deathnote.CanNoteFirstRound")]
    public bool CanNoteFirstRound { get; set; } = false;

    public ModdedNumberOption MaxNotesPerGame { get; } = new(
        MiraAPI.Translation.MiraLocaleManager.Get("DivaniMods.Options.Deathnote.MaxNotesPerGame"), 3f, 0f, 10f, 1f, MiraNumberSuffixes.None, "0", true);

    public ModdedNumberOption StartingCharges { get; } = new(
        MiraAPI.Translation.MiraLocaleManager.Get("DivaniMods.Options.Deathnote.StartingCharges"), 1f, 0f, 10f, 1f, MiraNumberSuffixes.None, "0", true);

    public ModdedNumberOption KillsPerExtraCharge { get; } = new(
        MiraAPI.Translation.MiraLocaleManager.Get("DivaniMods.Options.Deathnote.KillsPerExtraCharge"), 2f, 0f, 10f, 1f, MiraNumberSuffixes.None, "0", true);

    public ModdedNumberOption NoteCooldown { get; } = new(
        MiraAPI.Translation.MiraLocaleManager.Get("DivaniMods.Options.Deathnote.NoteCooldown"), 35f, 0f, 60f, 2.5f, MiraNumberSuffixes.Seconds);
}
