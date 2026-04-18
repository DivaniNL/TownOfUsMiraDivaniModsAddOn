using HarmonyLib;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using DivaniMods.Buttons;
using DivaniMods.Options;
using DivaniMods.Roles;
using UnityEngine;
using TMPro;

namespace DivaniMods.Patches;

[HarmonyPatch]
public static class LockdownPatch
{
    private static GameObject? _lockdownTimerObject;
    private static TMPro.TextMeshPro? _lockdownTimerText;
    
    // Only block TASK consoles during a lockdown. Task consoles have a
    // populated `TaskTypes` array; the emergency-meeting button, portal
    // consoles, etc. have `TaskTypes.Length == 0`. Map systems (Admin,
    // Cameras, Vitals, Doors, Comms, Reactor) use SystemConsole, which we
    // deliberately don't patch at all so crewmates keep full access to
    // meetings, info panels and utilities during a lockdown.
    //
    // Sabotage-fix consoles (reactor reset, lights, O2, comms, Fungle
    // reactor, Mushroom Mixup) are also Console instances with TaskTypes
    // set, so we whitelist those explicitly - otherwise the impostor could
    // lockdown the ship AND sabotage, trapping the crew with no way out.
    [HarmonyPatch(typeof(Console), nameof(Console.CanUse))]
    [HarmonyPrefix]
    public static bool ConsoleCanUsePrefix(Console __instance, NetworkedPlayerInfo pc, ref bool canUse, ref bool couldUse, ref float __result)
    {
        if (!LockdownButton.IsLockdownActive) return true;
        if (__instance == null) return true;
        if (__instance.TaskTypes == null || __instance.TaskTypes.Length == 0) return true;
        if (IsSabotageFixConsole(__instance)) return true;

        var playerControl = pc?.Object;
        if (playerControl == null) return true;
        if (playerControl.Data.Role.IsImpostor) return true;

        canUse = false;
        couldUse = false;
        __result = float.MaxValue;
        return false;
    }

    // Safety net: if something bypasses Console.CanUse and tries to open a
    // minigame, only block task minigames. `Minigame.Begin(PlayerTask task)`
    // is invoked with a non-null task for task minigames and `null` for
    // things like the EmergencyMinigame (meeting button). Sabotage-fix
    // minigames are driven by SabotageTask instances - we let those through
    // so the crew can always repair.
    [HarmonyPatch(typeof(Minigame), nameof(Minigame.Begin))]
    [HarmonyPrefix]
    public static bool MinigameBeginPrefix(Minigame __instance, PlayerTask task)
    {
        if (!LockdownButton.IsLockdownActive) return true;
        if (task == null) return true;
        if (task.TryCast<SabotageTask>() != null) return true;

        var localPlayer = PlayerControl.LocalPlayer;
        if (localPlayer == null) return true;
        if (localPlayer.Data.Role.IsImpostor) return true;

        __instance.Close();
        return false;
    }

    private static bool IsSabotageFixConsole(Console console)
    {
        var types = console.TaskTypes;
        if (types == null) return false;
        for (var i = 0; i < types.Length; i++)
        {
            switch (types[i])
            {
                case TaskTypes.ResetReactor:            // Polus/Skeld reactor meltdown
                case TaskTypes.FixLights:               // Electrical lights
                case TaskTypes.FixComms:                // Comms sabotage
                case TaskTypes.RestoreOxy:              // O2 sabotage
                case TaskTypes.StopCharles:             // Fungle reactor
                case TaskTypes.MushroomMixupSabotage:   // Fungle mushroom mixup
                    return true;
            }
        }
        return false;
    }
    
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    [HarmonyPostfix]
    public static void HudManagerUpdatePostfix(HudManager __instance)
    {
        if (__instance == null) return;
        
        var localPlayer = PlayerControl.LocalPlayer;
        if (localPlayer == null || localPlayer.Data == null) return;

        var role = localPlayer.Data.Role;
        if (role == null) return;

        UpdateLockdownButtonTimer();

        if (role.IsImpostor)
        {
            CleanupCrewmateTimer();
            return;
        }
        
        // Hide the timer during meetings / ejection - the countdown is also paused
        // in LockdownButton.LockdownTimerCoroutine so it resumes after the meeting.
        bool inMeeting = MeetingHud.Instance != null || ExileController.Instance != null;
        
        if (LockdownButton.IsLockdownActive && LockdownButton.LockdownTimeRemaining > 0 && !inMeeting)
        {
            ShowLockdownTimer(__instance, LockdownButton.LockdownTimeRemaining);
        }
        else
        {
            CleanupCrewmateTimer();
        }
    }
    
    private static void UpdateLockdownButtonTimer()
    {
        var instance = LockdownButton.Instance;
        if (instance == null) return;
        
        var button = instance.Button;
        if (button == null) return;
        
        if (LockdownButton.IsLockdownActive && LockdownButton.LockdownTimeRemaining > 0)
        {
            button.buttonLabelText.color = new Color(1f, 0.3f, 0.3f);
        }
        else
        {
            button.buttonLabelText.color = Color.white;
        }
    }
    
    private static void ShowLockdownTimer(HudManager hud, float timeRemaining)
    {
        if (_lockdownTimerObject == null || _lockdownTimerText == null)
        {
            CreateTimerObject(hud);
        }
        
        if (_lockdownTimerText != null && _lockdownTimerObject != null)
        {
            _lockdownTimerObject.SetActive(true);
            int seconds = Mathf.CeilToInt(timeRemaining);
            _lockdownTimerText.text = $"<color=#CC3333>LOCKDOWN</color>\n<size=80%>{seconds}s</size>";
        }
    }
    
    private static void CreateTimerObject(HudManager hud)
    {
        if (hud.TaskPanel == null) return;
        
        var existingText = hud.TaskPanel.GetComponentInChildren<TMPro.TextMeshPro>();
        if (existingText == null) return;
        
        _lockdownTimerObject = new GameObject("LockdownTimer");
        _lockdownTimerObject.transform.SetParent(hud.transform);
        _lockdownTimerObject.transform.localPosition = new Vector3(0, 2.2f, -50f);
        _lockdownTimerObject.layer = LayerMask.NameToLayer("UI");
        
        _lockdownTimerText = UnityEngine.Object.Instantiate(existingText, _lockdownTimerObject.transform);
        _lockdownTimerText.transform.localPosition = Vector3.zero;
        _lockdownTimerText.fontSize = 3f;
        _lockdownTimerText.fontStyle = FontStyles.Bold;
        _lockdownTimerText.alignment = TextAlignmentOptions.Center;
        _lockdownTimerText.color = Color.white;
        _lockdownTimerText.text = "";
    }
    
    private static void CleanupCrewmateTimer()
    {
        if (_lockdownTimerObject != null)
        {
            _lockdownTimerObject.SetActive(false);
        }
    }
    
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
    [HarmonyPostfix]
    public static void OnGameEndPostfix()
    {
        LockdownButton.ResetLockdown();
        
        if (_lockdownTimerObject != null)
        {
            UnityEngine.Object.Destroy(_lockdownTimerObject);
            _lockdownTimerObject = null;
            _lockdownTimerText = null;
        }
    }
    
    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.CoBegin))]
    [HarmonyPostfix]
    public static void OnIntroBeginPostfix()
    {
        LockdownButton.ResetLockdown();
        
        if (_lockdownTimerObject != null)
        {
            UnityEngine.Object.Destroy(_lockdownTimerObject);
            _lockdownTimerObject = null;
            _lockdownTimerText = null;
        }
    }
}

/// <summary>
/// Event handler for granting Deadlock charges on kill.
/// </summary>
public static class DeadlockKillEventHandler
{
    [RegisterEvent]
    public static void OnAfterMurder(AfterMurderEvent evt)
    {
        var killer = evt.Source;
        if (killer == null || !killer.AmOwner) return;
        
        if (killer.Data?.Role is not DeadlockRole) return;
        
        var chargesPerKill = LockdownButton.ChargesPerKill;
        if (chargesPerKill <= 0) return;
        
        var button = LockdownButton.Instance;
        if (button == null) return;
        
        button.AddCharges(chargesPerKill);
    }
}
