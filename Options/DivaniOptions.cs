using MiraAPI.GameOptions;
using MiraAPI.Translation;
using MiraAPI.GameOptions.Attributes;

namespace DivaniMods.Options;

public sealed class DivaniOptions : AbstractOptionGroup
{
    public override string GroupName => "Divani Mods";

    [ModdedToggleOption("DivaniMods.Options.Main.UseDutchMemeSoundpack")]
    public bool UseDutchMemeSoundpack { get; set; } = false;

    [ModdedToggleOption("DivaniMods.Options.Main.RainbowCamoComms")]
    public bool RainbowCamoComms { get; set; } = false;
}
