using Il2CppInterop.Runtime.Attributes;
using System;
using System.Linq;
using System.Text;
using AmongUs.GameOptions;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using DivaniMods.Assets;
using DivaniMods.Buttons.Neutral.NeutralKilling;
using DivaniMods.Options;
using TownOfUs;
using TownOfUs.Assets;
using TownOfUs.Modules.Wiki;
using TownOfUs.Roles;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using TownOfUs.Extensions;
using UnityEngine;
using DivaniMods.Interfaces;

namespace DivaniMods.Roles.Neutral.NeutralKilling;

public sealed class WatcherRole(IntPtr cppPtr)
    : NeutralRole(cppPtr), IDivaniRole, IWikiDiscoverable, IDoomable, ICrewVariant
{
    public static readonly Color WatcherColor = new Color32(0xD3, 0xA6, 0x35, 255);
    public static readonly Color GreenLightColor = new Color32(0x7C, 0xCE, 0x34, 255);
    public static readonly Color RedLightColor = new Color32(0xE4, 0x33, 0x22, 255);

    public string RoleName => MiraLocaleManager.Get("DivaniMods.Role.Watcher", "Watcher");
    public string LocaleKey => "Watcher";
    public string RoleDescription => MiraLocaleManager.Get("DivaniMods.Role.Watcher.Description");
    public string RoleMedDescription => MiraLocaleManager.Get("DivaniMods.Role.Watcher.MedDescription");
    public string RoleLongDescription => MiraLocaleManager.Get("DivaniMods.Role.Watcher.LongDescription");
    public Color RoleColor => WatcherColor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleAlignment RoleAlignment => RoleAlignment.NeutralKilling;

    public DoomableType DoomHintType => DoomableType.Fearmonger;

    public RoleBehaviour CrewVariant =>
        RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<SentryRole>());

    public bool HasImpostorVision => true;

    public string GetAdvancedDescription()
    {
        var desc = RoleLongDescription;

        var grace = OptionGroupSingleton<WatcherOptions>.Instance.RedLightGracePeriod.Value;
        if (grace > 0f)
        {
            desc += "\n" + MiraLocaleManager.Get("DivaniMods.Role.Watcher.Advanced.GracePeriod")
                .Replace("<seconds>", grace.ToString("0.0", TownOfUsPlugin.Culture));
        }

        desc += "\n" + MiraLocaleManager.Get("DivaniMods.Role.Watcher.Advanced.Exceptions");

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

        var chargesText = MiraLocaleManager.Get("DivaniMods.Role.Watcher.Tab.WatchCharges")
            .Replace("<charges>", charges.ToString(TownOfUsPlugin.Culture));

        var killsText = MiraLocaleManager.Get("DivaniMods.Role.Watcher.Tab.KillsUntilNextCharge")
            .Replace("<kills>", kills.ToString(TownOfUsPlugin.Culture))
            .Replace("<required>", req.ToString(TownOfUsPlugin.Culture));

        sb.AppendLine($"<b>{chargesText}</b>");
        sb.AppendLine($"<b>{killsText}</b>");

        return sb;
    }

    [HideFromIl2Cpp] public List<CustomButtonWikiDescription> Abilities { get; } =
    [
        new(
            MiraLocaleManager.Get("DivaniMods.Role.Watcher.Ability.Watch"),
            MiraLocaleManager.Get("DivaniMods.Role.Watcher.Ability.Watch.Description"),
            DivaniAssets.WatcherWatchButton
        ),
        new(
            MiraLocaleManager.Get("DivaniMods.Role.Watcher.Ability.Kill"),
            MiraLocaleManager.Get("DivaniMods.Role.Watcher.Ability.Kill.Description"),
            DivaniAssets.WatcherKillButton
        ),
    ];

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = MiraAPI.Utilities.Assets.TmpSpriteUtils.CreateSpriteAsset(DivaniAssets.WatcherIcon.LoadAsset(), "DivaniMod.Role.Neutral.Watcher", 1.45f),
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
            $"{TownOfUsColors.Neutral.ToTextColor()}{MiraLocaleManager.Get("NeutralKillingTaskHeader")}</color>";
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
