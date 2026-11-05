using Il2CppInterop.Runtime.Attributes;
using System;
using MiraAPI.Roles;
using MiraAPI.Translation;
using DivaniMods.Assets;
using DivaniMods.Interfaces;
using TownOfUs.Extensions;
using TownOfUs.Modules.Wiki;
using TownOfUs.Roles;
using TownOfUs.Utilities;
using UnityEngine;

namespace DivaniMods.Roles.Crewmate.CrewmateInvestigative;

public sealed class SentinelRole(IntPtr cppPtr)
    : CrewmateRole(cppPtr), IDivaniRole, IWikiDiscoverable, IDoomable
{
    public static readonly Color SentinelColor = new Color32(244, 169, 60, 255);

    public string RoleName => MiraLocaleManager.Get("DivaniMods.Role.Sentinel", "Sentinel");
    public string RoleDescription => MiraLocaleManager.Get("DivaniMods.Role.Sentinel.Description");
    public string RoleMedDescription => MiraLocaleManager.Get("DivaniMods.Role.Sentinel.MedDescription");
    public string RoleLongDescription => MiraLocaleManager.Get("DivaniMods.Role.Sentinel.LongDescription"); 
    public Color RoleColor => SentinelColor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateInvestigative;

    public DoomableType DoomHintType => DoomableType.Insight;

    public string GetAdvancedDescription() => RoleLongDescription + MiscUtils.AppendOptionsText(GetType());

    [HideFromIl2Cpp] public List<CustomButtonWikiDescription> Abilities { get; } =
    [
        new(
            MiraLocaleManager.Get("DivaniMods.Role.Sentinel.Ability.PlaceBeacon"),
            MiraLocaleManager.Get("DivaniMods.Role.Sentinel.Ability.PlaceBeacon.Description"),
            DivaniAssets.SentinelPlaceBeaconButton
        )
    ];

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = MiraAPI.Utilities.Assets.TmpSpriteUtils.CreateSpriteAsset(DivaniAssets.SentinelIcon.LoadAsset(), "DivaniMod.Role.Crewmate.Sentinel", 1.45f),
        OptionsScreenshot = DivaniAssets.SentinelBanner,
        Icon = DivaniAssets.SentinelIcon,
        IntroSound = DivaniAssets.SentinelIntroSound,
        MaxRoleCount = 1,
    };
}
