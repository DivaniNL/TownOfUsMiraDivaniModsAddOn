using System;
using AmongUs.GameOptions;
using MiraAPI.GameOptions;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using DivaniMods.Assets;
using DivaniMods.Options;
using TownOfUs.Modules.Wiki;
using TownOfUs.Roles;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;
using TownOfUs.Extensions;

namespace DivaniMods.Roles.Impostor.ImpostorConcealing;

public sealed class CunctatorRole(IntPtr cppPtr)
    : ImpostorRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable, ICrewVariant
{
    public string RoleName => "Cunctator";
    public string LocaleKey => "Cunctator";
    public string RoleDescription => "Delay Bodies!";
    public string RoleLongDescription =>
        "Bodies of those you kill only appear after a delay.";
    public Color RoleColor => Palette.ImpostorRed;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorConcealing;

    public DoomableType DoomHintType => DoomableType.Perception;

    public RoleBehaviour CrewVariant =>
        RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<EngineerTouRole>());

    public string GetAdvancedDescription()
    {
        var delay = OptionGroupSingleton<CunctatorOptions>.Instance?.BodyDelay?.Value;
        var delayText = delay.HasValue
            ? $"\n\nBodies appear after {delay.Value:0}s."
            : string.Empty;
        return RoleLongDescription + delayText + MiscUtils.AppendOptionsText(GetType());
    }

    public CustomRoleConfiguration Configuration => new(this)
    {
        OptionsScreenshot = DivaniAssets.CunctatorBanner,
        Icon = DivaniAssets.CunctatorIcon,
        IntroSound = DivaniAssets.CunctatorIntroSound,
        MaxRoleCount = 1,
    };
}
