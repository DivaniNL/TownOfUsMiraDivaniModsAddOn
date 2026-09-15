using Il2CppInterop.Runtime.Attributes;
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

namespace DivaniMods.Roles.Impostor.ImpostorSupport;

public sealed class DeadlockRole(IntPtr cppPtr)
    : ImpostorRole(cppPtr), IDivaniRole, IWikiDiscoverable, IDoomable, ICrewVariant
{
    public string RoleName => MiraLocaleManager.Get("DivaniMods.Role.Deadlock", "Deadlock");
    public string LocaleKey => "Deadlock";
    public string RoleDescription => MiraLocaleManager.Get("DivaniMods.Role.Deadlock.Description");
    public string RoleMedDescription => MiraLocaleManager.Get("DivaniMods.Role.Deadlock.MedDescription");
    public string RoleLongDescription => MiraLocaleManager.Get("DivaniMods.Role.Deadlock.LongDescription");
    public Color RoleColor => Palette.ImpostorRed;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorSupport;

    public DoomableType DoomHintType => DoomableType.Fearmonger;

    public RoleBehaviour CrewVariant =>
        RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<EngineerTouRole>());

    public string GetAdvancedDescription() => RoleLongDescription + MiscUtils.AppendOptionsText(GetType());

    [HideFromIl2Cpp] public List<CustomButtonWikiDescription> Abilities { get; } =
    [
        new(
            MiraLocaleManager.Get("DivaniMods.Role.Deadlock.Ability.Lockdown"),
            MiraLocaleManager.Get("DivaniMods.Role.Deadlock.Ability.Lockdown.Description"),
            DivaniAssets.DeadlockLockdownButton
        )
    ];

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = MiraAPI.Utilities.Assets.TmpSpriteUtils.CreateSpriteAsset(DivaniAssets.DeadlockIcon.LoadAsset(), "DivaniMod.Role.Impostor.Deadlock", 1.45f),
        OptionsScreenshot = DivaniAssets.DeadlockBanner,
        Icon = DivaniAssets.DeadlockIcon,
        IntroSound = DivaniAssets.DeadlockIntroSound,
        MaxRoleCount = 1,
    };
}
