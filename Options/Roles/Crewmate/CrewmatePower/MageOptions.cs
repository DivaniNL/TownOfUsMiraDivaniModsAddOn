using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using DivaniMods.Roles.Crewmate.CrewmatePower;

namespace DivaniMods.Options;

public enum EnergizeTiming
{
    AfterDelay,
    AfterMeeting,
}

public enum EnergizeNeutralBenignMode
{
    Nerf,
    Buff,
    None,
}

public class MageOptions : AbstractOptionGroup<MageRole>
{
    public override string GroupName => "Mage";

    public ModdedNumberOption SpellCooldown { get; } =
        new("Spell Cooldown", 25f, 10f, 90f, 2.5f, MiraNumberSuffixes.Seconds);

    public ModdedNumberOption MaxShockShieldUses { get; } =
        new("Max Shock Shield Uses", 3f, 0f, 15f, 1f, "∞", "∞", MiraNumberSuffixes.None, "0");

    public ModdedNumberOption MaxEnergizeUses { get; } =
        new("Max Energize Uses", 3f, 0f, 15f, 1f, "∞", "∞", MiraNumberSuffixes.None, "0");

    public ModdedNumberOption MaxIllusionUses { get; } =
        new("Max Illusion Uses", 3f, 0f, 15f, 1f, "∞", "∞", MiraNumberSuffixes.None, "0");

    public ModdedNumberOption ShockShieldDuration { get; } =
        new("Shock Shield Duration", 12.5f, 5f, 30f, 2.5f, MiraNumberSuffixes.Seconds);

    public ModdedToggleOption TargetSeesShockShield { get; } =
        new("Target Sees Shock Shield", true);

    public ModdedToggleOption MageNotifiedOnAttack { get; } =
        new("Notify Mage When Target Attacked", true);

    public ModdedEnumOption EnergizeApplyTiming { get; } =
        new("Energize Applies", (int)EnergizeTiming.AfterMeeting, typeof(EnergizeTiming), ["After Delay", "After Meeting"]);

    public ModdedEnumOption EnergizeNeutralBenign { get; } =
        new("Neutral Benign role uses are", (int)EnergizeNeutralBenignMode.Nerf, typeof(EnergizeNeutralBenignMode), ["Removed", "Added", "Ignored"]);

    public ModdedNumberOption EnergizeDelay { get; } =
        new("Energize Delay", 3f, 0f, 15f, 0.5f, MiraNumberSuffixes.Seconds)
        {
            Visible = () => OptionGroupSingleton<MageOptions>.Instance.EnergizeApplyTiming.Value == (int)EnergizeTiming.AfterDelay,
        };

    public ModdedNumberOption IllusionDuration { get; } =
        new("Illusion Duration", 12.5f, 5f, 30f, 2.5f, MiraNumberSuffixes.Seconds);

    public ModdedToggleOption IllusionTargetKnows { get; } =
        new("Illusion Target Knows", false);

    public ModdedToggleOption CrewKillingSeesIllusioned { get; } =
        new("Crew Killing Sees Illusioned", true);

    public ModdedToggleOption NeutralEvilSeesIllusioned { get; } =
        new("Neutral Evil Sees Illusioned", true);

    public ModdedToggleOption NeutralBenignSeesIllusioned { get; } =
        new("Neutral Benign Sees Illusioned", true);
}
