using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using DivaniMods.Assets;
using DivaniMods.Events.Neutral.NeutralBenign;
using DivaniMods.Options;
using TownOfUs;
using TownOfUs.Assets;
using TownOfUs.Interfaces;
using TownOfUs.Modules.Localization;
using TownOfUs.Modules.Wiki;
using TownOfUs.Roles;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;
using TownOfUs.Extensions;

namespace DivaniMods.Roles.Neutral.NeutralBenign;

public sealed class CatRole(IntPtr cppPtr)
    : NeutralRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable, ICrewVariant
{
    public static readonly Color CatColor = new Color32(222, 125, 127, 255);



    public string LocaleKey => "Schrodinger's Cat";
    public string RoleName => "Schrodinger's Cat";
    public string RoleDescription => "Use nine lives as an advantage!";
    public string RoleLongDescription =>
        "Use your nine lives as a Cat to survive any attacks, turning into your Attacker's role!\n" + "Win as your new role with your Attacker!";

    public Color RoleColor => CatColor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleAlignment RoleAlignment => RoleAlignment.NeutralBenign;

    public DoomableType DoomHintType => DoomableType.Protective;

    public RoleBehaviour CrewVariant => RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<Role>());

    public string GetAdvancedDescription() => RoleLongDescription + MiscUtils.AppendOptionsText(GetType());

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = MiraAPI.Utilities.Assets.TmpSpriteUtils.CreateSpriteAsset(DivaniAssets.CatIcon.LoadAsset(), "DivaniMod.Role.Neutral.Cat", 1.45f),
        Icon = DivaniAssets.CatIcon,
        IntroSound = DivaniAssets.CatIntroSound,
        MaxRoleCount = 1,
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>()
    };
    [HideFromIl2Cpp] public List<CustomButtonWikiDescription> Abilities { get; } =
    [
        new("Nine Lives", "Upon the first kill, it gets blocked and you turn into their role.", DivaniAssets.CatIcon)
    ];

    public override void SpawnTaskHeader(PlayerControl playerControl)
    {
        if (playerControl != PlayerControl.LocalPlayer)
        {
            return;
        }
        var task = PlayerTask.GetOrCreateTask<ImportantTextTask>(playerControl, 0);
        task.Text = $"{TownOfUsColors.Neutral.ToTextColor()}{TouLocale.GetParsed("NeutralBenignTaskHeader")}</color>";
        task.name = "NeutralRoleText";
    }
}

