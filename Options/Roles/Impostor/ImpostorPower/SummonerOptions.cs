using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using DivaniMods.Roles.Impostor.ImpostorPower;

namespace DivaniMods.Options;

public class SummonerOptions : AbstractRoleOptionGroup<SummonerRole>
{
    public override string GroupName => MiraLocaleManager.Get("DivaniMods.Role.Summoner", "Summoner");
    public ModdedNumberOption KillsRequiredForSummon { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.Summoner.KillsRequiredForSummon"), 3f, 1f, 5f, 1f, MiraNumberSuffixes.None);

    public ModdedNumberOption RevenantKillCooldown { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.Summoner.RevenantKillCooldown"), 25f, 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds);

    public ModdedNumberOption RevenantVentCooldown { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.Summoner.RevenantVentCooldown"), 20f, 5f, 60f, 2.5f, MiraNumberSuffixes.Seconds);

    public ModdedNumberOption RevenantMaxVentTime { get; } = new(
        MiraLocaleManager.Get("DivaniMods.Options.Summoner.RevenantMaxVentTime"), 10f, 2f, 30f, 1f, MiraNumberSuffixes.Seconds);

    public ModdedToggleOption RevenantSeesRoles { get; set; } = new(MiraLocaleManager.Get("DivaniMods.Options.Summoner.RevenantSeesRoles"), false);

}
