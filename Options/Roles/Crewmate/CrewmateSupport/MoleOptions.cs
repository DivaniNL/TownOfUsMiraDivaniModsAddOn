using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using DivaniMods.Roles.Crewmate.CrewmateSupport;

namespace DivaniMods.Options;

public sealed class MoleOptions : AbstractOptionGroup<MoleRole>
{
    public override string GroupName => "Mole";

    [ModdedNumberOption("Dig Cooldown", 5f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float DigCooldown { get; set; } = 25f;

    [ModdedNumberOption("Max Number Of Mole Vents Per Game", 1f, 6f, 1f, MiraNumberSuffixes.None)]
    public float MaxVents { get; set; } = 4f;

    public ModdedToggleOption EarnMoreVents { get; } = new("Earn More Vents After Tasks", false);

    public ModdedNumberOption TasksPerVent { get; } =
        new("Tasks Required For Additional Vent", 3f, 1f, 5f, 1f, MiraNumberSuffixes.None)
        {
            Visible = () => OptionGroupSingleton<MoleOptions>.Instance.EarnMoreVents
        };

    public ModdedNumberOption VentTimeLimit { get; } =
        new("Time In Mole Vents Before Kick Out", 10f, 1f, 15f, 1f, MiraNumberSuffixes.Seconds);

    public ModdedNumberOption VentCooldown { get; } =
        new("Mole Vent Cooldown", 10f, 1f, 15f, 1f, MiraNumberSuffixes.Seconds);

    [ModdedNumberOption("Rounds Mole Vents Last", 0f, 15f, 1f, MiraNumberSuffixes.None, "0", true)]
    public float VentRoundDuration { get; set; } = 2f;

    [ModdedEnumOption("Who Can Use Mole Vents", typeof(MoleVentUsage), ["Crewmates", "Anyone", "Mole"])]
    public MoleVentUsage VentUsage { get; set; } = MoleVentUsage.Anyone;

    [ModdedEnumOption("Vent Visibility", typeof(MoleVentVisibility), ["Immediate", "After Use", "After Next Meeting"])]
    public MoleVentVisibility VentVisibility { get; set; } = MoleVentVisibility.Immediate;

    public ModdedNumberOption DigDelay { get; } = new("Dig Delay", 3f, 0f, 10f, 0.5f, MiraNumberSuffixes.Seconds)
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
