using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using DivaniMods.Roles.Impostor.ImpostorKilling;

namespace DivaniMods.Options;

public enum MosquitoTargetMode
{
    Furthest,
    PlayerSelection,
}

public class MosquitoOptions : AbstractOptionGroup<MosquitoRole>
{
    public override string GroupName => "Mosquito";

    public ModdedEnumOption TargetMode { get; } = new(
        "Target Selection", (int)MosquitoTargetMode.PlayerSelection, typeof(MosquitoTargetMode),
        ["Furthest", "Selection Tablet"]);

    public ModdedNumberOption StingCooldown { get; } = new(
        "Sting Cooldown", 25f, 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds);

    public ModdedNumberOption StingCharges { get; } = new(
        "Initial Charges", 1f, 0f, 5f, 1f, MiraNumberSuffixes.None);

    public ModdedNumberOption ChargesPerKill { get; } = new(
        "Charges Per Kill", 1f, 0f, 3f, 1f, MiraNumberSuffixes.None);

    public ModdedToggleOption AimbotMode { get; } = new("Aimbot Mode", true);
}
