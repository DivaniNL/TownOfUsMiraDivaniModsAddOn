using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using DivaniMods.Roles.Neutral.NeutralEvil;

namespace DivaniMods.Options;

public class PlagueDoctorOptions : AbstractRoleOptionGroup<PlagueDoctorRole>
{
    public override string GroupName => MiraLocaleManager.Get("DivaniMods.Role.PlagueDoctor", "Plague Doctor");

    public ModdedNumberOption InfectCooldown { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.PlagueDoctor.InfectCooldown"), 25f, 5f, 60f, 2.5f, MiraNumberSuffixes.Seconds);

    public ModdedNumberOption MaxInfections { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.PlagueDoctor.MaxInfections"), 2f, 1f, 5f, 1f, MiraNumberSuffixes.None);

    public ModdedNumberOption InfectDistance { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.PlagueDoctor.InfectDistance"), 1f, 0.4f, 2f, 0.2f, MiraNumberSuffixes.Multiplier);

    public ModdedNumberOption InfectDuration { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.PlagueDoctor.InfectDuration"), 10f, 1f, 30f, 1f, MiraNumberSuffixes.Seconds);

    public ModdedNumberOption ImmunityTime { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.PlagueDoctor.ImmunityTime"), 10f, 0f, 30f, 2.5f, MiraNumberSuffixes.Seconds);

    [ModdedToggleOption("DivaniMods.Options.PlagueDoctor.CanVent")]
    public bool CanVent { get; set; } = false;

    [ModdedToggleOption("DivaniMods.Options.PlagueDoctor.TurnIntoAmne")]
    public bool TurnIntoAmne { get; set; } = true;

    [ModdedToggleOption("DivaniMods.Options.PlagueDoctor.CanWinDead")]
    public bool CanWinDead { get; set; } = false;

    public ModdedToggleOption InfectKiller { get; } = new("DivaniMods.Options.PlagueDoctor.InfectKiller", false)
    {
        Visible = () => OptionGroupSingleton<PlagueDoctorOptions>.Instance.CanWinDead,
    };

    [ModdedToggleOption("DivaniMods.Options.PlagueDoctor.NotifyPlayersWhenInfectionClose")]
    public bool NotifyPlayersWhenInfectionClose { get; set; } = true;

    public ModdedNumberOption NotifyWhenUninfectedLeft { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.PlagueDoctor.NotifyWhenUninfectedLeft"),
        3,
        1,
        14,
        1,
        MiraNumberSuffixes.None)
    {
        Visible = () => OptionGroupSingleton<PlagueDoctorOptions>.Instance.NotifyPlayersWhenInfectionClose,
    };

    [ModdedEnumOption("DivaniMods.Options.PlagueDoctor.WinOutcome", typeof(NeutralEvilWinOutcome), 
        [
            "DivaniMods.Options.PlagueDoctor.WinOutcome.EndsGame",
            "DivaniMods.Options.PlagueDoctor.WinOutcome.KillOnePlayer",
            "DivaniMods.Options.PlagueDoctor.WinOutcome.Nothing"
        ]
    )]
    public NeutralEvilWinOutcome WinOutcome { get; set; } = NeutralEvilWinOutcome.EndsGame;
}
