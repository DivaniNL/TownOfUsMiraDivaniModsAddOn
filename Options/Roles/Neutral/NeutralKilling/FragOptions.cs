using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using DivaniMods.Roles.Neutral.NeutralKilling;

namespace DivaniMods.Options;

public enum FragRewindBehavior
{
    Pause,
    Rewind
}

public class FragOptions : AbstractRoleOptionGroup<FragRole>
{
    public override string GroupName => MiraLocaleManager.Get("DivaniMods.Role.Frag", "Frag");

    public ModdedNumberOption BombTimer { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.Frag.BombTimer"), 20f, 10f, 45f, 5f, MiraNumberSuffixes.Seconds);

    public ModdedNumberOption GiveBombCooldown { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.Frag.GiveBombCooldown"), 25f, 10f, 60f, 5f, MiraNumberSuffixes.Seconds);

    public ModdedToggleOption CanVent { get; } = new(MiraLocaleManager.Get("DivaniMods.Options.Frag.CanVent"), false);

    public ModdedToggleOption ClericCanDefuse { get; } = new(MiraLocaleManager.Get("DivaniMods.Options.Frag.ClericCanDefuse"), true);

    public ModdedToggleOption NegatesVeteranAlert { get; } = new(MiraLocaleManager.Get("DivaniMods.Options.Frag.NegatesVeteranAlert"), true);

    public ModdedEnumOption OnTimelordRewind { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.Frag.OnTimelordRewind"), (int)FragRewindBehavior.Pause, typeof(FragRewindBehavior),
        [
            MiraLocaleManager.Get("DivaniMods.Options.Frag.OnTimelordRewind.Pause"),
            MiraLocaleManager.Get("DivaniMods.Options.Frag.OnTimelordRewind.Rewind")
        ]
        );
}
