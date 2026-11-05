using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Translation;
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

public enum ShockShieldVisibility
{
    Mage,
    MageAndTarget,
    Teammates,
}

public class MageOptions : AbstractRoleOptionGroup<MageRole>
{
    public override string GroupName => MiraLocaleManager.Get("DivaniMods.Role.Mage", "Mage");

    public ModdedNumberOption SpellCooldown { get; } =
        new(MiraLocaleManager.Get("DivaniMods.Options.Mage.SpellCooldown"), 25f, 10f, 90f, 2.5f, MiraNumberSuffixes.Seconds);

    public ModdedNumberOption MaxShockShieldUses { get; } =
        new(MiraLocaleManager.Get("DivaniMods.Options.Mage.MaxShockShieldUses"), 3f, 0f, 15f, 1f, "∞", "∞", MiraNumberSuffixes.None, "0");

    public ModdedNumberOption MaxEnergizeUses { get; } =
        new(MiraLocaleManager.Get("DivaniMods.Options.Mage.MaxEnergizeUses"), 3f, 0f, 15f, 1f, "∞", "∞", MiraNumberSuffixes.None, "0");

    public ModdedNumberOption MaxIllusionUses { get; } =
        new(MiraLocaleManager.Get("DivaniMods.Options.Mage.MaxIllusionUses"), 3f, 0f, 15f, 1f, "∞", "∞", MiraNumberSuffixes.None, "0");

    public ModdedNumberOption ShockShieldDuration { get; } =
        new(MiraLocaleManager.Get("DivaniMods.Options.Mage.ShockShieldDuration"), 12.5f, 5f, 30f, 2.5f, MiraNumberSuffixes.Seconds);

    public ModdedEnumOption ShockShieldVisibleTo { get; } =
        new(MiraLocaleManager.Get("DivaniMods.Options.Mage.ShockShieldVisibleTo"), (int)ShockShieldVisibility.MageAndTarget, typeof(ShockShieldVisibility),
            [
            MiraLocaleManager.Get("DivaniMods.Options.Mage.ShockShieldVisibleTo.Mage"),
            MiraLocaleManager.Get("DivaniMods.Options.Mage.ShockShieldVisibleTo.MageAndTarget"),
            MiraLocaleManager.Get("DivaniMods.Options.Mage.ShockShieldVisibleTo.Teammates")
            ]
        );

    public ModdedToggleOption MageNotifiedOnAttack { get; } =
        new(MiraLocaleManager.Get("DivaniMods.Options.Mage.MageNotifiedOnAttack"), true);

    public ModdedToggleOption ShockShieldReactsToInteractions { get; } =
        new(MiraLocaleManager.Get("DivaniMods.Options.Mage.ShockShieldReactsToInteractions"), true);

    public ModdedEnumOption EnergizeApplyTiming { get; } =
        new(MiraLocaleManager.Get("DivaniMods.Options.Mage.EnergizeApplyTiming"), (int)EnergizeTiming.AfterMeeting, typeof(EnergizeTiming), 
        [
            MiraLocaleManager.Get("DivaniMods.Options.Mage.EnergizeApplyTiming.AfterDelay"),
            MiraLocaleManager.Get("DivaniMods.Options.Mage.EnergizeApplyTiming.AfterMeeting")
        ]
        );

    public ModdedEnumOption EnergizeNeutralBenign { get; } =
        new(MiraLocaleManager.Get("DivaniMods.Options.Mage.EnergizeNeutralBenign"), (int)EnergizeNeutralBenignMode.Nerf, typeof(EnergizeNeutralBenignMode), 
        [
            MiraLocaleManager.Get("DivaniMods.Options.Mage.EnergizeNeutralBenign.Removed"),
            MiraLocaleManager.Get("DivaniMods.Options.Mage.EnergizeNeutralBenign.Added"),
            MiraLocaleManager.Get("DivaniMods.Options.Mage.EnergizeNeutralBenign.Ignored")
        ]
        );

    public ModdedNumberOption EnergizeDelay { get; } =
        new(MiraLocaleManager.Get("DivaniMods.Options.Mage.EnergizeDelay"), 3f, 0f, 15f, 0.5f, MiraNumberSuffixes.Seconds)
        {
            Visible = () => OptionGroupSingleton<MageOptions>.Instance.EnergizeApplyTiming.Value == (int)EnergizeTiming.AfterDelay,
        };

    public ModdedNumberOption IllusionDuration { get; } =
        new(MiraLocaleManager.Get("DivaniMods.Options.Mage.IllusionDuration"), 12.5f, 5f, 30f, 2.5f, MiraNumberSuffixes.Seconds);

    public ModdedToggleOption IllusionTargetKnows { get; } =
        new(MiraLocaleManager.Get("DivaniMods.Options.Mage.IllusionTargetKnows"), false);

    public ModdedToggleOption CrewKillingSeesIllusioned { get; } =
        new(MiraLocaleManager.Get("DivaniMods.Options.Mage.CrewKillingSeesIllusioned"), true);

    public ModdedToggleOption NeutralEvilSeesIllusioned { get; } =
        new(MiraLocaleManager.Get("DivaniMods.Options.Mage.NeutralEvilSeesIllusioned"), true);

    public ModdedToggleOption NeutralBenignSeesIllusioned { get; } =
        new(MiraLocaleManager.Get("DivaniMods.Options.Mage.NeutralBenignSeesIllusioned"), true);
}
