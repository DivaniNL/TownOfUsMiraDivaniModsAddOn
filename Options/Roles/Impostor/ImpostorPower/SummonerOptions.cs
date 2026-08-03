using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using DivaniMods.Roles.Impostor.ImpostorPower;

namespace DivaniMods.Options;

public class SummonerOptions : AbstractRoleOptionGroup<SummonerRole>
{
    public override string GroupName => "Summoner";

    public ModdedNumberOption KillsRequiredForSummon { get; } = new(
        "Kills Required For Summon", 3f, 1f, 5f, 1f, MiraNumberSuffixes.None);

    public ModdedNumberOption RevenantKillCooldown { get; } = new(
        "Revenant Kill Cooldown", 25f, 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds);

    public ModdedNumberOption RevenantVentCooldown { get; } = new(
        "Revenant Vent Cooldown", 20f, 5f, 60f, 2.5f, MiraNumberSuffixes.Seconds);

    public ModdedNumberOption RevenantMaxVentTime { get; } = new(
        "Revenant Max Vent Time", 10f, 2f, 30f, 1f, MiraNumberSuffixes.Seconds);

    public ModdedToggleOption RevenantSeesRoles { get; set; } = new("Revenant Sees Living Roles", false);

}
