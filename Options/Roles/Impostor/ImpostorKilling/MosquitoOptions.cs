using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using DivaniMods.Roles.Impostor.ImpostorKilling;

namespace DivaniMods.Options;

public enum MosquitoTargetMode
{
    Furthest,
    PlayerSelection,
}

public class MosquitoOptions : AbstractRoleOptionGroup<MosquitoRole>
{
    public override string GroupName => MiraLocaleManager.Get("DivaniMods.Role.Mosquito", "Mosquito");

    public ModdedEnumOption TargetMode { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.Mosquito.TargetMode"), (int)MosquitoTargetMode.PlayerSelection, typeof(MosquitoTargetMode),
        [
            MiraLocaleManager.Get("DivaniMods.Options.Mosquito.TargetMode.Furthest"),
            MiraLocaleManager.Get("DivaniMods.Options.Mosquito.TargetMode.SelectionTablet")
        ]
        );

    public ModdedNumberOption StingCooldown { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.Mosquito.StingCooldown"), 25f, 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds);

    public ModdedNumberOption StingCharges { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.Mosquito.StingCharges"), 1f, 0f, 5f, 1f, MiraNumberSuffixes.None);

    public ModdedNumberOption ChargesPerKill { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.Mosquito.ChargesPerKill"), 1f, 0f, 3f, 1f, MiraNumberSuffixes.None);

    public ModdedToggleOption AimbotMode { get; } = new(MiraLocaleManager.Get("DivaniMods.Options.Mosquito.AimbotMode"), true);
}
