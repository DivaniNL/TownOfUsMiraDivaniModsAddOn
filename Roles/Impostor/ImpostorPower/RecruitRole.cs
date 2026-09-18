using Il2CppInterop.Runtime.Attributes;
using System;
using AmongUs.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Hud;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using DivaniMods.Assets;
using DivaniMods.Modifiers.Impostor;
using TownOfUs.Assets;
using TownOfUs.Extensions;
using TownOfUs.Interfaces;
using TownOfUs.Modules.Wiki;
using TownOfUs.Roles;
using TownOfUs.Utilities;
using UnityEngine;
using DivaniMods.Interfaces;

namespace DivaniMods.Roles.Impostor.ImpostorPower;

public sealed class RecruitRole(IntPtr cppPtr)
    : ImpostorRole(cppPtr), IDivaniRole, IWikiDiscoverable, IDoomable, ISpawnChange, IGuessable
{
    public bool CanBeGuessed =>
        RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<RecruiterRole>()) is ICustomRole recruiter &&
        (int)recruiter.GetCount()! > 0 && (int)recruiter.GetChance()! > 0;

    public string LocaleKey => "Recruit";
    public string RoleName => MiraLocaleManager.Get("DivaniMods.Role.Recruit", "Recruit");
    public string RoleDescription => MiraLocaleManager.Get("DivaniMods.Role.Recruit.Description");
    public string RoleMedDescription => MiraLocaleManager.Get("DivaniMods.Role.Recruit.MedDescription");
    public string RoleLongDescription => MiraLocaleManager.Get("DivaniMods.Role.Recruit.LongDescription");
    public Color RoleColor => Palette.ImpostorRed;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorPower;

    public DoomableType DoomHintType => DoomableType.Trickster;

    public bool NoSpawn => true;

    public string GetAdvancedDescription() => RoleLongDescription + MiscUtils.AppendOptionsText(GetType());

    [HideFromIl2Cpp] public List<RoleBehaviour> ChosenRoles { get; } = [];
    [HideFromIl2Cpp] public RoleBehaviour? RandomRole { get; set; }
    [HideFromIl2Cpp] public RoleBehaviour? SelectedRole { get; set; }

    [HideFromIl2Cpp] public List<CustomButtonWikiDescription> Abilities { get; } =
    [
        new(
            MiraLocaleManager.Get("DivaniMods.Role.Recruit.Ability.ChangeRole"),
            MiraLocaleManager.Get("DivaniMods.Role.Recruit.Ability.ChangeRole.Description"),
            TouImpAssets.TraitorSelect
        )
    ];

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = MiraAPI.Utilities.Assets.TmpSpriteUtils.CreateSpriteAsset(DivaniAssets.RecruitIcon.LoadAsset(), "DivaniMod.Role.Impostor.Recruit", 1.45f),
        Icon = DivaniAssets.RecruitIcon,
        HideSettings = true,
        CanModifyChance = false,
        DefaultChance = 0,
        DefaultRoleCount = 0,
        MaxRoleCount = 0,
        ShowInFreeplay = true,
    };

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);

        if (Player.AmOwner)
        {
            ButtonResetPatches.ResetCooldowns();
            Player.SetKillTimer(Player.GetKillCooldown());
        }
    }

    public void Clear()
    {
        ChosenRoles.Clear();
        SelectedRole = null;
    }

    public void UpdateRole()
    {
        if (!SelectedRole)
        {
            return;
        }

        var currentTime = Player.killTimer;

        var roleType = RoleId.Get(SelectedRole!.GetType());
        Player.RpcChangeRole(roleType, false);
        Player.RpcAddModifier<RecruitCacheModifier>();
        SelectedRole = null;

        Player.SetKillTimer(currentTime);
    }
}
