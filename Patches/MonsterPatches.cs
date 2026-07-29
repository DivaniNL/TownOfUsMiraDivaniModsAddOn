using DivaniMods.Buttons.Neutral.NeutralKilling;
using DivaniMods.Modules.Monster;
using DivaniMods.Options;
using DivaniMods.Roles.Neutral.NeutralKilling;
using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using Reactor.Utilities;
using System.Collections;
using TownOfUs.Buttons;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Roles.Impostor;
using TownOfUs.Utilities;
using UnityEngine;

namespace DivaniMods.Patches;

[HarmonyPatch]
public static class MonsterPatches
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CheckMurder))]
    [HarmonyPrefix]
    public static bool CheckMurderPrefix([HarmonyArgument(0)] PlayerControl target)
        => target == null || !MonsterState.IsEaten(target.PlayerId);

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
    [HarmonyPrefix]
    public static bool MurderPlayerPrefix([HarmonyArgument(0)] PlayerControl target)
        => target == null || !MonsterState.IsEaten(target.PlayerId);

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.ReportDeadBody))]
    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    public static bool ReportDeadBodyPrefix(PlayerControl __instance)
        => !MonsterState.IsEaten(__instance.PlayerId);

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CmdReportDeadBody))]
    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    public static bool CmdReportDeadBodyPrefix(PlayerControl __instance)
        => !MonsterState.IsEaten(__instance.PlayerId);

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
    [HarmonyPostfix]
    [HarmonyPriority(Priority.First)]
    public static void MeetingHudStartPostfix(MeetingHud __instance)
    {
        DigestEveryone();
        SortDigestedVictims(__instance);
    }

    private static void SortDigestedVictims(MeetingHud meetingHud)
    {
        var pendingIds = MonsterState.PendingDigestSortIds();
        if (pendingIds.Count == 0 || meetingHud == null) return;

        foreach (var id in pendingIds) MonsterState.ClearPendingDigestSort(id);

        var areas = meetingHud.playerStates.ToArray();
        var positions = areas.Select(a => a.transform.localPosition).ToArray();
        var sorted = areas
            .Select((area, idx) => (area, idx))
            .OrderBy(x => x.area.AmDead ? 1 : 0)
            .ThenBy(x => x.idx)
            .Select(x => x.area)
            .ToArray();

        for (var i = 0; i < sorted.Length; i++)
            sorted[i].transform.localPosition = positions[i];
    }

    private static void DigestEveryone()
    {
        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player == null || player.Data?.Role is not MonsterRole || MonsterState.CountHeldBy(player.PlayerId) <= 0)
                continue;

            MonsterRole.RpcDigest(player);

            var button = CustomButtonSingleton<MonsterDevourButton>.Instance;
            if (button != null)
                button.SetUses((int)OptionGroupSingleton<MonsterOptions>.Instance.MaxDevouredPerRound);
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
    [HarmonyPostfix]
    public static void FixedUpdatePostfix(PlayerControl __instance)
    {
        if (__instance?.Data == null || __instance.HasDied()) return;
        if (!MonsterState.IsEaten(__instance.PlayerId)) return;

        __instance.moveable = false;

        var collider = __instance.GetComponent<Collider2D>();
        if (collider != null) collider.enabled = false;

        if (MonsterDevourAnimation.ShouldBeHidden(__instance.PlayerId))
        {
            __instance.Visible = false;

            var MonsterId = MonsterState.MonsterOf(__instance.PlayerId);
            var Monster = MonsterId.HasValue ? MiscUtils.PlayerById(MonsterId.Value) : null;
            if (Monster != null && !Monster.HasDied())
            {
                var pos = Monster.GetTruePosition();
                __instance.transform.position = new Vector3(pos.x, pos.y, __instance.transform.position.z);
                if (__instance.MyPhysics?.body != null)
                {
                    __instance.MyPhysics.body.position = pos;
                    __instance.MyPhysics.body.velocity = Vector2.zero;
                }
            }
        }
    }

    [HarmonyPatch(typeof(FootstepsModifier), nameof(FootstepsModifier.OnActivate))]
    [HarmonyPrefix]
    public static bool FootstepsOnActivatePrefix(FootstepsModifier __instance)
    {
        try
        {
            if (__instance.Player != null && MonsterState.IsEaten(__instance.Player.PlayerId)) return false;
        }
        catch { }
        return true;
    }

    [HarmonyPatch(typeof(FootstepsModifier), nameof(FootstepsModifier.FixedUpdate))]
    [HarmonyPrefix]
    public static bool FootstepsFixedUpdatePrefix(FootstepsModifier __instance)
    {
        if (__instance?.Player?.Data == null) return true;
        return !MonsterState.IsEaten(__instance.Player.PlayerId);
    }


    [HarmonyPatch(typeof(PuppeteerRole), nameof(PuppeteerRole.RpcPuppeteerControl))]
    [HarmonyPrefix]
    public static bool PuppeteerControlPrefix(PlayerControl target)
        => target == null || !MonsterState.IsEaten(target.PlayerId);

    [HarmonyPatch(typeof(ParasiteRole), nameof(ParasiteRole.RpcParasiteControl))]
    [HarmonyPrefix]
    public static bool ParasiteControlPrefix(PlayerControl target)
        => target == null || !MonsterState.IsEaten(target.PlayerId);

    [HarmonyPatch(typeof(TownOfUsButton), nameof(TownOfUsButton.CanUse))]
    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    public static bool TownOfUsButtonCanUsePrefix(ref bool __result)
    {
        if (!IsLocalPlayerEaten()) return true;
        __result = false;
        return false;
    }

    [HarmonyPatch(typeof(TownOfUsButton), nameof(TownOfUsButton.ClickHandler))]
    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    public static bool TownOfUsButtonClickPrefix() => !IsLocalPlayerEaten();

    [HarmonyPatch(typeof(KillButton), nameof(KillButton.DoClick))]
    [HarmonyPrefix]
    public static bool KillButtonPrefix() => !IsLocalPlayerEaten();

    [HarmonyPatch(typeof(UseButton), nameof(UseButton.DoClick))]
    [HarmonyPrefix]
    public static bool UseButtonPrefix() => !IsLocalPlayerEaten();

    [HarmonyPatch(typeof(ReportButton), nameof(ReportButton.DoClick))]
    [HarmonyPrefix]
    public static bool ReportButtonDoClickPrefix() => !IsLocalPlayerEaten();

    [HarmonyPatch(typeof(CustomActionButton), nameof(CustomActionButton.FixedUpdateHandler))]
    [HarmonyPrefix]
    public static bool CustomActionButtonFixedUpdatePrefix() => !IsLocalPlayerEaten();

    [HarmonyPatch(typeof(Vent), nameof(Vent.CanUse))]
    [HarmonyPrefix]
    public static bool VentCanUsePrefix(ref float __result, [HarmonyArgument(0)] NetworkedPlayerInfo playerInfo)
    {
        if (playerInfo?.Object == null || !MonsterState.IsEaten(playerInfo.PlayerId)) return true;
        __result = float.MaxValue;
        return false;
    }

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.ToggleMapVisible))]
    [HarmonyPrefix]
    public static bool ToggleMapVisiblePrefix() => !IsLocalPlayerEaten();

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CanMove), MethodType.Getter)]
    [HarmonyPrefix]
    public static bool CanMovePrefix(PlayerControl __instance, ref bool __result)
    {
        if (__instance != PlayerControl.LocalPlayer || !MonsterState.IsEaten(__instance.PlayerId)) return true;
        __result = false;
        return false;
    }
    private static bool wasEatenLastFrame;

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    [HarmonyPostfix]
    public static void HudManagerUpdatePostfix()
    {
        var isEaten = IsLocalPlayerEaten();

        if (isEaten)
        {
            foreach (var button in CustomButtonManager.Buttons)
                if (button?.Button != null && button.Button.gameObject.activeSelf)
                    button.Button.gameObject.SetActive(false);

            if (HudManager.Instance != null)
            {
                SetActiveIfPresent(HudManager.Instance.UseButton?.gameObject, false);
                SetActiveIfPresent(HudManager.Instance.ReportButton?.gameObject, false);
                SetActiveIfPresent(HudManager.Instance.KillButton?.gameObject, false);
                SetActiveIfPresent(HudManager.Instance.MapButton?.gameObject, false);
                SetActiveIfPresent(HudManager.Instance.SabotageButton?.gameObject, false);
                SetActiveIfPresent(HudManager.Instance.ImpostorVentButton?.gameObject, false);
            }
        }
        else if (wasEatenLastFrame && HudManager.Instance != null)
        {
            foreach (var button in CustomButtonManager.Buttons)
                if (button?.Button != null) button.Button.gameObject.SetActive(true);

            SetActiveIfPresent(HudManager.Instance.UseButton?.gameObject, true);
            SetActiveIfPresent(HudManager.Instance.ReportButton?.gameObject, true);
            SetActiveIfPresent(HudManager.Instance.KillButton?.gameObject, true);
            SetActiveIfPresent(HudManager.Instance.MapButton?.gameObject, true);
            SetActiveIfPresent(HudManager.Instance.SabotageButton?.gameObject, true);
            SetActiveIfPresent(HudManager.Instance.ImpostorVentButton?.gameObject, true);
        }

        wasEatenLastFrame = isEaten;
    }

    private static void SetActiveIfPresent(GameObject? go, bool active)
    {
        if (go != null) go.SetActive(active);
    }

    private static bool IsLocalPlayerEaten()
    {
        var local = PlayerControl.LocalPlayer;
        return local != null && MonsterState.IsEaten(local.PlayerId);
    }

    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Start))]
    [HarmonyPrefix]
    public static void GameStartPrefix() => MonsterState.ResetAll();

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
    [HarmonyPostfix]
    public static void GameEndPostfix() => MonsterState.ResetAll();
}