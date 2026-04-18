using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using DivaniMods.Roles;

namespace DivaniMods.Options;

public class ThiefOptions : AbstractOptionGroup<ThiefRole>
{
    public override string GroupName => "Thief";

    [ModdedNumberOption("Max Stolen Modifiers", 1, 5, 1)]
    public float MaxStolenModifiers { get; set; } = 2;
    
    [ModdedNumberOption("Pickpocket Cooldown", 10, 60, 5, MiraNumberSuffixes.Seconds)]
    public float PickpocketCooldown { get; set; } = 25;
    
    [ModdedNumberOption("Pickpocket Range", 0.5f, 3f, 0.25f, MiraNumberSuffixes.Multiplier)]
    public float PickpocketRange { get; set; } = 1f;
}
