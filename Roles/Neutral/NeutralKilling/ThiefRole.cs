using Il2CppInterop.Runtime.Attributes;
using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using DivaniMods.Assets;
using DivaniMods.Options;
using TownOfUs;
using TownOfUs.Assets;
using TownOfUs.Extensions;
using TownOfUs.Modifiers.Game.Impostor;
using TownOfUs.Modules.Localization;
using TownOfUs.Modules.Wiki;
using TownOfUs.Roles;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;

namespace DivaniMods.Roles.Neutral.NeutralKilling;

public sealed class ThiefRole(IntPtr cppPtr)
    : NeutralRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable, ICrewVariant
{
    public static readonly Color ThiefColor = new Color(0.5f, 0.3f, 0.1f);

    public string RoleName => "Thief";
    public string LocaleKey => "Thief";
    public string RoleDescription => "Steal everything!";
    public string RoleLongDescription => "Use your Pickpocket ability to steal modifiers from players and kill to be the last person standing!";
    public Color RoleColor => ThiefColor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleAlignment RoleAlignment => RoleAlignment.NeutralKilling;

    public override bool IsAffectedByComms => false;

    public DoomableType DoomHintType => DoomableType.Trickster;

    public RoleBehaviour CrewVariant =>
        RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<SwapperRole>());

    public bool HasImpostorVision => true;

    public string GetAdvancedDescription() => RoleLongDescription + MiscUtils.AppendOptionsText(GetType());

    [HideFromIl2Cpp] public List<uint> StolenModifierIds { get; } = new();

    public int MaxStolenModifiers => (int)OptionGroupSingleton<ThiefOptions>.Instance.MaxStolenModifiers;

    public bool CanStealMore => StolenModifierIds.Count < MaxStolenModifiers;

    [HideFromIl2Cpp] public List<CustomButtonWikiDescription> Abilities { get; } =
    [
        new("Pickpocket", "Steal a modifier from a nearby player. You can only target players who have a modifier to steal. Trying to steal a modifier you cannot use will give you a random modifier instead.", DivaniAssets.PickpocketButton),
        new("Kill", "Kill a nearby player.", DivaniAssets.ThiefKillButton)
    ];

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = MiraAPI.Utilities.Assets.TmpSpriteUtils.CreateSpriteAsset(DivaniAssets.ThiefIcon.LoadAsset(), "DivaniMod.Role.Neutral.Thief", 1.45f),
        Icon = DivaniAssets.ThiefIcon,
        OptionsScreenshot = DivaniAssets.ThiefBanner,
        IntroSound = DivaniAssets.ThiefIntroSound,
        MaxRoleCount = 1,
        CanUseVent = OptionGroupSingleton<ThiefOptions>.Instance.CanVent.Value,
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

    public override void Initialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Initialize(this, targetPlayer);
        StolenModifierIds.Clear();

        if (Player.AmOwner && OptionGroupSingleton<ThiefOptions>.Instance.CanVent.Value)
        {
            HudManager.Instance.ImpostorVentButton.graphic.sprite = DivaniAssets.ThiefVentButton.LoadAsset();
            HudManager.Instance.ImpostorVentButton.buttonLabelText.SetOutlineColor(ThiefColor);
        }
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);
        StolenModifierIds.Clear();
        TouRoleUtils.ClearTaskHeader(Player);

        if (Player.AmOwner && OptionGroupSingleton<ThiefOptions>.Instance.CanVent.Value)
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
        return ParityWinMet() && !HasUnmetDeadlyQuota();
    }

    public bool ParityWinMet()
    {
        if (Player.HasDied())
        {
            return false;
        }

        var aliveCount = Helpers.GetAlivePlayers().Count;
        var killersAlive = MiscUtils.KillersAliveCount;

        return aliveCount <= killersAlive && killersAlive == 1;
    }

    public bool HasUnmetDeadlyQuota()
    {
        return Player.TryGetModifier<DeadlyQuotaModifier>(out var quota) &&
               !quota.IgnoreQuota &&
               quota.KillCount < quota.KillQuota;
    }

    public override bool DidWin(GameOverReason gameOverReason)
    {
        return WinConditionMet();
    }
}