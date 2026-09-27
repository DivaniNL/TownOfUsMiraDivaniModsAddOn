using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using DivaniMods.Roles.Impostor.ImpostorConcealing;

namespace DivaniMods.Options;

public class CunctatorOptions : AbstractRoleOptionGroup<CunctatorRole>
{
    public override string GroupName => MiraLocaleManager.Get("DivaniMods.Role.Cunctator", "Cunctator");

    public ModdedNumberOption BodyDelay { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.Cunctator.BodyDelay"), 10f, 1f, 30f, 1f, MiraNumberSuffixes.Seconds);
}
