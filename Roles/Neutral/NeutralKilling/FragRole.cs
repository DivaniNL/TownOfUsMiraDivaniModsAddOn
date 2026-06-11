using System;
using System.Linq;
using AmongUs.GameOptions;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Utilities;
using DivaniMods.Assets;
using DivaniMods.Buttons.Neutral.NeutralKilling;
using DivaniMods.Options;
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

namespace DivaniMods.Roles.Neutral.NeutralKilling;

public sealed class FragRole(IntPtr cppPtr)
    : NeutralRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable, ICrewVariant
{
    public static readonly Color FragColor = new Color32(232, 168, 124, 255);

    public string RoleName => "Frag";
    public string RoleDescription => "Hot potato time!";
    public string RoleLongDescription =>
        "Give a time bomb to a player.\n" +
        "After a short random delay the timer starts.\n" +
        "The holder can pass it on, but not back\n" +
        "to the previous holder until it moves again.\n" +
        "Win by outlasting all other killers.";
    public Color RoleColor => FragColor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleAlignment RoleAlignment => RoleAlignment.NeutralKilling;

    public DoomableType DoomHintType => DoomableType.Relentless;

    public RoleBehaviour CrewVariant =>
        RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<TrapperRole>());

    public string GetAdvancedDescription() => RoleLongDescription + MiscUtils.AppendOptionsText(GetType());

    public bool HasImpostorVision => true;

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = DivaniAssets.FragIcon,
        IntroSound = DivaniAssets.FragIntroSound,
        MaxRoleCount = 1,
        CanUseVent = OptionGroupSingleton<FragOptions>.Instance.CanVent.Value,
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>(),
    };

    public override void SpawnTaskHeader(PlayerControl playerControl)
    {
        if (playerControl != PlayerControl.LocalPlayer)
        {
            return;
        }
        ImportantTextTask orCreateTask = PlayerTask.GetOrCreateTask<ImportantTextTask>(playerControl, 0);
        orCreateTask.Text =
            $"{TownOfUsColors.Neutral.ToTextColor()}{TouLocale.GetParsed("NeutralKillingTaskHeader")}</color>";
        orCreateTask.name = "NeutralRoleText";
    }

    public void OffsetButtons()
    {
        var beforeVent = !OptionGroupSingleton<FragOptions>.Instance.CanVent.Value;
        Coroutines.Start(MiscUtils.CoMoveButtonIndex(CustomButtonSingleton<FragGiveBombButton>.Instance, beforeVent));
        Coroutines.Start(MiscUtils.CoMoveButtonIndex(CustomButtonSingleton<FragBombButton>.Instance, beforeVent));
    }

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        if (Player.AmOwner)
        {
            OffsetButtons();
            HudManager.Instance.ImpostorVentButton.graphic.sprite = DivaniAssets.FragVentButton.LoadAsset();
            HudManager.Instance.ImpostorVentButton.buttonLabelText.SetOutlineColor(FragColor);
            CustomButtonSingleton<FakeVentButton>.Instance.Show = false;
        }
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
        var fragCount = CustomRoleUtils.GetActiveRolesOfType<FragRole>().Count(x => !x.Player.HasDied());

        if (MiscUtils.KillersAliveCount > fragCount)
        {
            return false;
        }

        return fragCount >= Helpers.GetAlivePlayers().Count - fragCount;
    }

    public override bool DidWin(GameOverReason gameOverReason)
    {
        return WinConditionMet();
    }
}
