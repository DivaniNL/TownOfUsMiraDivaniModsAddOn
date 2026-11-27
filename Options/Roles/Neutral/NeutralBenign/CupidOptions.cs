using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Translation;
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

public class CupidOptions : AbstractRoleOptionGroup<CupidRole>
{
    public override string GroupName => MiraLocaleManager.Get("DivaniMods.Role.Cupid", "Cupid");

    public ModdedNumberOption MatchmakeCooldown { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.Cupid.MatchmakeCooldown"), 10f, 5f, 60f, 2.5f, MiraNumberSuffixes.Seconds);

    public ModdedNumberOption ProtectCooldown { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.Cupid.ProtectCooldown"), 25f, 5f, 120f, 2.5f, MiraNumberSuffixes.Seconds);

    public ModdedNumberOption ProtectDuration { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.Cupid.ProtectDuration"), 10f, 5f, 15f, 1f, MiraNumberSuffixes.Seconds);

    public ModdedEnumOption<CupidProtectShowOptions> ShowProtect { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.Cupid.ShowProtect"), CupidProtectShowOptions.CupidAndLovers,
        [
            MiraLocaleManager.Get("DivaniMods.Options.Cupid.ShowProtect.Cupid"),
            MiraLocaleManager.Get("DivaniMods.Options.Cupid.ShowProtect.CupidAndLovers"),
            MiraLocaleManager.Get("DivaniMods.Options.Cupid.ShowProtect.Everyone")
        ]
        );

    public ModdedEnumOption<CupidBecomeOptions> OnLoverDeath { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.Cupid.OnLoverDeath"), CupidBecomeOptions.Amnesiac,
        [
            MiraLocaleManager.Get("DivaniMods.Options.Cupid.OnLoverDeath.Crew"),
            MiraLocaleManager.Get("DivaniMods.Options.Cupid.OnLoverDeath.Amnesiac"),
            MiraLocaleManager.Get("DivaniMods.Options.Cupid.OnLoverDeath.Survivor"),
            MiraLocaleManager.Get("DivaniMods.Options.Cupid.OnLoverDeath.Mercenary"),
            MiraLocaleManager.Get("DivaniMods.Options.Cupid.OnLoverDeath.Jester"),
            MiraLocaleManager.Get("DivaniMods.Options.Cupid.OnLoverDeath.CupidDies")
        ]
        );

    public ModdedToggleOption CupidRevivedOnLoversRevive { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.Cupid.CupidRevivedOnLoversRevive"), false)
    {
        Visible = () => OptionGroupSingleton<CupidOptions>.Instance.OnLoverDeath.Value == CupidBecomeOptions.CupidDies,
    };

    [ModdedToggleOption("DivaniMods.Options.Cupid.LoversKnowCupid")]
    public bool LoversKnowCupid { get; set; } = true;

    [ModdedToggleOption("DivaniMods.Options.Cupid.CupidKnowsLoverRoles")]
    public bool CupidKnowsLoverRoles { get; set; } = true;

    [ModdedToggleOption("DivaniMods.Options.Cupid.ProtectSeparately")]
    public bool ProtectSeparately { get; set; } = false;

}
