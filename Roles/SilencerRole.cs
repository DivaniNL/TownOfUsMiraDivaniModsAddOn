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

namespace DivaniMods.Roles;

public sealed class SilencerRole(IntPtr cppPtr)
    : ImpostorRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable, ICrewVariant
{
    public string RoleName => "Silencer";
    public string RoleDescription => "Cut meeting voting time with every kill!";
    public string RoleLongDescription =>
        "Each kill you make shaves seconds\n" +
        "off the voting time of every meeting for the rest of the game.";
    public Color RoleColor => Palette.ImpostorRed;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorKilling;

    // Doomsayer hint category: shortening meetings is a deception/manipulation
    // theme, same bucket as Blackmailer/Janitor.
    public DoomableType DoomHintType => DoomableType.Insight;

    // Imitator: closest crew analogue is the Engineer (no obvious crew mirror
    // for a meeting-shortener; matches Deadlock's pick).
    public RoleBehaviour CrewVariant =>
        RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<EngineerTouRole>());

    public string GetAdvancedDescription() => RoleLongDescription + MiscUtils.AppendOptionsText(GetType());

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = DivaniAssets.SilencerIcon,
        IntroSound = DivaniAssets.SilencerIntroSound,
        MaxRoleCount = 1,
    };
}
