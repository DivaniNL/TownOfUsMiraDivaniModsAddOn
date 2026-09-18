using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.GameOptions;
using TownOfUs.Events;
using TownOfUs.Modifiers;
using DivaniMods.Options;
using DivaniMods.Roles.Neutral.NeutralEvil;
using UnityEngine;
using TownOfUs.Modules;

namespace DivaniMods.Patches;

[HarmonyPatch]
public static class PlagueDoctorPatch
{
    private static readonly StringBuilder StatusBuilder = new(512);

    [RegisterEvent]
    public static void OnMeetingStart(StartMeetingEvent evt)
    {
        PlagueDoctorRole.HandleMeetingStart();
    }

    [RegisterEvent]
    public static void OnRoundStart(RoundStartEvent evt)
    {
        TryClearStalePlagueDoctorStateIfNeeded();
        PlagueDoctorRole.OnRoundStart(evt.TriggeredByIntro);
    }

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
            PlagueDoctorRole.InfectPlayer(killer);
        }
    }

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CoStartGame))]
    [HarmonyPostfix]
    public static void ResetOnGameStart()
    {
        PlagueDoctorRole.ClearAndReload();
    }

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

        PlagueDoctorRole.FreezeInfectionStates();

        bool gameplayActive = !MeetingHud.Instance
                              && !ExileController.Instance
                              && !PlagueDoctorRole.MeetingFlag;
        if (gameplayActive)
        {
            PlagueDoctorRole.TickImmunityTimer(Time.deltaTime);
        }

        bool localIsFullyDead = GameHistory.IsFullyDead(localPlayer);



        if (isLocalPD || localIsFullyDead)
        {
            UpdateStatusText();
        }
        else
        {
            ClearStatusText();
        }

        if (Time.time - _lastWarningCheck >= 1f)
        {
            PlagueDoctorRole.TryShowInfectionWarning();
            _lastWarningCheck = Time.time;
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

    private static void ClearStatusText()
    {
        var localPlayer = PlayerControl.LocalPlayer;
        if (localPlayer == null || localPlayer.myTasks == null) return;

        for (var i = localPlayer.myTasks.Count - 1; i >= 0; i--)
        {
            var task = localPlayer.myTasks[i];
            if (task != null && task.name == "PlagueDoctorInfectionStatus")
            {
                localPlayer.RemoveTask(task);
            }
        }
    }

    private static string BuildInfectionStatusText()
    {
        var infectDuration = OptionGroupSingleton<PlagueDoctorOptions>.Instance.InfectDuration.Value;
        StatusBuilder.Clear();

        if (PlagueDoctorRole.ImmunityTimer > 0f)
        {
            StatusBuilder.Append("<color=#00FF00>Players immune to non-direct infection for: ")
                .Append(PlagueDoctorRole.ImmunityTimer.ToString("F1"))
                .Append("seconds</color>\n");
        }

        StatusBuilder.Append("<color=#FFC000>[Infection Progress]</color>\n");

        var leftColumn = new StringBuilder(256);
        var rightColumn = new StringBuilder(256);
        var playerCount = 0;

        foreach (var p in PlayerControl.AllPlayerControls)
        {
            if (p == null || p == PlagueDoctorRole.PlagueDoctorPlayer) continue;
            if (p.Data == null) continue;
            if (PlagueDoctorRole.IsKnownDead(p)) continue;
            if (PlagueDoctorRole.IsPlagueDoctor(p)) continue;

            var builder = (playerCount % 2 == 0) ? leftColumn : rightColumn;
            playerCount++;

            builder.Append(TrimName(p.Data.PlayerName));
            builder.Append(": ");

            var infected = PlagueDoctorRole.GetDisplayedInfectionState(p, out var progress);

            if (infected)
            {
                builder.Append("<color=#FF0000>INFECTED</color>");
            }
            else
            {
                var percent = Mathf.Clamp01(progress / infectDuration);
                Color color;
                if (percent < 0.5f)
                    color = Color.Lerp(Color.green, Color.yellow, percent * 2f);
                else
                    color = Color.Lerp(Color.yellow, Color.red, (percent * 2f) - 1f);

                builder.Append("<color=#")
                    .Append(ColorUtility.ToHtmlStringRGB(color))
                    .Append('>')
                    .Append((percent * 100f).ToString("F0"))
                    .Append("%</color>");
            }

            builder.Append('\n');
        }

        if (leftColumn.Length > 0)
        {
            StatusBuilder.Append(leftColumn);
            if (rightColumn.Length > 0)
            {
                StatusBuilder.Append("<pos=90%>")
                    .Append(rightColumn);
            }
        }
        else if (rightColumn.Length > 0)
        {
            StatusBuilder.Append(rightColumn);
        }

        return StatusBuilder.ToString();
    }

    private static string TrimName(string playerName)
    {
        return playerName;
    }
}
