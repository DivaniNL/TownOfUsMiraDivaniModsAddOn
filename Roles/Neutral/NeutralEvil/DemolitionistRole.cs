using System;
using System.Text;
using AmongUs.GameOptions;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using Il2CppInterop.Runtime.Attributes;
using DivaniMods.Assets;
using DivaniMods.Options;
using DivaniMods.Patches;
using TownOfUs;
using TownOfUs.Assets;
using TownOfUs.Buttons;
using TownOfUs.Extensions;
using TownOfUs.Modules.Localization;
using TownOfUs.Modules.Wiki;
using TownOfUs.Roles;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;

namespace DivaniMods.Roles.Neutral.NeutralEvil;

public sealed class DemolitionistRole(IntPtr cppPtr)
    : NeutralRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable, ICrewVariant
{
    public static readonly Color DemolitionistColor = new Color32(0x28, 0x36, 0x7D, 255);

    public string RoleName => "Demolitionist";
    public string RoleDescription => "Plant sabotages to win!";
    public string RoleLongDescription =>
        "Plant at admin, security, vitals, or door log.\n" +
        "If the crew defuses in time, it fails.";
    public Color RoleColor => DemolitionistColor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleAlignment RoleAlignment => RoleAlignment.NeutralEvil;

    public DoomableType DoomHintType => DoomableType.Relentless;

    public RoleBehaviour CrewVariant =>
        RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<EngineerTouRole>());

    public string GetAdvancedDescription() => RoleLongDescription + MiscUtils.AppendOptionsText(GetType());

    public bool HasImpostorVision => true;

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = DivaniAssets.DemolitionistIcon,
        IntroSound = DivaniAssets.DemolitionistIntroSound,
        MaxRoleCount = 1,
        CanUseVent = OptionGroupSingleton<DemolitionistOptions>.Instance.CanVent,
        // Required so on death the role swaps to NeutralGhostRole and the solo
        // win condition keeps tracking the planted-sabotage counter; mirrors the
        // pattern Frag / Plague Doctor / Opportunist already use.
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>(),
    };

    public override void SpawnTaskHeader(PlayerControl playerControl)
    {
        if (playerControl != PlayerControl.LocalPlayer)
        {
            return;
        }
        var task = PlayerTask.GetOrCreateTask<ImportantTextTask>(playerControl, 0);
        task.Text =
            $"{TownOfUsColors.Neutral.ToTextColor()}{TouLocale.GetParsed("NeutralEvilTaskHeader")}</color>";
        task.name = "NeutralRoleText";
    }

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var stringB = ITownOfUsRole.SetNewTabText(this);
        var needed = (int)OptionGroupSingleton<DemolitionistOptions>.Instance.SabotagesToWin;
        var capped = Math.Min(DemolitionistSabotageState.SuccessfulSabotages, needed);
        stringB.AppendLine(TownOfUsPlugin.Culture, $"<b>Successful sabotages: {capped}/{needed}</b>");
        return stringB;
    }

    public override void Initialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Initialize(this, targetPlayer);

        // Match GlitchRole: always wire the real ImpostorVentButton + hide FakeVentButton for the local player.
        // Actual vent permission still comes from <see cref="CustomRoleConfiguration.CanUseVent"/> (Saboteur Can Vent).
        if (Player.AmOwner)
        {
            HudManager.Instance.ImpostorVentButton.graphic.sprite = TouAssets.VentSprite.LoadAsset();
            HudManager.Instance.ImpostorVentButton.buttonLabelText.SetOutlineColor(DemolitionistColor);
            CustomButtonSingleton<FakeVentButton>.Instance.Show = false;
        }

        DemolitionistSabotageState.RegisterDemolitionist(targetPlayer);
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);
        TouRoleUtils.ClearTaskHeader(Player);

        if (Player.AmOwner)
        {
            HudManager.Instance.ImpostorVentButton.graphic.sprite = TouAssets.VentSprite.LoadAsset();
            HudManager.Instance.ImpostorVentButton.buttonLabelText.SetOutlineColor(TownOfUsColors.Impostor);
            CustomButtonSingleton<FakeVentButton>.Instance.Show = true;
        }
    }

    public override bool CanUse(IUsable usable)
    {
        if (!GameManager.Instance.LogicUsables.CanUse(usable, Player))
        {
            return false;
        }

        var console = usable.TryCast<Console>()!;
        return console == null || console.AllowImpostor;
    }

    public bool WinConditionMet()
    {
        var needed = (int)OptionGroupSingleton<DemolitionistOptions>.Instance.SabotagesToWin;
        return DemolitionistSabotageState.SuccessfulSabotages >= needed;
    }

    public override bool DidWin(GameOverReason gameOverReason)
    {
        return WinConditionMet();
    }
}
