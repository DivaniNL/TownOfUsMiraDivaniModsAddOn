using Il2CppInterop.Runtime.Attributes;
using System;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Utilities.Extensions;
using DivaniMods.Assets;
using DivaniMods.Options;
using DivaniMods.Roles.Crewmate.CrewmateKilling;
using TownOfUs.Modifiers;
using TownOfUs.Modules.Wiki;
using TownOfUs.Patches;
using TownOfUs.Roles;
using TownOfUs.Utilities;
using TownOfUs.Utilities.Appearances;
using UnityEngine;
using UnityEngine.UI;

namespace DivaniMods.Roles.Crewmate.CrewmateAfterlife;

public sealed class VengefulSoulRole(IntPtr cppPtr)
    : CrewmateGhostRole(cppPtr), ITownOfUsRole, IGhostRole, IWikiDiscoverable
{
    public bool Setup { get; set; }
    public bool Caught { get; set; }
    public bool Faded { get; set; }

    public bool CanBeClicked
    {
        get => false;
        set { }
    }

    public bool GhostActive => Setup && !Caught;

    public bool CanCatch() => false;

    public string LocaleKey => "VengefulSoul";
    public string RoleName => "Vengeful Soul";
    public string RoleDescription => "Hunt down your killer!";
    public string RoleLongDescription =>
        "You were murdered as the Retributionist and rose as a Vengeful Soul.\n" +
        "Seek revenge on your killer to return to the ship!";

    public string GetAdvancedDescription() => RoleLongDescription + MiscUtils.AppendOptionsText(GetType());

    public Color RoleColor => RetributionistRole.RetributionistColor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateAfterlife;

    [HideFromIl2Cpp] public List<CustomButtonWikiDescription> Abilities { get; } =
    [
        new("Revenge", "Hunt down and kill your killer before your time runs out to get revived.", DivaniAssets.VengefulSoulRevengeButton)
    ];

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = DivaniAssets.RetributionistIcon,
        HideSettings = true,
        CanModifyChance = false,
        DefaultChance = 0,
        DefaultRoleCount = 0,
        MaxRoleCount = 0,
        ShowInFreeplay = false,
        TasksCountForProgress = false,
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
            HudManager.Instance.AbilityButton.SetDisabled();
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
        if (Player == null || Player.Data.Role is not VengefulSoulRole || MeetingHud.Instance)
        {
            return;
        }

        FadeUpdate();

        if (GhostActive && OptionGroupSingleton<RetributionistOptions>.Instance.RevengeBreaksShields)
        {
            if (Player.TryGetModifier<IndirectAttackerModifier>(out var indirect))
            {
                indirect.ResetTimer();
            }
            else
            {
                Player.AddModifier<IndirectAttackerModifier>(true);
            }
        }
    }

    public void Clicked()
    {
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

        if (targetPlayer != null)
        {
            targetPlayer.ResetAppearance(fullReset: true);
            targetPlayer.cosmetics?.ToggleNameVisible(true);

            var body = targetPlayer.cosmetics?.currentBodySprite?.BodySprite;
            if (body != null)
            {
                body.SetOutline(null);
                body.color = Color.white;
            }

            if (targetPlayer.Data != null && targetPlayer.Data.IsDead)
            {
                targetPlayer.gameObject.layer = LayerMask.NameToLayer("Ghost");
                targetPlayer.MyPhysics?.ResetMoveState();
            }
        }

        Faded = false;
    }

    public override void UseAbility()
    {
        if (GhostActive)
        {
            return;
        }

        base.UseAbility();
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
