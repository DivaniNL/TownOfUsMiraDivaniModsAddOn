using Il2CppInterop.Runtime.Attributes;
using System;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using DivaniMods.Assets;
using TownOfUs.Modules.Wiki;
using TownOfUs.Roles;
using TownOfUs.Utilities;
using UnityEngine;
using TownOfUs.Extensions;


namespace DivaniMods.Roles.Impostor.ImpostorKilling;

public sealed class DeathnoteRole(IntPtr cppPtr)
    : ImpostorRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public string RoleName => "Deathnote";
    public string LocaleKey => "Deathnote";
    public string RoleDescription => "Collect my pages!";
    public string RoleLongDescription =>
        "Note players, their name will be in your book.\n" + 
        "they will succumb with you when you die.";
    public Color RoleColor => Palette.ImpostorRed;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorKilling;

    public DoomableType DoomHintType => DoomableType.Hunter;

    public string GetAdvancedDescription() => RoleLongDescription + MiscUtils.AppendOptionsText(GetType());

    [HideFromIl2Cpp] public List<CustomButtonWikiDescription> Abilities { get; } =
    [
        new("Note", "Note down a player in your note book, they will die when you are killed.", DivaniAssets.NoteButton)
    ];

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = MiraAPI.Utilities.Assets.TmpSpriteUtils.CreateSpriteAsset(DivaniAssets.DeathnoteIcon.LoadAsset(), "DivaniMod.Role.Impostor.Deathnote", 1.45f),
        UseVanillaKillButton = true,
        Icon = DivaniAssets.DeathnoteIcon,
        IntroSound = DivaniAssets.DeadlockIntroSound,
        MaxRoleCount = 1,
    };

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);
    }
}
