using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using DivaniMods.Roles.Neutral.NeutralBenign;

namespace DivaniMods.Options;

public enum CupidProtectShowOptions
{
    Cupid,
    CupidAndLovers,
    Everyone
}

public enum CupidBecomeOptions
{
    Crew,
    Amnesiac,
    Survivor,
    Mercenary,
    Jester,
    CupidDies
}

public class CupidOptions : AbstractOptionGroup<CupidRole>
{
    public override string GroupName => "Cupid";

    public ModdedNumberOption MatchmakeCooldown { get; } = new(
        "Matchmake Cooldown", 10f, 5f, 60f, 2.5f, MiraNumberSuffixes.Seconds);

    public ModdedNumberOption ProtectCooldown { get; } = new(
        "Bestow Cooldown", 25f, 5f, 120f, 2.5f, MiraNumberSuffixes.Seconds);

    public ModdedNumberOption ProtectDuration { get; } = new(
        "Bestow Duration", 10f, 5f, 15f, 1f, MiraNumberSuffixes.Seconds);

    public ModdedEnumOption ShowProtect { get; } = new(
        "Show Protection", (int)CupidProtectShowOptions.CupidAndLovers, typeof(CupidProtectShowOptions),
        ["Cupid", "Cupid And Lovers", "Everyone"]);

    public ModdedEnumOption OnLoverDeath { get; } = new(
        "When A Lover Dies, Cupid Becomes", (int)CupidBecomeOptions.Amnesiac, typeof(CupidBecomeOptions),
        ["Crewmate", "Amnesiac", "Survivor", "Mercenary", "Jester", "Cupid Dies"]);

    [ModdedToggleOption("Lovers Know Cupid Exists")]
    public bool LoversKnowCupid { get; set; } = true;

    [ModdedToggleOption("Cupid Knows Lovers' Roles")]
    public bool CupidKnowsLoverRoles { get; set; } = true;

    [ModdedToggleOption("Protect Lovers Separately")]
    public bool ProtectSeparately { get; set; } = false;
}
