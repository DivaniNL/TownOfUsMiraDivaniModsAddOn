using System;
using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using MiraAPI.Translation;
using DivaniMods.Assets;
using DivaniMods.Interfaces;
using TownOfUs.Assets;
using TownOfUs.Modules.Wiki;
using TownOfUs.Roles;
using TownOfUs.Utilities;
using UnityEngine;
using TownOfUs.Extensions;

namespace DivaniMods.Roles.Crewmate.CrewmateSupport;

public sealed class LocatorRole(IntPtr cppPtr)
    : CrewmateRole(cppPtr), IDivaniRole, IWikiDiscoverable, IDoomable
{
    public static readonly Color LocatorColor = new Color32(0xDD, 0xAB, 0x99, 255);

    public static int MarksRemaining { get; set; }
    public static int MarksThisRound { get; set; }

    public string RoleName => MiraLocaleManager.Get("DivaniMods.Role.Locator", "Locator");
    public string RoleDescription => MiraLocaleManager.Get("DivaniMods.Role.Locator.Description");
    public string RoleMedDescription => MiraLocaleManager.Get("DivaniMods.Role.Locator.MedDescription");
    public string RoleLongDescription => MiraLocaleManager.Get("DivaniMods.Role.Locator.LongDescription");
    public Color RoleColor => LocatorColor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateSupport;

    public DoomableType DoomHintType => DoomableType.Trickster;

    public string GetAdvancedDescription() => RoleLongDescription + MiscUtils.AppendOptionsText(GetType());

    [HideFromIl2Cpp] public List<CustomButtonWikiDescription> Abilities { get; } =
    [
        new(
            MiraLocaleManager.Get("DivaniMods.Role.Locator.Ability.Mark"),
            MiraLocaleManager.Get("DivaniMods.Role.Locator.Ability.Mark.Description"),
            DivaniAssets.LocatorIcon
        )
    ];

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = MiraAPI.Utilities.Assets.TmpSpriteUtils.CreateSpriteAsset(DivaniAssets.LocatorIcon.LoadAsset(), "DivaniMod.Role.Crewmate.Locator", 1.45f),
        Icon = DivaniAssets.LocatorIcon,
        OptionsScreenshot = TouBanners.CrewmateRoleBanner,
        IntroSound = TouAudio.NoisemakerIntroSound,
        MaxRoleCount = 1,
    };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var stringB = ITownOfUsRole.SetNewTabText(this);
        stringB.AppendLine($"{RoleColor.ToTextColor()}<b>{MiraLocaleManager.Get("DivaniMods.Role.Locator.Tab.MarksLeft")
        .Replace("<count>", MarksRemaining.ToString())}</b></color>");
        return stringB;
    }
}
