using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.GameEnd;
using MiraAPI.GameOptions;
using TownOfUs.Events;
using DivaniMods.GameOver;
using DivaniMods.Options;
using DivaniMods.Roles;
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
        PlagueDoctorRole.OnRoundStart();
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
            DivaniPlugin.Instance.Log.LogInfo($"PlagueDoctorPatch: Infecting killer {killer.PlayerId} on PD death");
            PlagueDoctorRole.RpcSetInfected(localPlayer, killer.PlayerId);

            // Immediately check the win condition on PD's client after the killer
            // is infected. Without this, there is a race between the PD dying and
            // the next HudManager.Update tick that can leave the game in a weird
            // state (e.g. impostor sees Defeat but PD win screen never triggers).
            CheckWinCondition();
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
        if (PlagueDoctorRole.PlagueDoctorPlayer == null) return;
        
        var localPlayer = PlayerControl.LocalPlayer;
        if (localPlayer == null) return;

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
        
        bool isLocalPD = localPlayer == PlagueDoctorRole.PlagueDoctorPlayer;
        bool canWinDead = PlagueDoctorRole.CanWinWhileDead;
        bool pdIsDead = PlagueDoctorRole.PlagueDoctorPlayer.Data?.IsDead ?? false;
        bool localIsDead = localPlayer.Data?.IsDead ?? false;
        
        if (isLocalPD)
        {
            if (!pdIsDead || canWinDead)
            {
                RunInfectionSpread();
                CheckWinCondition();
            }
            
            UpdateStatusText();
        }
        else if (localIsDead)
        {
            UpdateStatusText();
        }

        // Safety net: the host also evaluates the win condition, so that if the
        // PD client is dead/disconnected/laggy (e.g. imp just killed PD) the game
        // still ends correctly instead of getting stuck in a weird state.
        if (AmongUsClient.Instance != null && AmongUsClient.Instance.AmHost)
        {
            HostCheckWinCondition();
        }
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

    private static void CheckWinCondition()
    {
        var localPlayer = PlayerControl.LocalPlayer;
        if (localPlayer == null) return;
        if (PlagueDoctorRole.PlagueDoctorPlayer == null) return;
        
        // Only the PD's client should trigger the win
        if (localPlayer != PlagueDoctorRole.PlagueDoctorPlayer) return;
        
        // Don't trigger if already triggered
        if (PlagueDoctorRole.TriggerPlagueDoctorWin) return;
        
        if (PlagueDoctorRole.WinConditionMet())
        {
            DivaniPlugin.Instance.Log.LogInfo("PlagueDoctorPatch: Win condition met, calling RpcTriggerPlagueDoctorWin");
            
            // Use the RPC to sync to all clients and trigger game end on host
            PlagueDoctorRole.RpcTriggerPlagueDoctorWin(localPlayer);
        }
    }

    /// <summary>
    /// Host-side safety net for win triggering. Runs on the host regardless of
    /// whether the host is the PD. This ensures the game ends correctly if the
    /// PD's client is unable to trigger the win (e.g. just died and their client
    /// is transitioning). If the win condition is not met, this is a no-op.
    /// </summary>
    private static void HostCheckWinCondition()
    {
        if (PlagueDoctorRole.PlagueDoctorPlayer == null) return;
        if (PlagueDoctorRole.TriggerPlagueDoctorWin) return;
        if (MeetingHud.Instance != null) return;
        if (PlagueDoctorRole.MeetingFlag) return;

        if (PlagueDoctorRole.WinConditionMet())
        {
            var localPlayer = PlayerControl.LocalPlayer;
            if (localPlayer == null) return;
            DivaniPlugin.Instance.Log.LogInfo("PlagueDoctorPatch: Host safety-net detected win condition, triggering win RPC");
            // Use the RPC so all clients get TriggerPlagueDoctorWin=true and
            // VerifyCondition passes everywhere. Sender must be the local player
            // because Reactor's MethodRpc dispatches via the sender's NetId.
            PlagueDoctorRole.RpcTriggerPlagueDoctorWin(localPlayer);
        }
    }

    private static void UpdateStatusText()
    {
        if (MeetingHud.Instance != null)
        {
            if (PlagueDoctorRole.StatusText != null)
            {
                PlagueDoctorRole.StatusText.gameObject.SetActive(false);
            }
            return;
        }

        if (PlagueDoctorRole.StatusText == null)
        {
            CreateStatusText();
        }

        if (PlagueDoctorRole.StatusText == null) return;

        PlagueDoctorRole.StatusText.gameObject.SetActive(true);
        
        var infectDuration = OptionGroupSingleton<PlagueDoctorOptions>.Instance.InfectDuration;

        var text = string.Empty;

        // Green immunity countdown sits above the infection progress list and
        // disappears once the grace period is over.
        if (PlagueDoctorRole.ImmunityTimer > 0f)
        {
            text += $"<color=#00FF00>Players immune to non-direct infection for: {PlagueDoctorRole.ImmunityTimer:F1}seconds</color>\n";
        }

        text += "<color=#FFC000>[Infection Progress]</color>\n";

        foreach (var p in PlayerControl.AllPlayerControls)
        {
            if (p == null || p == PlagueDoctorRole.PlagueDoctorPlayer) continue;
            if (PlagueDoctorRole.DeadPlayers.ContainsKey(p.PlayerId) && PlagueDoctorRole.DeadPlayers[p.PlayerId]) continue;
            if (p.Data == null || p.Data.IsDead) continue;

            text += $"{p.Data.PlayerName}: ";

            if (PlagueDoctorRole.InfectedPlayers.ContainsKey(p.PlayerId))
            {
                text += "<color=#FF0000>INFECTED</color>";
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
                text += $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{(percent * 100f):F1}%</color>";
            }

            text += "\n";
        }

        PlagueDoctorRole.StatusText.text = text;
    }

    private static void CreateStatusText()
    {
        if (HudManager.Instance?.roomTracker == null) return;

        var gameObject = UnityEngine.Object.Instantiate(HudManager.Instance.roomTracker.gameObject);
        gameObject.transform.SetParent(HudManager.Instance.transform);
        gameObject.SetActive(true);

        var roomTracker = gameObject.GetComponent<RoomTracker>();
        if (roomTracker != null)
        {
            UnityEngine.Object.DestroyImmediate(roomTracker);
        }

        PlagueDoctorRole.StatusText = gameObject.GetComponent<TMPro.TMP_Text>();

        var aliveCount = PlayerControl.AllPlayerControls.ToArray()
            .Count(x => x != null && x.Data != null && !x.Data.IsDead && x != PlagueDoctorRole.PlagueDoctorPlayer);

        gameObject.transform.localPosition = new Vector3(-2.7f, -0.1f - aliveCount * 0.07f, gameObject.transform.localPosition.z);
        PlagueDoctorRole.StatusText.transform.localScale = Vector3.one;
        PlagueDoctorRole.StatusText.fontSize = 1.5f;
        PlagueDoctorRole.StatusText.fontSizeMin = 1.5f;
        PlagueDoctorRole.StatusText.fontSizeMax = 1.5f;
        PlagueDoctorRole.StatusText.alignment = TMPro.TextAlignmentOptions.BottomLeft;
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

    /// <summary>
    /// Handle winners list when PD wins - ensures PD is in winners even when dead.
    /// Also removes PD from winners when they didn't win.
    /// </summary>
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last)]
    public static void OnGameEndHandlePD(ref EndGameResult endGameResult)
    {
        DivaniPlugin.Instance.Log.LogInfo($"PlagueDoctorPatch: OnGameEndHandlePD running");
        if (CustomGameOver.Instance is PlagueDoctorGameOver)
        {
            EnsurePDInWinners();
        }
        else
        {
            RemovePDFromWinnersInternal();
        }
    }
    
    /// <summary>
    /// Handle winners list at SetEverythingUp time (runs right before end game display).
    /// </summary>
    [HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.SetEverythingUp))]
    [HarmonyPrefix]
    [HarmonyPriority(Priority.Last)]
    public static void HandlePDWinners()
    {
        DivaniPlugin.Instance.Log.LogInfo($"PlagueDoctorPatch: HandlePDWinners (SetEverythingUp) running. CustomGameOver: {CustomGameOver.Instance?.GetType().Name ?? "null"}");
        if (CustomGameOver.Instance is PlagueDoctorGameOver)
        {
            EnsurePDInWinners();
        }
        else
        {
            RemovePDFromWinnersInternal();
        }
    }
    
    /// <summary>
    /// Ensures PD is in the winners list - important when PD won while dead.
    /// Uses fresh data lookup via GameData in case PlayerControl references are stale.
    /// </summary>
    private static void EnsurePDInWinners()
    {
        if (PlagueDoctorRole.PlagueDoctorPlayer == null)
        {
            DivaniPlugin.Instance.Log.LogWarning("PlagueDoctorPatch: Cannot ensure PD in winners - PlagueDoctorPlayer is null");
            return;
        }
        
        var pdPlayerId = PlagueDoctorRole.PlagueDoctorPlayer.PlayerId;
        
        // Try to get fresh data from GameData (more reliable than cached PlayerControl reference)
        NetworkedPlayerInfo? pdData = null;
        if (GameData.Instance != null)
        {
            pdData = GameData.Instance.GetPlayerById(pdPlayerId);
        }
        
        // Fallback: use the cached reference
        if (pdData == null)
        {
            pdData = PlagueDoctorRole.PlagueDoctorPlayer.Data;
        }
        
        if (pdData == null)
        {
            DivaniPlugin.Instance.Log.LogError($"PlagueDoctorPatch: Cannot find PD data for PlayerId {pdPlayerId}");
            return;
        }
        
        if (EndGameResult.CachedWinners == null)
        {
            DivaniPlugin.Instance.Log.LogWarning("PlagueDoctorPatch: CachedWinners is null");
            return;
        }
        
        // Check if PD is already in winners
        bool pdInWinners = false;
        foreach (var winner in EndGameResult.CachedWinners)
        {
            if (winner.PlayerName == pdData.PlayerName)
            {
                pdInWinners = true;
                break;
            }
        }
        
        if (!pdInWinners)
        {
            // Clear any other winners (PD wins solo) and add PD
            EndGameResult.CachedWinners.Clear();
            var winnerData = new CachedPlayerData(pdData);
            EndGameResult.CachedWinners.Add(winnerData);
            DivaniPlugin.Instance.Log.LogInfo($"PlagueDoctorPatch: Added PD ({pdData.PlayerName}) to winners. IsDead: {pdData.IsDead}");
        }
        else
        {
            DivaniPlugin.Instance.Log.LogInfo($"PlagueDoctorPatch: PD already in winners list");
        }
    }
    
    private static void RemovePDFromWinnersInternal()
    {
        DivaniPlugin.Instance.Log.LogInfo($"PlagueDoctorPatch: RemovePDFromWinnersInternal. CustomGameOver.Instance type: {CustomGameOver.Instance?.GetType().Name ?? "null"}");
        
        // If PD didn't win (our custom game over wasn't triggered), remove them from winners
        if (CustomGameOver.Instance is not PlagueDoctorGameOver)
        {
            if (PlagueDoctorRole.PlagueDoctorPlayer == null)
            {
                DivaniPlugin.Instance.Log.LogInfo("PlagueDoctorPatch: PlagueDoctorPlayer is null, skipping removal");
                return;
            }
            
            var pdName = PlagueDoctorRole.PlagueDoctorPlayer.Data?.PlayerName;
            var pdPlayerId = PlagueDoctorRole.PlagueDoctorPlayer.PlayerId;
            if (string.IsNullOrEmpty(pdName))
            {
                DivaniPlugin.Instance.Log.LogInfo("PlagueDoctorPatch: PD name is null/empty, skipping removal");
                return;
            }
            
            if (EndGameResult.CachedWinners == null)
            {
                DivaniPlugin.Instance.Log.LogInfo("PlagueDoctorPatch: CachedWinners is null");
                return;
            }
            
            DivaniPlugin.Instance.Log.LogInfo($"PlagueDoctorPatch: Looking for PD ({pdName}, ID:{pdPlayerId}) in {EndGameResult.CachedWinners.Count} winners");
            
            var winnersToRemove = new List<CachedPlayerData>();
            
            foreach (var winner in EndGameResult.CachedWinners)
            {
                DivaniPlugin.Instance.Log.LogInfo($"PlagueDoctorPatch: Winner in list: {winner.PlayerName}");
                if (winner.PlayerName == pdName)
                {
                    winnersToRemove.Add(winner);
                    DivaniPlugin.Instance.Log.LogInfo($"PlagueDoctorPatch: Marking PD ({pdName}) for removal from winners list");
                }
            }
            
            foreach (var winner in winnersToRemove)
            {
                EndGameResult.CachedWinners.Remove(winner);
                DivaniPlugin.Instance.Log.LogInfo($"PlagueDoctorPatch: Removed PD ({winner.PlayerName}) from winners list. New count: {EndGameResult.CachedWinners.Count}");
            }
            
            if (winnersToRemove.Count == 0)
            {
                DivaniPlugin.Instance.Log.LogInfo("PlagueDoctorPatch: PD was not found in winners list");
            }
        }
        else
        {
            DivaniPlugin.Instance.Log.LogInfo("PlagueDoctorPatch: PD won, not removing from winners");
        }
    }
}
