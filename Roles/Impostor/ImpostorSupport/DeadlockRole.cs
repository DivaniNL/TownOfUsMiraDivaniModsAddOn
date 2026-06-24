using Il2CppInterop.Runtime.Attributes;
using System;
using AmongUs.GameOptions;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using DivaniMods.Assets;
using TownOfUs.Extensions;
using TownOfUs.Modules.Wiki;
using TownOfUs.Roles;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace DivaniMods.Roles.Impostor.ImpostorSupport;

public sealed class DeadlockRole(IntPtr cppPtr)
    : ImpostorRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable, ICrewVariant
{
    public string RoleName => "Deadlock";
    public string LocaleKey => "Deadlock";
    public string RoleDescription => "Disable tasks!";
    public string RoleLongDescription => "Use your Lockdown ability to temporarily\ndisable all crewmate tasks.\nDuring lockdown, crewmates cannot access\nor complete any tasks.";
    public Color RoleColor => Palette.ImpostorRed;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorSupport;

    public DoomableType DoomHintType => DoomableType.Fearmonger;

    public RoleBehaviour CrewVariant =>
        RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<EngineerTouRole>());

    public string GetAdvancedDescription() => RoleLongDescription + MiscUtils.AppendOptionsText(GetType());

    [HideFromIl2Cpp] public List<CustomButtonWikiDescription> Abilities { get; } =
    [
        new("Lockdown", "Temporarily disable all crewmate tasks.", DivaniAssets.DeadlockLockdownButton)
    ];

    public CustomRoleConfiguration Configuration => new(this)
    {
        OptionsScreenshot = DivaniAssets.DeadlockBanner,
        Icon = DivaniAssets.DeadlockIcon,
        IntroSound = DivaniAssets.DeadlockIntroSound,
        MaxRoleCount = 1,
    };
}
