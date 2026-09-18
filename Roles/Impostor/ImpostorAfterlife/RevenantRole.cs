using System;
using AmongUs.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using DivaniMods.Assets;
using TownOfUs.Modifiers;
using TownOfUs.Modules.Wiki;
using TownOfUs.Patches;
using TownOfUs.Roles;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using TownOfUs.Utilities.Appearances;
using UnityEngine;
using UnityEngine.UI;
using DivaniMods.Interfaces;

namespace DivaniMods.Roles.Impostor.ImpostorAfterlife;

public sealed class RevenantRole(IntPtr cppPtr)
    : ImpostorGhostRole(cppPtr), IDivaniRole, IGhostRole, IWikiDiscoverable
{
    public static readonly Color RevenantColor = new(0.78f, 0.05f, 0.05f, 1f);

    public bool Setup { get; set; }
    public bool Caught { get; set; }
    public bool Faded { get; set; }

    public bool CanBeClicked
    {
        get => GhostActive;
        set { }
    }

    public bool GhostActive => Setup && !Caught;

    public bool CanCatch()
    {
        var local = PlayerControl.LocalPlayer;
        return local != null && !local.IsImpostorAligned();
    }

    public string LocaleKey => "Revenant";
    public string RoleName => MiraLocaleManager.Get("DivaniMods.Role.Revenant", "Revenant");
    public string RoleDescription => MiraLocaleManager.Get("DivaniMods.Role.Revenant.Description");
    public string RoleMedDescription => MiraLocaleManager.Get("DivaniMods.Role.Revenant.MedDescription");
    public string RoleLongDescription => MiraLocaleManager.Get("DivaniMods.Role.Revenant.LongDescription");

    public string GetAdvancedDescription() => RoleLongDescription + MiscUtils.AppendOptionsText(GetType());

    public Color RoleColor => RevenantColor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorAfterlife;
    public bool HasImpostorVision => true;

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = MiraAPI.Utilities.Assets.TmpSpriteUtils.CreateSpriteAsset(DivaniAssets.RevenantIcon.LoadAsset(), "DivaniMod.Role.Impostor.Revenant", 1.45f),
        Icon = DivaniAssets.RevenantIcon,
        HideSettings = true,
        CanModifyChance = false,
        DefaultChance = 0,
        DefaultRoleCount = 0,
        MaxRoleCount = 0,
        ShowInFreeplay = true,
    };

    public void Spawn()
    {
        Setup = true;

        if (HudManagerPatches.CamouflageCommsEnabled)
        {
            Player.SetCamouflage(false);
        }

        Player.gameObject.layer = LayerMask.NameToLayer("Players");

        Player.gameObject.GetComponent<PassiveButton>().OnClick = new Button.ButtonClickedEvent();
        Player.gameObject.GetComponent<PassiveButton>().OnClick.AddListener((Action)(() => Player.OnClick()));
        Player.gameObject.GetComponent<BoxCollider2D>().enabled = true;

        if (Player.AmOwner)
        {
            Player.SpawnAtRandomVent();
            Player.MyPhysics.ResetMoveState();

            HudManager.Instance.SetHudActive(false);
            HudManager.Instance.SetHudActive(true);
            HudManagerPatches.ResetZoom();
        }
    }

    public void FadeUpdate()
    {
        if (!Caught && Setup)
        {
            Player.GhostFade();
            Faded = true;
        }
        else if (Faded)
        {
            Player.ResetAppearance();
            Player.cosmetics.ToggleNameVisible(true);
            Player.cosmetics.currentBodySprite.BodySprite.color = Color.white;
            Player.gameObject.layer = LayerMask.NameToLayer("Ghost");
            Player.MyPhysics.ResetMoveState();
            Faded = false;
        }
    }

    public void FixedUpdate()
    {
        if (Player == null || Player.Data.Role is not RevenantRole || MeetingHud.Instance)
        {
            return;
        }

        FadeUpdate();
    }

    public void Clicked()
    {
        Caught = true;
        Player.Exiled();

        DivaniMods.Events.Impostor.ImpostorPower.SummonerState.ResetKills();
    }

    public static int ActiveCount { get; private set; }

    public static void ResetActiveCount()
    {
        ActiveCount = 0;
    }

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);

        ActiveCount++;

        DivaniMods.Events.Impostor.ImpostorPower.SummonerState.ResetKills();

        if (!Player.HasModifier<BasicGhostModifier>())
        {
            Player.AddModifier<BasicGhostModifier>();
        }
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);

        if (ActiveCount > 0)
        {
            ActiveCount--;
        }
    }

    public override bool CanUse(IUsable console)
    {
        var validUsable = console.TryCast<Console>() ||
                          console.TryCast<DoorConsole>() ||
                          console.TryCast<OpenDoorConsole>() ||
                          console.TryCast<DeconControl>() ||
                          console.TryCast<PlatformConsole>() ||
                          console.TryCast<Ladder>() ||
                          console.TryCast<ZiplineConsole>();

        return GhostActive && validUsable;
    }
}
