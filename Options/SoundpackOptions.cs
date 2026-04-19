using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;

namespace DivaniMods.Options;

public sealed class SoundpackOptions : AbstractOptionGroup
{
    public override string GroupName => "Soundpack";

    // Replaces vanilla door-open / door-close SFX with the custom Dutch Meme
    // clips shipped in Resources/. Handled by DutchMemeSoundpackPatch at round
    // start so the swap is atomic per-game and reverts automatically when the
    // toggle is off (original map prefab is re-instantiated each round).
    [ModdedToggleOption("Use Dutch Meme Soundpack")]
    public bool UseDutchMemeSoundpack { get; set; } = false;
}
