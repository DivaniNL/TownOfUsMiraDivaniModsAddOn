using Il2CppInterop.Runtime.Attributes;
using System;
using MiraAPI.GameOptions;
using MiraAPI.Roles;
using MiraAPI.Translation;
using DivaniMods.Assets;
using DivaniMods.Options;
using DivaniMods.Interfaces;
using TownOfUs.Extensions;
using TownOfUs.Modules.Wiki;
using TownOfUs.Roles;
using TownOfUs.Utilities;
using UnityEngine;

namespace DivaniMods.Roles.Crewmate.CrewmateSupport;

public sealed class PortalmakerRole(IntPtr cppPtr)
    : CrewmateRole(cppPtr), IDivaniRole, IWikiDiscoverable, IDoomable
{
    public string RoleName => MiraLocaleManager.Get("DivaniMods.Role.Portalmaker", "Portalmaker");
    public string RoleDescription => MiraLocaleManager.Get("DivaniMods.Role.Portalmaker.Description");
    public string RoleMedDescription => MiraLocaleManager.Get("DivaniMods.Role.Portalmaker.MedDescription");
    public string RoleLongDescription => MiraLocaleManager.Get("DivaniMods.Role.Portalmaker.LongDescription");
    public Color RoleColor => new Color(0.047f, 0.420f, 0.961f);
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateSupport;

    public DoomableType DoomHintType => DoomableType.Trickster;

    public string GetAdvancedDescription() => RoleLongDescription + MiscUtils.AppendOptionsText(GetType());

    [HideFromIl2Cpp] public List<CustomButtonWikiDescription> Abilities { get; } =
    [
        new(
        MiraLocaleManager.Get("DivaniMods.Role.Portalmaker.Ability.PlacePortal"),
        MiraLocaleManager.Get("DivaniMods.Role.Portalmaker.Ability.PlacePortal.Description"),
        DivaniAssets.PlacePortalButton
    ),
        new(
            MiraLocaleManager.Get("DivaniMods.Role.Portalmaker.Ability.UsePortal"),
            MiraLocaleManager.Get("DivaniMods.Role.Portalmaker.Ability.UsePortal.Description"),
            DivaniAssets.UsePortalButton
        )
    ];

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = MiraAPI.Utilities.Assets.TmpSpriteUtils.CreateSpriteAsset(DivaniAssets.PortalmakerIcon.LoadAsset(), "DivaniMod.Role.Crewmate.Portalmaker", 1.45f),
        OptionsScreenshot = DivaniAssets.PortalmakerBanner,
        Icon = DivaniAssets.PortalmakerIcon,
        IntroSound = DivaniAssets.PortalMakerIntroSound,
        MaxRoleCount = 1,
    };
}
