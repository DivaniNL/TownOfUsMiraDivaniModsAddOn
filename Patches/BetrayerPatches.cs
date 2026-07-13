using System;
using System.Linq;
using HarmonyLib;
using MiraAPI.GameEnd;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using DivaniMods.GameOver;
using DivaniMods.Modifiers.Game.Alliance;
using DivaniMods.Options;
using TownOfUs.Buttons.Impostor;
using TownOfUs.Interfaces;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Modifiers.Impostor.Herbalist;
using TownOfUs.Modules.Components;
using TownOfUs.Options;
using TownOfUs.Options.Maps;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles.Impostor;
using TownOfUs.Roles.Other;
using TownOfUs.Utilities;
using UnityEngine;

namespace DivaniMods.Patches;

[HarmonyPatch]
public static class BetrayerPatches
{
    private static bool IsLocalBetrayer()
    {
        var player = PlayerControl.LocalPlayer;
        return player != null && player.Data != null && player.HasModifier<BetrayerModifier>();
    }

    private static bool CanHuntBetrayer(PlayerControl? player)
    {
        return player != null && player.Data != null && player.IsImpostorAligned() &&
               !player.HasModifier<BetrayerModifier>();
    }

    private static bool IsDeadOrSpectating(PlayerControl? player)
    {
        return player != null && player.Data != null &&
               (player.HasDied() || player.Data.Role is SpectatorRole);
    }

    [HarmonyPatch(typeof(TownOfUs.Utilities.Extensions), nameof(TownOfUs.Utilities.Extensions.GetClosestLivingPlayer))]
    [HarmonyPrefix]
    public static void GetClosestLivingPlayerPrefix(PlayerControl playerControl, ref bool includeImpostors)
    {
        if (playerControl != null && playerControl.HasModifier<BetrayerModifier>())
        {
            includeImpostors = true;
        }
    }

    [HarmonyPatch(typeof(TownOfUs.Utilities.Extensions), nameof(TownOfUs.Utilities.Extensions.GetClosestLivingPlayer))]
    [HarmonyPriority(Priority.Last)]
    [HarmonyPostfix]
    public static void GetClosestLivingPlayerPostfix(PlayerControl playerControl, bool includeImpostors,
        float distance, bool ignoreColliders, Predicate<PlayerControl>? predicate, ref PlayerControl? __result)
    {
        if (includeImpostors || !CanHuntBetrayer(playerControl) || !BetrayerRevealedModifier.AnyRevealed())
        {
            return;
        }

        var candidates = MiraAPI.Utilities.Helpers.GetClosestPlayers(playerControl, distance, ignoreColliders)
            .Where(player => !player.Data.Disconnected &&
                             player.PlayerId != playerControl.PlayerId &&
                             !player.Data.IsDead &&
                             ((player.TryGetModifier<DisabledModifier>(out var disabled) &&
                               disabled.IsConsideredAlive) || !player.HasModifier<DisabledModifier>()) &&
                             (!player.IsImpostorAligned() || player.HasModifier<BetrayerRevealedModifier>()))
            .ToList();

        __result = predicate != null ? candidates.Find(predicate) : candidates.FirstOrDefault();
    }

    [HarmonyPatch(typeof(ParasiteOvertakeButton), nameof(ParasiteOvertakeButton.GetTarget))]
    [HarmonyPriority(Priority.Last)]
    [HarmonyPostfix]
    public static void ParasiteGetTargetPostfix(ParasiteOvertakeButton __instance, ref PlayerControl? __result)
    {
        var local = PlayerControl.LocalPlayer;

        if (local?.Data?.Role is not ParasiteRole parasite || parasite.Controlled != null ||
            !BetrayerRevealedModifier.CanRelaxTargeting(local))
        {
            return;
        }

        __result = local.GetClosestLivingPlayer(
            true,
            __instance.Distance,
            predicate: plr =>
                plr != null &&
                !plr.AmOwner &&
                !plr.HasDied() &&
                (!plr.IsImpostorAligned() || BetrayerRevealedModifier.AllowsImpostorTarget(local, plr)) &&
                !plr.IsInTargetingAnimState() &&
                !plr.GetModifiers<BaseModifier>().Any(x => x is IUncontrollable) &&
                !plr.HasModifier<ParasiteInfectedModifier>());
    }

    [HarmonyPatch(typeof(EclipsalBlindButton), "OnClick")]
    [HarmonyPostfix]
    public static void EclipsalBlindPostfix()
    {
        var local = PlayerControl.LocalPlayer;

        if (local == null || ShipStatus.Instance == null || !BetrayerRevealedModifier.CanRelaxTargeting(local))
        {
            return;
        }

        var radius = OptionGroupSingleton<EclipsalOptions>.Instance.BlindRadius;
        var nearby = MiraAPI.Utilities.Helpers.GetClosestPlayers(local, radius * ShipStatus.Instance.MaxLightRadius);

        foreach (var player in nearby.Where(x => !x.HasDied() && x.IsImpostor() &&
                                                 !x.HasModifier<EclipsalBlindModifier>() &&
                                                 BetrayerRevealedModifier.AllowsImpostorTarget(local, x)))
        {
            player.RpcAddModifier<EclipsalBlindModifier>(local);
        }
    }

    [HarmonyPatch(typeof(GrenadierFlashModifier), "ShouldPlayerBeBlinded")]
    [HarmonyPostfix]
    public static void GrenadierShouldBeBlindedPostfix(PlayerControl player, ref bool __result)
    {
        if (__result || player == null || player.HasDied() || MeetingHud.Instance ||
            !player.TryGetModifier<GrenadierFlashModifier>(out var flash))
        {
            return;
        }

        __result = BetrayerRevealedModifier.AllowsImpostorTarget(flash.Grenadier, player);
    }

    [HarmonyPatch(typeof(GrenadierFlashModifier), "ShouldPlayerBeDimmed")]
    [HarmonyPostfix]
    public static void GrenadierShouldBeDimmedPostfix(PlayerControl player, ref bool __result)
    {
        if (!__result || player == null || player.HasDied() ||
            !player.TryGetModifier<GrenadierFlashModifier>(out var flash))
        {
            return;
        }

        if (BetrayerRevealedModifier.AllowsImpostorTarget(flash.Grenadier, player))
        {
            __result = false;
        }
    }

    [HarmonyPatch(typeof(GrenadierFlashModifier), nameof(GrenadierFlashModifier.CanUseConsoles), MethodType.Getter)]
    [HarmonyPostfix]
    public static void GrenadierCanUseConsolesPostfix(GrenadierFlashModifier __instance, ref bool __result)
    {
        if (__result && BetrayerRevealedModifier.AllowsImpostorTarget(__instance.Grenadier, __instance.Player))
        {
            __result = false;
        }
    }

    [HarmonyPatch(typeof(GrenadierFlashModifier), nameof(GrenadierFlashModifier.CanOpenMap), MethodType.Getter)]
    [HarmonyPostfix]
    public static void GrenadierCanOpenMapPostfix(GrenadierFlashModifier __instance, ref bool __result)
    {
        if (__result && BetrayerRevealedModifier.AllowsImpostorTarget(__instance.Grenadier, __instance.Player))
        {
            __result = false;
        }
    }

    [HarmonyPatch(typeof(PuppeteerKillButton), nameof(PuppeteerKillButton.GetTarget))]
    [HarmonyPriority(Priority.Last)]
    [HarmonyPostfix]
    public static void PuppeteerGetTargetPostfix(PuppeteerKillButton __instance, ref PlayerControl? __result)
    {
        var local = PlayerControl.LocalPlayer;
        var controlButton = CustomButtonSingleton<PuppeteerControlButton>.Instance;

        if (local?.Data?.Role is not PuppeteerRole puppeteer || puppeteer.Controlled == null ||
            controlButton == null || !controlButton.EffectActive ||
            !BetrayerRevealedModifier.CanRelaxTargeting(local))
        {
            return;
        }

        __result = puppeteer.Controlled.GetClosestLivingPlayer(
            true,
            __instance.Distance,
            predicate: plr =>
                plr != null &&
                !plr.AmOwner &&
                !plr.HasDied() &&
                !plr.IsInTargetingAnimState() &&
                (!plr.IsImpostorAligned() || BetrayerRevealedModifier.AllowsImpostorTarget(local, plr)));
    }

    [HarmonyPatch(typeof(AmbusherAmbushButton), nameof(AmbusherAmbushButton.GetTarget))]
    [HarmonyPriority(Priority.Last)]
    [HarmonyPostfix]
    public static void AmbusherGetTargetPostfix(AmbusherAmbushButton __instance, ref PlayerControl? __result)
    {
        var local = PlayerControl.LocalPlayer;

        if (local?.Data?.Role is not AmbusherRole ambusher || ambusher.Pursued == null ||
            !BetrayerRevealedModifier.CanRelaxTargeting(local))
        {
            return;
        }

        __result = ambusher.Pursued.GetClosestLivingPlayer(true, __instance.Distance, false,
            plr => !plr.IsImpostorAligned() || BetrayerRevealedModifier.AllowsImpostorTarget(local, plr));
    }

    [HarmonyPatch(typeof(HerbalistAbilityHerbButton), nameof(HerbalistAbilityHerbButton.GetTarget))]
    [HarmonyPriority(Priority.Last)]
    [HarmonyPostfix]
    public static void HerbalistGetTargetPostfix(HerbalistAbilityHerbButton __instance, ref PlayerControl? __result)
    {
        var local = PlayerControl.LocalPlayer;

        if (__instance.CurrentAbility is not HerbAbilities.Confuse || local == null ||
            OptionGroupSingleton<GeneralOptions>.Instance.FFAImpostorMode ||
            !BetrayerRevealedModifier.CanRelaxTargeting(local))
        {
            return;
        }

        __result = local.GetClosestLivingPlayer(true, __instance.Distance, false,
            x => (!x.IsImpostorAligned() || BetrayerRevealedModifier.AllowsImpostorTarget(local, x)) &&
                 !x.HasModifier<HerbalistConfusedModifier>(mod => mod.Herbalist.AmOwner));
    }

    [HarmonyPatch(typeof(TownOfUs.Utilities.PlayerRoleTextExtensions),
        nameof(TownOfUs.Utilities.PlayerRoleTextExtensions.UpdateAllianceSymbols),
        typeof(string), typeof(PlayerControl), typeof(TownOfUs.Utilities.DataVisibility))]
    [HarmonyPostfix]
    public static void UpdateAllianceSymbolsPostfix(ref string __result, PlayerControl player,
        TownOfUs.Utilities.DataVisibility visibility)
    {
        if (player == null || !player.TryGetModifier<BetrayerModifier>(out var betrayer))
        {
            return;
        }

        var local = PlayerControl.LocalPlayer;
        var hidden = visibility == TownOfUs.Utilities.DataVisibility.Hidden;
        var reveal = visibility is TownOfUs.Utilities.DataVisibility.Show ||
                     (!hidden && IsDeadOrSpectating(local));

        var revealedToImpostors = !hidden && CanHuntBetrayer(local) &&
                                  player.HasModifier<BetrayerRevealedModifier>();

        if (player.AmOwner || reveal || revealedToImpostors)
        {
            __result += $"<color=#FFFFFF> (<color={BetrayerRevealedModifier.ColorTag}>{betrayer.ShortName}</color>)</color>";
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetKillTimer))]
    [HarmonyPriority(Priority.High)]
    [HarmonyPrefix]
    public static void SetKillTimerPrefix(PlayerControl __instance, ref float time)
    {
        if (__instance == null || __instance.Data?.Role == null || !__instance.Data.Role.CanUseKillButton)
        {
            return;
        }

        if (!__instance.HasModifier<BetrayerModifier>())
        {
            return;
        }

        var lobbyCooldown = GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown;
        if (lobbyCooldown <= 0f)
        {
            return;
        }

        var fullCooldown = __instance.GetKillCooldown();
        if (Mathf.Abs(time - lobbyCooldown) < 0.01f || Mathf.Abs(time - fullCooldown) < 0.01f)
        {
            time = OptionGroupSingleton<BetrayerOptions>.Instance.KillCooldown.Value;
        }
    }

    [HarmonyPatch(typeof(ImpostorRole), nameof(ImpostorRole.IsValidTarget))]
    [HarmonyPriority(Priority.Last)]
    [HarmonyPostfix]
    public static void ImpostorIsValidTargetPostfix(ImpostorRole __instance, [HarmonyArgument(0)] NetworkedPlayerInfo target,
        ref bool __result)
    {
        if (__result || __instance == null || __instance.Player == null)
        {
            return;
        }

        var killer = __instance.Player;

        if (!killer.HasModifier<BetrayerModifier>() &&
            !(CanHuntBetrayer(killer) && target?.Object != null &&
              target.Object.HasModifier<BetrayerRevealedModifier>()))
        {
            return;
        }

        __result = target is { Disconnected: false, IsDead: false } &&
                   target.PlayerId != killer.PlayerId && target.Role && target.Object &&
                   !target.Object.inVent && !target.Object.inMovingPlat &&
                   !(target.Object.TryGetModifier<DisabledModifier>(out var disabled) &&
                     !disabled.CanBeInteractedWith);
    }

    [HarmonyPatch(typeof(GameManager), nameof(GameManager.RpcEndGame))]
    [HarmonyPriority(Priority.First)]
    [HarmonyPrefix]
    public static bool RpcEndGamePrefix([HarmonyArgument(0)] GameOverReason endReason)
    {
        if (endReason != GameOverReason.ImpostorsBySabotage || !HexBombSabotageSystem.BombFinished)
        {
            return true;
        }

        var betrayer = BetrayerModifier.GetHexBombBetrayer();
        var winner = betrayer?.Data;
        if (winner == null || !SpellslingerRole.EveryoneHexed())
        {
            return true;
        }

        CustomGameOver.Trigger<BetrayerGameOver>([winner]);
        return false;
    }

    [HarmonyPatch(typeof(SabotageButton), nameof(SabotageButton.DoClick))]
    [HarmonyPrefix]
    public static bool SabotageClickPrefix()
    {
        return !(IsLocalBetrayer() && !OptionGroupSingleton<BetrayerOptions>.Instance.CanSabotage.Value);
    }

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last)]
    public static void HudUpdatePostfix(HudManager __instance)
    {
        if (!IsLocalBetrayer())
        {
            return;
        }

        var options = OptionGroupSingleton<BetrayerOptions>.Instance;

        if (!options.CanSabotage.Value && __instance.SabotageButton != null)
        {
            __instance.SabotageButton.ToggleVisible(false);
        }
    }

    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.CalculateLightRadius))]
    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last)]
    public static void CalculateLightRadiusPostfix(ShipStatus __instance, NetworkedPlayerInfo player, ref float __result)
    {
        if (MiscUtils.CurrentGamemode() is TouGamemode.HideAndSeek)
        {
            return;
        }

        if (player == null || player.IsDead || player.Object == null)
        {
            return;
        }

        if (!player.Object.HasModifier<BetrayerModifier>() ||
            OptionGroupSingleton<BetrayerOptions>.Instance.HasImpostorVision.Value)
        {
            return;
        }

        var t = 1f;
        if (__instance.Systems != null && __instance.Systems.TryGetValue(SystemTypes.Electrical, out var system))
        {
            var switchSystem = system.TryCast<SwitchSystem>();
            if (switchSystem != null)
            {
                t = switchSystem.Level;
            }
        }

        __result = Mathf.Lerp(__instance.MinLightRadius, __instance.MaxLightRadius, t) *
                   GameOptionsManager.Instance.currentNormalGameOptions.CrewLightMod;
        __result *= TownOfUsMapOptions.GetMapBasedCrewmateVision();
    }
}
