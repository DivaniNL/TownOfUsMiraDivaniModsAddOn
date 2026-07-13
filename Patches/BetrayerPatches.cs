using System;
using System.Linq;
using HarmonyLib;
using MiraAPI.GameEnd;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using DivaniMods.GameOver;
using DivaniMods.Modifiers.Game.Alliance;
using DivaniMods.Options;
using TownOfUs.Modifiers;
using TownOfUs.Modules.Components;
using TownOfUs.Options;
using TownOfUs.Options.Maps;
using TownOfUs.Roles.Impostor;
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

        var hidden = visibility == TownOfUs.Utilities.DataVisibility.Hidden;
        var reveal = visibility is TownOfUs.Utilities.DataVisibility.Show ||
                     (PlayerControl.LocalPlayer.HasDied() &&
                      OptionGroupSingleton<GeneralOptions>.Instance.TheDeadKnow && !hidden);

        var revealedToImpostors = CanHuntBetrayer(PlayerControl.LocalPlayer) &&
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

    [HarmonyPatch(typeof(VentButton), nameof(VentButton.DoClick))]
    [HarmonyPrefix]
    public static bool VentClickPrefix()
    {
        return !(IsLocalBetrayer() && !OptionGroupSingleton<BetrayerOptions>.Instance.CanVent.Value);
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

        if (!options.CanVent.Value && __instance.ImpostorVentButton != null)
        {
            __instance.ImpostorVentButton.ToggleVisible(false);
        }

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
