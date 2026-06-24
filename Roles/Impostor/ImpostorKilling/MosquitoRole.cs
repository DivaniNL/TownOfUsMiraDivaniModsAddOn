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

public sealed class MosquitoRole(IntPtr cppPtr)
    : ImpostorRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public string RoleName => "Mosquito";
    public string LocaleKey => "Mosquito";
    public string RoleDescription => "Bzzzzz..Splat!";
    public string RoleLongDescription =>
        "Launch a mosquito that flies to a target and stings it to death.\n" +
        "Everyone can click/tap the mosquitos to swat them them!";
    public Color RoleColor => Palette.ImpostorRed;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorKilling;

    public DoomableType DoomHintType => DoomableType.Hunter;

    public string GetAdvancedDescription() => RoleLongDescription + MiscUtils.AppendOptionsText(GetType());

    [HideFromIl2Cpp] public List<CustomButtonWikiDescription> Abilities { get; } =
    [
        new("Sting", "Launch a mosquito that flies to a target and stings it to death. The mosquitos can be swatted by clicking/tapping them", DivaniAssets.MosquitoStingButton)
    ];

    public CustomRoleConfiguration Configuration => new(this)
    {
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
