using System.Collections.Generic;
using System.Linq;
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

namespace DivaniMods.Patches;

[HarmonyPatch]
public static class PlagueDoctorPatch
{
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

    [RegisterEvent]
    public static void OnRoundStart(RoundStartEvent evt)
    {
        TryClearStalePlagueDoctorStateIfNeeded();
        PlagueDoctorRole.OnRoundStart();
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

        bool gameplayActive = !MeetingHud.Instance
                              && !ExileController.Instance
                              && !PlagueDoctorRole.MeetingFlag;
        if (gameplayActive)
        {
            PlagueDoctorRole.TickImmunityTimer(Time.deltaTime);
        }

        bool localIsFullyDead = DeathHandlerModifier.IsFullyDead(localPlayer);

        if (isLocalPD || localIsFullyDead)
        {
            UpdateStatusText();
        }
        else
        {
            ClearStatusText();
        }

        PlagueDoctorRole.TryShowInfectionWarning();

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

        var text = string.Empty;

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
            if (PlagueDoctorRole.IsPlagueDoctor(p)) continue;

            var entry = $"{TrimName(p.Data.PlayerName)}: ";

            if (PlagueDoctorRole.IsInfected(p))
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
                text += $"<pos=90%>{entries[rightIndex]}";
            }
            text += "\n";
        }

        return text;
    }

    private static string TrimName(string playerName)
    {
        return playerName;
    }
}
