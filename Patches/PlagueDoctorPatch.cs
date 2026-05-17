using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.GameOptions;
using TownOfUs.Events;
using DivaniMods.Options;
using DivaniMods.Roles.Neutral.NeutralEvil;
using UnityEngine;

namespace DivaniMods.Patches;

[HarmonyPatch]
public static class PlagueDoctorPatch
{
    private static float _lastProgressUpdate;

    [RegisterEvent]
    public static void OnMeetingStart(StartMeetingEvent evt)
    {
        PlagueDoctorRole.HandleMeetingStart();
    }

    [RegisterEvent]
    public static void OnMeetingEnd(EndMeetingEvent evt)
    {
        PlagueDoctorRole.OnMeetingEnd();
    }

    /// <summary>
    /// Fires when the round actually starts (players regain control after the
    /// ejection animation). This is when the post-meeting immunity grace period
    /// starts - if we started it at EndMeetingEvent it would tick down during
    /// the ejection sequence and be gone by the time anyone can move.
    /// </summary>
    [RegisterEvent]
    public static void OnRoundStart(RoundStartEvent evt)
    {
        TryClearStalePlagueDoctorStateIfNeeded();
        PlagueDoctorRole.OnRoundStart();
    }

    /// <summary>
    /// If the stored PD is still alive but no longer has <see cref="PlagueDoctorRole"/> (e.g. recruited Impostor),
    /// tear down infection state and HUD. Dead PD may keep ghost win logic — do not clear when <see cref="PlayerControl.Data.IsDead"/>.
    /// </summary>
    internal static void TryClearStalePlagueDoctorStateIfNeeded()
    {
        if (PlagueDoctorRole.PlagueDoctorPlayer == null)
        {
            return;
        }

        var pd = PlagueDoctorRole.PlagueDoctorPlayer;
        if (pd.Data == null || pd.Data.IsDead)
        {
            return;
        }

        if (pd.Data.Role is PlagueDoctorRole)
        {
            return;
        }

        PlagueDoctorRole.ClearAndReload();
    }

    internal static void ResetInfectionSpreadThrottle()
    {
        _lastProgressUpdate = 0f;
    }

    /// <summary>
    /// Handle Plague Doctor death - infect the killer if option is enabled.
    /// </summary>
    [RegisterEvent]
    public static void OnAfterMurder(AfterMurderEvent evt)
    {
        var victim = evt.Target;
        var killer = evt.Source;

        if (victim == null || killer == null) return;

        bool victimIsPD = victim == PlagueDoctorRole.PlagueDoctorPlayer;

        if (!victimIsPD) return;

        var localPlayer = PlayerControl.LocalPlayer;
        bool isLocalPD = victim.AmOwner ||
                         (localPlayer != null && PlagueDoctorRole.PlagueDoctorPlayer != null &&
                          localPlayer.PlayerId == PlagueDoctorRole.PlagueDoctorPlayer.PlayerId);

        if (!isLocalPD || localPlayer == null) return;

        var infectKiller = OptionGroupSingleton<PlagueDoctorOptions>.Instance.InfectKiller;

        if (infectKiller)
        {
            PlagueDoctorRole.RpcSetInfected(localPlayer, killer.PlayerId);
        }
    }

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CoStartGame))]
    [HarmonyPostfix]
    public static void ResetOnGameStart()
    {
        PlagueDoctorRole.ClearAndReload();
        _lastProgressUpdate = 0f;
    }

    // Main update loop - runs infection spread and win check
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    [HarmonyPostfix]
    public static void HudManagerUpdate(HudManager __instance)
    {
        var localPlayer = PlayerControl.LocalPlayer;
        if (localPlayer == null || localPlayer.Data == null) return;

        bool isLocalPD = localPlayer.Data.Role is PlagueDoctorRole ||
                         (PlagueDoctorRole.PlagueDoctorPlayer != null &&
                          localPlayer.PlayerId == PlagueDoctorRole.PlagueDoctorPlayer.PlayerId);

        if (PlagueDoctorRole.PlagueDoctorPlayer == null) return;

        // Tick immunity timer every frame while gameplay is active. Don't tick
        // during meetings or the ejection sequence, or the grace period would
        // silently drain before players can actually move.
        bool gameplayActive = MeetingHud.Instance == null
                              && ExileController.Instance == null
                              && !PlagueDoctorRole.MeetingFlag;
        if (gameplayActive)
        {
            PlagueDoctorRole.TickImmunityTimer(Time.deltaTime);
        }
        
        bool canWinDead = PlagueDoctorRole.CanWinWhileDead;
        bool pdIsDead = PlagueDoctorRole.PlagueDoctorPlayer?.Data?.IsDead ?? false;
        bool localIsDead = localPlayer.Data?.IsDead ?? false;
        
        if (isLocalPD)
        {
            if (!pdIsDead || canWinDead)
            {
                RunInfectionSpread();
            }

            UpdateStatusText();
        }
        else if (localIsDead)
        {
            UpdateStatusText();
        }

        // The actual win condition is evaluated on the host by TownOfUs's
        // NeutralRoleWinCondition (registered in WinConditionRegistry), which
        // calls PlagueDoctorRole.WinConditionMet() and then fires
        // CustomGameOver.Trigger<NeutralGameOver>(...) - Mira then broadcasts
        // the end game to every client so they all see the right win screen.
    }

    private static void RunInfectionSpread()
    {
        if (PlagueDoctorRole.MeetingFlag || MeetingHud.Instance != null) return;
        // Respect the post-meeting immunity grace period.
        if (PlagueDoctorRole.ImmunityTimer > 0f) return;
        
        var localPlayer = PlayerControl.LocalPlayer;
        if (localPlayer == null) return;

        var infectDistance = OptionGroupSingleton<PlagueDoctorOptions>.Instance.InfectDistance;
        var infectDuration = OptionGroupSingleton<PlagueDoctorOptions>.Instance.InfectDuration;

        foreach (var target in PlayerControl.AllPlayerControls)
        {
            if (target == null || target == PlagueDoctorRole.PlagueDoctorPlayer) continue;
            if (target.Data == null || target.Data.IsDead) continue;
            if (target.inVent) continue;
            if (PlagueDoctorRole.InfectedPlayers.ContainsKey(target.PlayerId)) continue;

            if (!PlagueDoctorRole.InfectionProgress.ContainsKey(target.PlayerId))
            {
                PlagueDoctorRole.InfectionProgress[target.PlayerId] = 0f;
            }

            foreach (var infectedId in PlagueDoctorRole.InfectedPlayers.Keys.ToList())
            {
                var source = GetPlayerById(infectedId);
                if (source == null || source.Data == null || source.Data.IsDead) continue;

                var distance = Vector3.Distance(source.transform.position, target.transform.position);
                var blocked = PhysicsHelpers.AnythingBetween(
                    source.GetTruePosition(),
                    target.GetTruePosition(),
                    Constants.ShipAndObjectsMask,
                    false);

                if (distance <= infectDistance && !blocked)
                {
                    PlagueDoctorRole.InfectionProgress[target.PlayerId] += Time.deltaTime;

                    if (Time.time - _lastProgressUpdate > 0.5f)
                    {
                        PlagueDoctorRole.RpcUpdateInfectionProgress(localPlayer, target.PlayerId, PlagueDoctorRole.InfectionProgress[target.PlayerId]);
                        _lastProgressUpdate = Time.time;
                    }

                    break;
                }
            }

            if (PlagueDoctorRole.InfectionProgress[target.PlayerId] >= infectDuration)
            {
                PlagueDoctorRole.RpcSetInfected(localPlayer, target.PlayerId);
            }
        }
    }

    private static void UpdateStatusText()
    {
        var localPlayer = PlayerControl.LocalPlayer;
        if (localPlayer == null) return;

        var statusTask = PlayerTask.GetOrCreateTask<ImportantTextTask>(localPlayer, 1);
        statusTask.name = "PlagueDoctorInfectionStatus";
        statusTask.Text = BuildInfectionStatusText();
    }

    private static string BuildInfectionStatusText()
    {
        var infectDuration = OptionGroupSingleton<PlagueDoctorOptions>.Instance.InfectDuration;

        var text = string.Empty;

        // Green immunity countdown sits above the infection progress list and
        // disappears once the grace period is over.
        if (PlagueDoctorRole.ImmunityTimer > 0f)
        {
            text += $"<color=#00FF00>Players immune to non-direct infection for: {PlagueDoctorRole.ImmunityTimer:F1}seconds</color>\n";
        }

        text += "<color=#FFC000>[Infection Progress]</color>\n";

        var entries = new List<string>();
        foreach (var p in PlayerControl.AllPlayerControls)
        {
            if (p == null || p == PlagueDoctorRole.PlagueDoctorPlayer) continue;
            if (PlagueDoctorRole.DeadPlayers.ContainsKey(p.PlayerId) && PlagueDoctorRole.DeadPlayers[p.PlayerId]) continue;
            if (p.Data == null || p.Data.IsDead) continue;

            var entry = $"{TrimName(p.Data.PlayerName)}: ";

            if (PlagueDoctorRole.InfectedPlayers.ContainsKey(p.PlayerId))
            {
                entry += "<color=#FF0000>INFECTED</color>";
            }
            else
            {
                var progress = PlagueDoctorRole.InfectionProgress.GetValueOrDefault(p.PlayerId, 0f);
                var percent = Mathf.Clamp01(progress / infectDuration);
                Color color;
                if (percent < 0.5f)
                    color = Color.Lerp(Color.green, Color.yellow, percent * 2f);
                else
                    color = Color.Lerp(Color.yellow, Color.red, (percent * 2f) - 1f);
                entry += $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{(percent * 100f):F0}%</color>";
            }

            entries.Add(entry);
        }

        var splitIndex = (entries.Count + 1) / 2;
        for (var i = 0; i < splitIndex; i++)
        {
            text += entries[i];
            var rightIndex = i + splitIndex;
            if (rightIndex < entries.Count)
            {
                text += $"<pos=80%>{entries[rightIndex]}";
            }
            text += "\n";
        }

        return text;
    }

    private static string TrimName(string playerName)
    {
        return playerName;
    }

    private static PlayerControl? GetPlayerById(byte id)
    {
        foreach (var p in PlayerControl.AllPlayerControls)
        {
            if (p != null && p.PlayerId == id)
            {
                return p;
            }
        }
        return null;
    }
}
