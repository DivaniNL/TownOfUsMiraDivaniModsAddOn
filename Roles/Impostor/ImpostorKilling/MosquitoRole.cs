using Il2CppInterop.Runtime.Attributes;
using System;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Translation;
using DivaniMods.Assets;
using TownOfUs.Modules.Wiki;
using TownOfUs.Roles;
using TownOfUs.Utilities;
using UnityEngine;
using TownOfUs.Extensions;
using DivaniMods.Interfaces;


namespace DivaniMods.Roles.Impostor.ImpostorKilling;

public sealed class MosquitoRole(IntPtr cppPtr)
    : ImpostorRole(cppPtr), IDivaniRole, IWikiDiscoverable, IDoomable
{
   public string RoleName => MiraLocaleManager.Get("DivaniMods.Role.Mosquito", "Mosquito");
    public string LocaleKey => "Mosquito";
    public string RoleDescription => MiraLocaleManager.Get("DivaniMods.Role.Mosquito.Description");
    public string RoleMedDescription => MiraLocaleManager.Get("DivaniMods.Role.Mosquito.MedDescription");
    public string RoleLongDescription => MiraLocaleManager.Get("DivaniMods.Role.Mosquito.LongDescription");
    public Color RoleColor => Palette.ImpostorRed;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorKilling;

    public DoomableType DoomHintType => DoomableType.Hunter;

    public string GetAdvancedDescription() => RoleLongDescription + MiscUtils.AppendOptionsText(GetType());

    [HideFromIl2Cpp] public List<CustomButtonWikiDescription> Abilities { get; } =
    [
        new(
            MiraLocaleManager.Get("DivaniMods.Role.Mosquito.Ability.Sting"),
            MiraLocaleManager.Get("DivaniMods.Role.Mosquito.Ability.Sting.Description"),
            DivaniAssets.MosquitoStingButton
        )
    ];

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = MiraAPI.Utilities.Assets.TmpSpriteUtils.CreateSpriteAsset(DivaniAssets.MosquitoIcon.LoadAsset(), "DivaniMod.Role.Impostor.Mosquito", 1.45f),
        OptionsScreenshot = DivaniAssets.MosquitoBanner,
        UseVanillaKillButton = false,
        Icon = DivaniAssets.MosquitoIcon,
        IntroSound = DivaniAssets.MosquitoIntroSound,
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
