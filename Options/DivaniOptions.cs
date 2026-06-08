using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;

namespace DivaniMods.Options;

public sealed class DivaniOptions : AbstractOptionGroup
{
    public override string GroupName => "Divani Mods";

    [ModdedToggleOption("Use Dutch Meme Soundpack")]
    public bool UseDutchMemeSoundpack { get; set; } = false;

    [ModdedToggleOption("Rainbow Camouflaged Comms")]
    public bool RainbowCamoComms { get; set; } = false;
}
