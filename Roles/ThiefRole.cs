using System;
using System.Collections.Generic;
using MiraAPI.GameOptions;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using DivaniMods.Assets;
using DivaniMods.Options;
using TownOfUs.Extensions;
using TownOfUs.Modules.Wiki;
using TownOfUs.Roles;
using TownOfUs.Utilities;
using UnityEngine;

namespace DivaniMods.Roles;

public sealed class ThiefRole(IntPtr cppPtr)
    : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public string RoleName => "Thief";
    public string RoleDescription => "Steal modifiers from other players!";
    public string RoleLongDescription => "Use your Pickpocket ability to steal modifiers from nearby players. You can hold a limited number of stolen modifiers.";
    public Color RoleColor => new Color(0.5f, 0.3f, 0.1f);
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmatePower;

    // Doomsayer hint category: stealing/manipulation fits Trickster (same as Mayor/Swapper).
    public DoomableType DoomHintType => DoomableType.Trickster;

    public string GetAdvancedDescription() => RoleLongDescription + MiscUtils.AppendOptionsText(GetType());
    
    public List<uint> StolenModifierIds { get; } = new();
    
    public int MaxStolenModifiers => (int)OptionGroupSingleton<ThiefOptions>.Instance.MaxStolenModifiers;
    
    public bool CanStealMore => StolenModifierIds.Count < MaxStolenModifiers;

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = DivaniAssets.ThiefIcon,
        IntroSound = DivaniAssets.ThiefIntroSound,
    };
    
    public override void Initialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Initialize(this, targetPlayer);
        StolenModifierIds.Clear();
    }
    
    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);
        StolenModifierIds.Clear();
    }
}
