using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using DivaniMods.Roles.Crewmate.CrewmateSupport;

namespace DivaniMods.Options;

public sealed class MoleOptions : AbstractRoleOptionGroup<MoleRole>
{
    public override string GroupName => MiraLocaleManager.Get("DivaniMods.Role.Mole", "Mole");

    [ModdedNumberOption("DivaniMods.Options.Mole.DigCooldown", 5f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float DigCooldown { get; set; } = 25f;

    [ModdedNumberOption("DivaniMods.Options.Mole.MaxVents", 1f, 6f, 1f, MiraNumberSuffixes.None)]
    public float MaxVents { get; set; } = 4f;

    public ModdedToggleOption EarnMoreVents { get; } = new(MiraLocaleManager.Get("DivaniMods.Options.Mole.EarnMoreVents"), false);

    public ModdedNumberOption TasksPerVent { get; } =
        new(MiraLocaleManager.Get("DivaniMods.Options.Mole.TasksPerVent"), 3f, 1f, 5f, 1f, MiraNumberSuffixes.None)
        {
            Visible = () => OptionGroupSingleton<MoleOptions>.Instance.EarnMoreVents
        };

    public ModdedNumberOption VentTimeLimit { get; } =
        new(MiraLocaleManager.Get("DivaniMods.Options.Mole.VentTimeLimit"), 10f, 1f, 15f, 1f, MiraNumberSuffixes.Seconds);

    public ModdedNumberOption VentCooldown { get; } =
        new(MiraLocaleManager.Get("DivaniMods.Options.Mole.VentCooldown"), 10f, 1f, 15f, 1f, MiraNumberSuffixes.Seconds);

    [ModdedNumberOption("DivaniMods.Options.Mole.VentRoundDuration", 0f, 15f, 1f, MiraNumberSuffixes.None, "0", true)]
    public float VentRoundDuration { get; set; } = 2f;

    [ModdedEnumOption("DivaniMods.Options.Mole.VentUsage", typeof(MoleVentUsage), 
    [
        "DivaniMods.Options.Mole.VentUsage.Crewmates",
        "DivaniMods.Options.Mole.VentUsage.Anyone",
        "DivaniMods.Options.Mole.VentUsage.Mole"
    ]
    )]
    public MoleVentUsage VentUsage { get; set; } = MoleVentUsage.Anyone;

    [ModdedEnumOption("DivaniMods.Options.Mole.VentVisibility", typeof(MoleVentVisibility), 
    [
        "DivaniMods.Options.Mole.VentVisibility.Immediate",
        "DivaniMods.Options.Mole.VentVisibility.AfterUse",
        "DivaniMods.Options.Mole.VentVisibility.AfterNextMeeting"
    ]
    )]
    public MoleVentVisibility VentVisibility { get; set; } = MoleVentVisibility.Immediate;

    public ModdedNumberOption DigDelay { get; } = new(MiraLocaleManager.Get("DivaniMods.Options.Mole.DigDelay"), 3f, 0f, 10f, 0.5f, MiraNumberSuffixes.Seconds)
    {
        Visible = () => OptionGroupSingleton<MoleOptions>.Instance.VentVisibility is MoleVentVisibility.Immediate
    };
}

public enum MoleVentUsage
{
    Crewmates,
    Anyone,
    Mole
}

public enum MoleVentVisibility
{
    Immediate,
    AfterUse,
    AfterNextMeeting
}
