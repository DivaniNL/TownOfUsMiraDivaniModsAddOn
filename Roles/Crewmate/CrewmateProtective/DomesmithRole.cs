using Il2CppInterop.Runtime.Attributes;
using System;
using System.Collections.Generic;
using MiraAPI.Roles;
using MiraAPI.Translation;
using DivaniMods.Assets;
using DivaniMods.Interfaces;
using TownOfUs.Extensions;
using TownOfUs.Modules.Wiki;
using TownOfUs.Roles;
using TownOfUs.Utilities;
using UnityEngine;
using MiraAPI.Modifiers;
using TownOfUs.Modifiers.Game;

namespace DivaniMods.Roles.Crewmate.CrewmateProtective;

public sealed class DomesmithRole(IntPtr cppPtr)
    : CrewmateRole(cppPtr), IDivaniRole, IWikiDiscoverable, IDoomable
{
    public static readonly Color DomesmithColor = new Color32(0x0E, 0xAA, 0xC3, 255);

    public string RoleName => MiraLocaleManager.Get("DivaniMods.Role.Domesmith", "Domesmith");
    public string RoleDescription => MiraLocaleManager.Get("DivaniMods.Role.Domesmith.Description");
    public string RoleMedDescription => MiraLocaleManager.Get("DivaniMods.Role.Domesmith.MedDescription");
    public string RoleLongDescription =>
        PlayerControl.LocalPlayer
        && PlayerControl.LocalPlayer.TryGetModifier<AllianceGameModifier>(out var allyMod) && !allyMod.GetsPunished
            ? MiraLocaleManager.Get("DivaniMods.Role.Domesmith.LongDescription.Evil")
            : MiraLocaleManager.Get("DivaniMods.Role.Domesmith.LongDescription");
    
    public Color RoleColor => DomesmithColor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateProtective;

    public DoomableType DoomHintType => DoomableType.Protective;

    public string GetAdvancedDescription() => RoleLongDescription + MiscUtils.AppendOptionsText(GetType());

    [HideFromIl2Cpp] public List<CustomButtonWikiDescription> Abilities { get; } =
    [
        new(MiraLocaleManager.Get("DivaniMods.Role.Domesmith.Ability.PlaceDome"),
            MiraLocaleManager.Get("DivaniMods.Role.Domesmith.Ability.PlaceDome.Description"),
            DivaniAssets.DomesmithPlaceDomeButton
        )
    ];

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = MiraAPI.Utilities.Assets.TmpSpriteUtils.CreateSpriteAsset(DivaniAssets.DomesmithIcon.LoadAsset(), "DivaniMod.Role.Crewmate.Domesmith", 1.45f),
        OptionsScreenshot = DivaniAssets.DomesmithBanner,
        Icon = DivaniAssets.DomesmithIcon,
        IntroSound = DivaniAssets.DomesmithIntroSound,
        MaxRoleCount = 1,
    };
}
