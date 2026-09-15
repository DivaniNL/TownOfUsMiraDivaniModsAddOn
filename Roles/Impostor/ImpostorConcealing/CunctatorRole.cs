using System;
using AmongUs.GameOptions;
using MiraAPI.GameOptions;
using MiraAPI.Roles;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using DivaniMods.Assets;
using DivaniMods.Options;
using TownOfUs.Modules.Wiki;
using TownOfUs.Roles;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;
using TownOfUs.Extensions;
using DivaniMods.Interfaces;

namespace DivaniMods.Roles.Impostor.ImpostorConcealing;

public sealed class CunctatorRole(IntPtr cppPtr)
    : ImpostorRole(cppPtr), IDivaniRole, IWikiDiscoverable, IDoomable, ICrewVariant
{
    public string RoleName => MiraLocaleManager.Get("DivaniMods.Role.Cunctator", "Cunctator");
    public string LocaleKey => "Cunctator";
    public string RoleDescription => MiraLocaleManager.Get("DivaniMods.Role.Cunctator.Description");
    public string RoleMedDescription => MiraLocaleManager.Get("DivaniMods.Role.Cunctator.MedDescription");
    public string RoleLongDescription => MiraLocaleManager.Get("DivaniMods.Role.Cunctator.LongDescription");
    public Color RoleColor => Palette.ImpostorRed;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorConcealing;

    public DoomableType DoomHintType => DoomableType.Perception;

    public RoleBehaviour CrewVariant =>
        RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<AltruistRole>());

    public string GetAdvancedDescription()
    {
        var delay = OptionGroupSingleton<CunctatorOptions>.Instance?.BodyDelay?.Value;
        var delayText = delay.HasValue
            ? "\n\n" + MiraLocaleManager.Get("DivaniMods.Role.Cunctator.Advanced.BodyDelay")
                .Replace("<seconds>", delay.Value.ToString("0"))
            : string.Empty;

        return RoleLongDescription + delayText + MiscUtils.AppendOptionsText(GetType());
    }

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = MiraAPI.Utilities.Assets.TmpSpriteUtils.CreateSpriteAsset(DivaniAssets.CunctatorIcon.LoadAsset(), "DivaniMod.Role.Impostor.Cunctator", 1.45f),
        OptionsScreenshot = DivaniAssets.CunctatorBanner,
        Icon = DivaniAssets.CunctatorIcon,
        IntroSound = DivaniAssets.CunctatorIntroSound,
        MaxRoleCount = 1,
    };
}
