using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using DivaniMods.Roles;

namespace DivaniMods.Options;

public class FragOptions : AbstractOptionGroup<FragRole>
{
    public override string GroupName => "Frag";

    [ModdedNumberOption("Bomb Timer", 10, 45, 5, MiraNumberSuffixes.Seconds)]
    public float BombTimer { get; set; } = 20;

    [ModdedNumberOption("Give Bomb Cooldown", 10, 60, 5, MiraNumberSuffixes.Seconds)]
    public float GiveBombCooldown { get; set; } = 25;
}
