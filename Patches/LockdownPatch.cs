using HarmonyLib;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using DivaniMods.Assets;
using DivaniMods.Buttons.Impostor.ImpostorSupport;
using DivaniMods.Options;
using DivaniMods.Roles.Impostor.ImpostorSupport;
using DivaniMods.Utilities;
using UnityEngine;

namespace DivaniMods.Patches;

[HarmonyPatch]
public static class LockdownPatch
{
    public const string TimerId = "divani.lockdown";
    private const int TimerPriority = 10;

    private static Sprite? _cachedDeadlockIcon;

    private static Sprite? GetDeadlockRoleIcon()
    {
        if (_cachedDeadlockIcon != null) return _cachedDeadlockIcon;
        try
        {
            _cachedDeadlockIcon = DivaniAssets.DeadlockIcon.LoadAsset();
        }
        catch
        {
            // non-fatal
        }

        return _cachedDeadlockIcon;
    }

    [HarmonyPatch(typeof(Console), nameof(Console.CanUse))]
    [HarmonyPrefix]
    public static bool ConsoleCanUsePrefix(Console __instance, NetworkedPlayerInfo pc, ref bool canUse, ref bool couldUse, ref float __result)
    {
        if (!LockdownButton.IsLockdownActive) return true;
        if (__instance == null) return true;
        if (__instance.AllowImpostor) return true;
        if (IsSabotageFixConsole(__instance)) return true;

        var playerControl = pc?.Object;
        if (playerControl == null) return true;
        if (playerControl.Data.Role.IsImpostor) return true;

        canUse = false;
        couldUse = false;
        __result = float.MaxValue;
        return false;
    }

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
            DivaniTimers.Remove(TimerId);
            return;
        }
        
        bool inMeeting = MeetingHud.Instance || ExileController.Instance;
        
        if (LockdownButton.IsLockdownActive && LockdownButton.LockdownTimeRemaining > 0 && !inMeeting)
        {
            DivaniTimers.Set(
                TimerId,
                "<b><color=#CC3333>LOCKDOWN</color></b>",
                GetDeadlockRoleIcon(),
                Mathf.Max(0f, LockdownButton.LockdownTimeRemaining),
                useLocalTimeDelta: false,
                priority: TimerPriority);
        }
        else
        {
            DivaniTimers.Remove(TimerId);
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
    
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
    [HarmonyPostfix]
    public static void OnGameEndPostfix()
    {
        LockdownButton.ResetLockdown();
    }
    
    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.CoBegin))]
    [HarmonyPostfix]
    public static void OnIntroBeginPostfix()
    {
        LockdownButton.ResetLockdown();
    }
}

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
