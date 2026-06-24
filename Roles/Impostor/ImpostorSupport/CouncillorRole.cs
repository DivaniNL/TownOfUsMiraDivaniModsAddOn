using Il2CppInterop.Runtime.Attributes;
using System;
using System.Text;
using AmongUs.GameOptions;
using MiraAPI.GameOptions;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using DivaniMods.Assets;
using DivaniMods.Events.Impostor.ImpostorSupport;
using TownOfUs;
using TownOfUs.Modules.Wiki;
using TownOfUs.Roles;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;
using TownOfUs.Extensions;

namespace DivaniMods.Roles.Impostor.ImpostorSupport;

public sealed class CouncillorRole(IntPtr cppPtr)
    : ImpostorRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable, ICrewVariant
{
    public string RoleName => "Councillor";
    public string RoleDescription => "Your vote? It's mine!";
    public string RoleLongDescription =>
        "Each kill gives extra votes in the next meeting.";
    public Color RoleColor => Palette.ImpostorRed;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorSupport;

    public DoomableType DoomHintType => DoomableType.Trickster;

    public RoleBehaviour CrewVariant =>
        RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<EngineerTouRole>());

    public string GetAdvancedDescription() => RoleLongDescription + MiscUtils.AppendOptionsText(GetType());

    [HideFromIl2Cpp] public List<CustomButtonWikiDescription> Abilities { get; } = [];

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = DivaniAssets.CouncillorIcon,
        IntroSound = DivaniAssets.CouncillorIntroSound,
        MaxRoleCount = 1,
    };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var stringB = ITownOfUsRole.SetNewTabText(this);
        var extra = CouncillorEvents.GetExtraVotes(Player.PlayerId);
        stringB.AppendLine(TownOfUsPlugin.Culture, $"<b>Extra votes next meeting: +{extra}</b>");
        return stringB;
    }
}
