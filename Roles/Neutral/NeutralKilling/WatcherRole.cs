using Il2CppInterop.Runtime.Attributes;
using System;
using System.Linq;
using System.Text;
using AmongUs.GameOptions;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using DivaniMods.Assets;
using DivaniMods.Buttons.Neutral.NeutralKilling;
using DivaniMods.Options;
using TownOfUs;
using TownOfUs.Assets;
using TownOfUs.Modules.Localization;
using TownOfUs.Modules.Wiki;
using TownOfUs.Roles;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using TownOfUs.Extensions;
using UnityEngine;

namespace DivaniMods.Roles.Neutral.NeutralKilling;

public sealed class WatcherRole(IntPtr cppPtr)
    : NeutralRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public static readonly Color WatcherColor = new Color32(0xD3, 0xA6, 0x35, 255);
    public static readonly Color GreenLightColor = new Color32(0x7C, 0xCE, 0x34, 255);
    public static readonly Color RedLightColor = new Color32(0xE4, 0x33, 0x22, 255);

    public string RoleName => "Watcher";
    public string LocaleKey => "Watcher";
    public string RoleDescription => "Green light... Red light!";
    public string RoleLongDescription =>
        "Call out Red Light, Green Light.\n" +
        "Anyone who moves during Red Light dies.";
    public Color RoleColor => WatcherColor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleAlignment RoleAlignment => RoleAlignment.NeutralKilling;

    public DoomableType DoomHintType => DoomableType.Fearmonger;

    public bool HasImpostorVision => true;

    public string GetAdvancedDescription()
    {
        var desc = RoleLongDescription;

        var grace = OptionGroupSingleton<WatcherOptions>.Instance.RedLightGracePeriod.Value;
        if (grace > 0f)
        {
            desc += $"\nRed Light starts with a {grace:0.0}s grace period. Moving during it won't get you killed.";
        }

        desc += "\nPlayers locked in a Duel or currently invisible are not affected by Red Light.";

        return desc + MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var sb = ITownOfUsRole.SetNewTabText(this);
        var req = (int)OptionGroupSingleton<WatcherOptions>.Instance.KillsPerExtraCharge.Value;
        var button = CustomButtonSingleton<WatcherWatchButton>.Instance;
        var charges = button?.CurrentCharges ?? 0;
        var kills = button != null ? Math.Min(button.KillsTowardCharge, req) : 0;

        sb.AppendLine(TownOfUsPlugin.Culture, $"<b>Watch charges: {charges}</b>");
        sb.AppendLine(TownOfUsPlugin.Culture, $"<b>Kills until next charge {kills}/{req}</b>");

        return sb;
    }

    [HideFromIl2Cpp] public List<CustomButtonWikiDescription> Abilities { get; } =
    [
        new("Watch", "Start a Green Light, then a Red Light. Anyone who moves during Red Light (after the grace period) dies. Players locked in a Duel or currently invisible are not affected by Red Light.", DivaniAssets.WatcherWatchButton),
        new("Kill", "Kill a nearby player.", DivaniAssets.WatcherKillButton)
    ];

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = DivaniAssets.WatcherIcon,
        IntroSound = DivaniAssets.WatcherIntroSound,
        MaxRoleCount = 1,
        CanUseVent = OptionGroupSingleton<WatcherOptions>.Instance.CanVent.Value,
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

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);

        if (Player.AmOwner)
        {
            CustomButtonSingleton<WatcherWatchButton>.Instance?.ResetCharges();

            if (OptionGroupSingleton<WatcherOptions>.Instance.CanVent.Value)
            {
                HudManager.Instance.ImpostorVentButton.graphic.sprite = DivaniAssets.WatcherVentButton.LoadAsset();
                HudManager.Instance.ImpostorVentButton.buttonLabelText.SetOutlineColor(WatcherColor);
            }
        }
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);
        TouRoleUtils.ClearTaskHeader(Player);

        if (Player.AmOwner && OptionGroupSingleton<WatcherOptions>.Instance.CanVent.Value)
        {
            HudManager.Instance.ImpostorVentButton.graphic.sprite = TouAssets.VentSprite.LoadAsset();
            HudManager.Instance.ImpostorVentButton.buttonLabelText.SetOutlineColor(TownOfUsColors.Impostor);
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
        if (Player.HasDied())
        {
            return false;
        }

        var aliveCount = Helpers.GetAlivePlayers().Count;
        var killersAlive = MiscUtils.KillersAliveCount;

        return aliveCount <= killersAlive && killersAlive == 1;
    }

    public override bool DidWin(GameOverReason gameOverReason)
    {
        return WinConditionMet();
    }
}
