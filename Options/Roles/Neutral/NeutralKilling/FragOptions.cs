using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using DivaniMods.Roles.Neutral.NeutralKilling;

namespace DivaniMods.Options;

public enum FragRewindBehavior
{
    Pause,
    Rewind
}

public class FragOptions : AbstractOptionGroup<FragRole>
{
    public override string GroupName => "Frag";

    public ModdedNumberOption BombTimer { get; } = new(
        "Frag Timer", 20f, 10f, 45f, 5f, MiraNumberSuffixes.Seconds);

    public ModdedNumberOption GiveBombCooldown { get; } = new(
        "Give Frag Cooldown", 25f, 10f, 60f, 5f, MiraNumberSuffixes.Seconds);

    public ModdedToggleOption CanVent { get; } = new("Frag Can Vent", false);

    public ModdedEnumOption OnTimelordRewind { get; } = new(
        "On Timelord Rewind", (int)FragRewindBehavior.Pause, typeof(FragRewindBehavior),
        ["Pause Timer", "Rewind Timer"]);
}
