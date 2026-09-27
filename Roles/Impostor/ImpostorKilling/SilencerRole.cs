using System;
using AmongUs.GameOptions;
using MiraAPI.Roles;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using DivaniMods.Assets;
using TownOfUs.Extensions;
using TownOfUs.Modules.Wiki;
using TownOfUs.Roles;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;
using DivaniMods.Interfaces;

namespace DivaniMods.Roles.Impostor.ImpostorKilling;

public sealed class SilencerRole(IntPtr cppPtr)
    : ImpostorRole(cppPtr), IDivaniRole, IWikiDiscoverable, IDoomable, ICrewVariant
{
    public string RoleName => MiraLocaleManager.Get("DivaniMods.Role.Silencer", "Silencer");
    public string LocaleKey => "Silencer";
    public string RoleDescription => MiraLocaleManager.Get("DivaniMods.Role.Silencer.Description");
    public string RoleMedDescription => MiraLocaleManager.Get("DivaniMods.Role.Silencer.MedDescription");
    public string RoleLongDescription => MiraLocaleManager.Get("DivaniMods.Role.Silencer.LongDescription");
    public Color RoleColor => Palette.ImpostorRed;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorKilling;

    public DoomableType DoomHintType => DoomableType.Fearmonger;

    public RoleBehaviour CrewVariant =>
        RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<EngineerTouRole>());

    public string GetAdvancedDescription() => RoleLongDescription + MiscUtils.AppendOptionsText(GetType());

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = MiraAPI.Utilities.Assets.TmpSpriteUtils.CreateSpriteAsset(DivaniAssets.SilencerIcon.LoadAsset(), "DivaniMod.Role.Impostor.Silencer", 1.45f),
        OptionsScreenshot = DivaniAssets.SilencerBanner,
        Icon = DivaniAssets.SilencerIcon,
        IntroSound = DivaniAssets.SilencerIntroSound,
        MaxRoleCount = 1,
    };
}
