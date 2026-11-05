using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using DivaniMods.Roles.Neutral.NeutralKilling;

namespace DivaniMods.Options;

public class ThiefOptions : AbstractRoleOptionGroup<ThiefRole>
{
    public override string GroupName => MiraLocaleManager.Get("DivaniMods.Role.Thief", "Thief");

    public ModdedNumberOption MaxStolenModifiers { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.Thief.MaxStolenModifiers"), 5f, 1f, 15f, 1f, MiraNumberSuffixes.None);

    public ModdedNumberOption KillCooldown { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.Thief.KillCooldown"), 25f, 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds, "0.0");

    public ModdedNumberOption PickpocketCooldown { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.Thief.PickpocketCooldown"), 25f, 10f, 60f, 5f, MiraNumberSuffixes.Seconds);

    public ModdedNumberOption PickpocketDuration { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.Thief.PickpocketDuration"), 3f, 1f, 10f, 0.5f, MiraNumberSuffixes.Seconds);

    [ModdedToggleOption("DivaniMods.Options.Thief.StealingLoverHeartbreaksVictim")]
    public bool StealingLoverHeartbreaksVictim { get; set; } = true;

    public ModdedToggleOption CanVent { get; } = new(MiraLocaleManager.Get("DivaniMods.Options.Thief.CanVent"), true);

    public ModdedToggleOption CanStealAllianceModifiers { get; } = new(MiraLocaleManager.Get("DivaniMods.Options.Thief.CanStealAllianceModifiers"), false);
}