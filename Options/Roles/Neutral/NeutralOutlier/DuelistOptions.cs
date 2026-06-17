using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
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

public class DuelistOptions : AbstractOptionGroup<DuelistRole>
{
    public override string GroupName => "Duelist";

    public ModdedNumberOption DuelCooldown { get; } = new(
        "Duel Cooldown", 40f, 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds);

    public ModdedNumberOption DuelSpeed { get; } = new(
        "Duel Speed Boost", 1.10f, 1.00f, 1.50f, 0.05f, MiraNumberSuffixes.Multiplier);

    public ModdedNumberOption DuelsToWin { get; } = new(
        "Duels Needed To Win", 4f, 1f, 10f, 1f, MiraNumberSuffixes.None);

    public ModdedNumberOption DuelsLostToDie { get; } = new(
        "Duels Lost Before Dying", 2f, 1f, 10f, 1f, MiraNumberSuffixes.None);

    public ModdedEnumOption WinType { get; } = new(
        "When Victorious", (int)DuelistWinType.WinAlone, typeof(DuelistWinType),
        ["Win Alone", "Leave In Victory"]);

    public ModdedEnumOption SpawnType { get; } = new(
        "Duelers Spawn", (int)DuelSpawnType.Close, typeof(DuelSpawnType),
        ["Close", "Far", "Random"]);
}
