using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using DivaniMods.Roles.Crewmate.CrewmatePower;

namespace DivaniMods.Options;

public class ThiefOptions : AbstractOptionGroup<ThiefRole>
{
    public override string GroupName => "Thief";

    [ModdedNumberOption("Max Stolen Modifiers", 1, 5, 1)]
    public float MaxStolenModifiers { get; set; } = 2;
    
    [ModdedNumberOption("Pickpocket Cooldown", 10, 60, 5, MiraNumberSuffixes.Seconds)]
    public float PickpocketCooldown { get; set; } = 25;

    [ModdedNumberOption("Pickpocket Duration", 1f, 10f, 0.5f, MiraNumberSuffixes.Seconds)]
    public float PickpocketDuration { get; set; } = 3f;

    [ModdedNumberOption("Pickpocket Range", 0.5f, 3f, 0.25f, MiraNumberSuffixes.Multiplier)]
    public float PickpocketRange { get; set; } = 1f;
    
    [ModdedToggleOption("Stealing Lover Breaks Their Heart")]
    public bool StealingLoverHeartbreaksVictim { get; set; } = true;
}
