using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using DivaniMods.Roles.Neutral.NeutralOutlier;

namespace DivaniMods.Options;

public enum DuelistWinType
{
    WinAlone,
    LeaveInVictory,
}

public enum DuelSpawnType
{
    Close,
    Far,
    Random,
}

public class DuelistOptions : AbstractRoleOptionGroup<DuelistRole>
{
    public override string GroupName => MiraLocaleManager.Get("DivaniMods.Role.Duelist", "Duelist");

    public ModdedNumberOption DuelCooldown { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.Duelist.DuelCooldown"), 40f, 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds);

    public ModdedNumberOption DuelSpeed { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.Duelist.DuelSpeed"), 1.10f, 1.00f, 1.50f, 0.05f, MiraNumberSuffixes.Multiplier);

    public ModdedNumberOption DuelsToWin { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.Duelist.DuelsToWin"), 4f, 1f, 10f, 1f, MiraNumberSuffixes.None);

    public ModdedNumberOption DuelsLostToDie { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.Duelist.DuelsLostToDie"), 2f, 1f, 10f, 1f, MiraNumberSuffixes.None);

    public ModdedEnumOption WinType { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.Duelist.WinType"), (int)DuelistWinType.WinAlone, typeof(DuelistWinType),
        [
            MiraLocaleManager.Get("DivaniMods.Options.Duelist.WinType.WinAlone"),
            MiraLocaleManager.Get("DivaniMods.Options.Duelist.WinType.LeaveInVictory")
        ]
        );

    public ModdedEnumOption SpawnType { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.Duelist.SpawnType"), (int)DuelSpawnType.Close, typeof(DuelSpawnType),
        [
            MiraLocaleManager.Get("DivaniMods.Options.Duelist.SpawnType.Close"),
            MiraLocaleManager.Get("DivaniMods.Options.Duelist.SpawnType.Far"),
            MiraLocaleManager.Get("DivaniMods.Options.Duelist.SpawnType.Random")
        ]);
}
