using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using DivaniMods.Roles.Impostor.ImpostorPower;

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
        "Target Selection", (int)MosquitoTargetMode.Furthest, typeof(MosquitoTargetMode),
        ["Furthest", "Selection Tablet"]);

    public ModdedNumberOption StingCooldown { get; } = new(
        "Sting Cooldown", 25f, 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds);

    public ModdedNumberOption StingCharges { get; } = new(
        "Sting Charges", 2f, 0f, 15f, 1f, "∞", "#", MiraNumberSuffixes.None, "0");

    public ModdedNumberOption ChargesPerKill { get; } = new(
        "Charges Per Kill", 1f, 0f, 3f, 1f, MiraNumberSuffixes.None)
    {
        Visible = () => OptionGroupSingleton<MosquitoOptions>.Instance.StingCharges.Value > 0,
    };

    public ModdedToggleOption AimbotMode { get; } = new("Aimbot Mode", false);
}
