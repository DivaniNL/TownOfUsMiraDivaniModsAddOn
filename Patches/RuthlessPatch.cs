using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Mira;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using MiraAPI.Utilities;
using DivaniMods.Modifiers;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Crewmate;

namespace DivaniMods.Patches;

/// <summary>
/// Ruthless modifier: Impostor can kill through shields (Medic, GA, Survivor, first-death shield, etc.).
/// Veterans on alert still counter-kill the impostor normally.
/// 
/// Strategy:
/// 1. EARLY button click handler: Set bypass flag BEFORE TOU checks shields
/// 2. LATE button click handler: UnCancel if TOU cancelled it (for shields only)
/// 3. EARLY murder handler: Set bypass flag (backup)
/// 4. LATE murder handler: UnCancel if TOU cancelled it (for shields only)
/// 5. Harmony patches on TOU's shield RPCs to suppress flash notifications
/// </summary>
public static class RuthlessEventHandler
{
    private static Type? _baseShieldModifierType;
    private static Type? _invulnerabilityModifierType;
    private static bool _typesInitialized;
    
    public static bool ShouldBypassProtection;
    
    private static void InitializeTypes()
    {
        if (_typesInitialized) return;
        _typesInitialized = true;
        
        try
        {
            var assembly = Assembly.Load("TownOfUsMira");
            if (assembly == null) return;
            
            foreach (var type in assembly.GetTypes())
            {
                if (type.Name == "BaseShieldModifier")
                    _baseShieldModifierType = type;
                if (type.Name == "InvulnerabilityModifier")
                    _invulnerabilityModifierType = type;
            }
            
            DivaniPlugin.Instance.Log.LogInfo($"Ruthless: Found BaseShieldModifier: {_baseShieldModifierType != null}, InvulnerabilityModifier: {_invulnerabilityModifierType != null}");
        }
        catch (Exception ex)
        {
            DivaniPlugin.Instance.Log.LogError($"Ruthless: Failed to load TOU types: {ex.Message}");
        }
    }

    /// <summary>
    /// Il2CppInterop uses wrapper CLR types; <see cref="Type.IsAssignableFrom"/> against types from TownOfUsMira.dll often fails.
    /// Walk the runtime type chain by name (same approach as thief / Harmony patches).
    /// </summary>
    private static bool TypeChainContains(Type? t, string typeName)
    {
        while (t != null)
        {
            if (t.Name == typeName)
                return true;
            t = t.BaseType;
        }

        return false;
    }
    
    private static bool IsProtected(PlayerControl target)
    {
        InitializeTypes();
        
        // Fast path: Mira generic lookup matches gameplay modifiers reliably.
        if (target.HasModifier<MedicShieldModifier>())
            return true;

        // First-death shield is NOT a BaseShieldModifier (ExcludedGameModifier); still blocks kills until bypassed.
        if (target.HasModifier<FirstDeadShield>())
            return true;

        foreach (var mod in target.GetModifiers<BaseModifier>())
        {
            var modType = mod.GetType();

            if (TypeChainContains(modType, "BaseShieldModifier"))
                return true;

            if (_baseShieldModifierType != null && _baseShieldModifierType.IsAssignableFrom(modType))
                return true;

            if (_invulnerabilityModifierType != null && _invulnerabilityModifierType.IsAssignableFrom(modType))
                return true;

            if (TypeChainContains(modType, "InvulnerabilityModifier"))
                return true;
        }
        
        return false;
    }
    
    private static bool CheckRuthlessKill(PlayerControl? source, PlayerControl? target, string context)
    {
        ShouldBypassProtection = false;
        
        if (source == null || target == null) return false;
        
        bool hasRuthless = source.HasModifier<RuthlessModifier>();
        
        if (!hasRuthless) return false;
        
        bool isProtected = IsProtected(target);
        
        DivaniPlugin.Instance.Log.LogInfo($"Ruthless {context}: {source.Data?.PlayerName} -> {target.Data?.PlayerName}, Protected: {isProtected}");
        
        // Only bypass shields, NOT veteran alert
        if (isProtected)
        {
            ShouldBypassProtection = true;
            DivaniPlugin.Instance.Log.LogInfo($"Ruthless {context}: Marked for bypass (shield)");
            return true;
        }
        
        return false;
    }

    /// <summary>
    /// EARLY button click: Set bypass flag BEFORE TOU's handler
    /// </summary>
    [RegisterEvent(-1000)]
    public static void OnButtonClickEarly(MiraButtonClickEvent evt)
    {
        var button = evt.Button;
        if (button == null) return;
        
        var buttonType = button.GetType();
        var isKillButton = buttonType.GetInterfaces().Any(i => i.Name == "IKillButton");
        if (!isKillButton) return;
        
        var targetProp = buttonType.GetProperty("Target");
        var target = targetProp?.GetValue(button) as PlayerControl;
        if (target == null) return;
        
        var source = PlayerControl.LocalPlayer;
        CheckRuthlessKill(source, target, "BUTTON_EARLY");
    }

    /// <summary>
    /// LATE button click: UnCancel if TOU cancelled it and we have Ruthless bypass
    /// </summary>
    [RegisterEvent(1000)]
    public static void OnButtonClickLate(MiraButtonClickEvent evt)
    {
        if (!ShouldBypassProtection) return;
        
        DivaniPlugin.Instance.Log.LogInfo($"Ruthless BUTTON_LATE: IsCancelled: {evt.IsCancelled}");
        
        if (evt.IsCancelled)
        {
            DivaniPlugin.Instance.Log.LogInfo($"Ruthless BUTTON_LATE: UnCancelling button click event");
            evt.UnCancel();
        }
    }

    /// <summary>
    /// EARLY murder: Backup check in case button click didn't fire
    /// </summary>
    [RegisterEvent(-1000)]
    public static void OnBeforeMurderEarly(BeforeMurderEvent evt)
    {
        if (!ShouldBypassProtection)
        {
            CheckRuthlessKill(evt.Source, evt.Target, "MURDER_EARLY");
        }
    }

    /// <summary>
    /// LATE murder: UnCancel if TOU cancelled it and we have Ruthless bypass
    /// </summary>
    [RegisterEvent(1000)]
    public static void OnBeforeMurderLate(BeforeMurderEvent evt)
    {
        if (!ShouldBypassProtection) return;
        
        DivaniPlugin.Instance.Log.LogInfo($"Ruthless MURDER_LATE: IsCancelled: {evt.IsCancelled}");
        
        if (evt.IsCancelled)
        {
            DivaniPlugin.Instance.Log.LogInfo($"Ruthless MURDER_LATE: UnCancelling murder event");
            evt.UnCancel();
        }
    }

    /// <summary>
    /// After murder: Log death info state for debugging
    /// </summary>
    [RegisterEvent(-100)]
    public static void OnAfterMurderDebug(AfterMurderEvent evt)
    {
        var target = evt.Target;
        var source = evt.Source;
        var hasDeathHandler = target.HasModifier<TownOfUs.Modifiers.DeathHandlerModifier>();
        DivaniPlugin.Instance.Log.LogInfo($"[DEBUG] AfterMurder: {source.Data?.PlayerName} killed {target.Data?.PlayerName}, hasDeathHandler: {hasDeathHandler}");
        
        if (hasDeathHandler)
        {
            var dh = target.GetModifier<TownOfUs.Modifiers.DeathHandlerModifier>();
            DivaniPlugin.Instance.Log.LogInfo($"[DEBUG] DeathHandler: CauseOfDeath={dh?.CauseOfDeath}, KilledBy={dh?.KilledBy}, LockInfo={dh?.LockInfo}");
        }
    }
    
    /// <summary>
    /// After murder: Reset bypass flag
    /// </summary>
    [RegisterEvent]
    public static void OnAfterMurder(AfterMurderEvent evt)
    {
        ResetBypassState();
    }
    
    /// <summary>
    /// Resets all bypass state variables.
    /// </summary>
    public static void ResetBypassState()
    {
        if (ShouldBypassProtection)
        {
            DivaniPlugin.Instance.Log.LogInfo("Ruthless: Resetting bypass state");
        }
        ShouldBypassProtection = false;
    }
    
    /// <summary>
    /// Reset bypass state when a meeting starts.
    /// </summary>
    [RegisterEvent]
    public static void OnMeetingStart(StartMeetingEvent evt)
    {
        ResetBypassState();
    }
    
    public static void Initialize(Harmony harmony)
    {
        try
        {
            var touAssembly = Assembly.Load("TownOfUsMira");
            if (touAssembly == null)
            {
                DivaniPlugin.Instance.Log.LogWarning("Ruthless: TownOfUsMira assembly not found");
                return;
            }
            
            int patchCount = 0;
            
            // Bool: (event, source, target) — same parameter names/order for Medic, Mirrorcaster.
            var skipBoolKillSource = typeof(RuthlessRpcPatches).GetMethod(
                nameof(RuthlessRpcPatches.SkipIfRuthlessBoolKillSource),
                BindingFlags.Public | BindingFlags.Static);
            // Cleric: (event, target, source) — attacker is the third argument.
            var skipBoolCleric = typeof(RuthlessRpcPatches).GetMethod(
                nameof(RuthlessRpcPatches.SkipIfRuthlessBoolClericBarrier),
                BindingFlags.Public | BindingFlags.Static);
            var skipVoidKillSource = typeof(RuthlessRpcPatches).GetMethod(
                nameof(RuthlessRpcPatches.SkipIfRuthlessVoidKillSource),
                BindingFlags.Public | BindingFlags.Static);
            var skipVoidInvuln = typeof(RuthlessRpcPatches).GetMethod(
                nameof(RuthlessRpcPatches.SkipIfRuthlessVoidInvulnerability),
                BindingFlags.Public | BindingFlags.Static);

            var boolPatchesKillSource = new[] {
                ("TownOfUs.Events.Crewmate.MedicEvents", "CheckForMedicShield"),
                ("TownOfUs.Events.Crewmate.MirrorcasterEvents", "CheckForMagicMirror")
            };

            foreach (var (typeName, methodName) in boolPatchesKillSource)
            {
                var type = touAssembly.GetType(typeName);
                if (type == null) continue;

                var method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                if (method != null && skipBoolKillSource != null)
                {
                    harmony.Patch(method, prefix: new HarmonyMethod(skipBoolKillSource));
                    patchCount++;
                    DivaniPlugin.Instance.Log.LogInfo($"Ruthless: Patched {typeName}.{methodName} (bool, kill source)");
                }
            }

            foreach (var typeName in new[] { "TownOfUs.Events.Crewmate.ClericEvents", "TownOfUs.Events.Impostor.HerbalistEvents" })
            {
                var t = touAssembly.GetType(typeName);
                var barrierMethod = t?.GetMethod("CheckForClericBarrier",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                if (barrierMethod != null && skipBoolCleric != null)
                {
                    harmony.Patch(barrierMethod, prefix: new HarmonyMethod(skipBoolCleric));
                    patchCount++;
                    DivaniPlugin.Instance.Log.LogInfo($"Ruthless: Patched {typeName}.CheckForClericBarrier (bool, target/source order)");
                }
            }

            var voidPatchesKillSource = new[] {
                ("TownOfUs.Events.Crewmate.WardenEvents", "CheckForWardenFortify")
            };

            foreach (var (typeName, methodName) in voidPatchesKillSource)
            {
                var type = touAssembly.GetType(typeName);
                if (type == null) continue;

                var method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                if (method != null && skipVoidKillSource != null)
                {
                    harmony.Patch(method, prefix: new HarmonyMethod(skipVoidKillSource));
                    patchCount++;
                    DivaniPlugin.Instance.Log.LogInfo($"Ruthless: Patched {typeName}.{methodName} (void, kill source)");
                }
            }

            var skipFirstShield = typeof(RuthlessRpcPatches).GetMethod(
                nameof(RuthlessRpcPatches.SkipIfRuthlessFirstDeathShield),
                BindingFlags.Public | BindingFlags.Static);
            {
                var firstShieldType = touAssembly.GetType("TownOfUs.Events.Misc.FirstShieldEvents");
                var firstMethod = firstShieldType?.GetMethod("CheckForFirstDeathShield",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                if (firstMethod != null && skipFirstShield != null)
                {
                    harmony.Patch(firstMethod, prefix: new HarmonyMethod(skipFirstShield));
                    patchCount++;
                    DivaniPlugin.Instance.Log.LogInfo("Ruthless: Patched FirstShieldEvents.CheckForFirstDeathShield (void, target then source)");
                }
            }

            {
                var invulnType = touAssembly.GetType("TownOfUs.Events.InvulnerabilityEvents");
                var invulnMethod = invulnType?.GetMethod("CheckForInvulnerability",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                if (invulnMethod != null && skipVoidInvuln != null)
                {
                    harmony.Patch(invulnMethod, prefix: new HarmonyMethod(skipVoidInvuln));
                    patchCount++;
                    DivaniPlugin.Instance.Log.LogInfo("Ruthless: Patched InvulnerabilityEvents.CheckForInvulnerability (void)");
                }
            }
            
            DivaniPlugin.Instance.Log.LogInfo($"Ruthless: Patched {patchCount} shield check method(s)");
        }
        catch (Exception ex)
        {
            DivaniPlugin.Instance.Log.LogError($"Ruthless: Failed to initialize: {ex.Message}");
        }
    }
}

public static class RuthlessRpcPatches
{
    /// <summary>
    /// Shield checks run on every client with the real attacker as <paramref name="source"/>.
    /// Must not use <see cref="PlayerControl.LocalPlayer"/> — only the killer's client is local to the impostor.
    /// </summary>
    public static bool IsRuthlessAttacker(PlayerControl? source)
    {
        if (source == null)
        {
            return false;
        }

        if (source.HasModifier<RuthlessModifier>())
        {
            return true;
        }

        foreach (var mod in source.GetModifiers<BaseModifier>())
        {
            if (mod.GetType().Name == "RuthlessModifier")
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Prefix for <c>CheckForMedicShield</c> / <c>CheckForMagicMirror</c>: (event, source, target).
    /// </summary>
    public static bool SkipIfRuthlessBoolKillSource(ref bool __result, PlayerControl source)
    {
        if (IsRuthlessAttacker(source))
        {
            DivaniPlugin.Instance.Log.LogInfo("Ruthless: Skipping shield check (bool, kill source)");
            __result = false;
            return false;
        }

        return true;
    }

    /// <summary>
    /// Prefix for <c>CheckForClericBarrier</c>: (event, target, source).
    /// </summary>
    public static bool SkipIfRuthlessBoolClericBarrier(ref bool __result, PlayerControl target, PlayerControl source)
    {
        if (IsRuthlessAttacker(source))
        {
            DivaniPlugin.Instance.Log.LogInfo("Ruthless: Skipping cleric barrier check (bool)");
            __result = false;
            return false;
        }

        return true;
    }

    /// <summary>
    /// Prefix for <c>CheckForWardenFortify</c>: (event, source, target).
    /// </summary>
    public static bool SkipIfRuthlessVoidKillSource(PlayerControl source)
    {
        if (IsRuthlessAttacker(source))
        {
            DivaniPlugin.Instance.Log.LogInfo("Ruthless: Skipping warden fortify check (void)");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Prefix for <c>FirstShieldEvents.CheckForFirstDeathShield</c>: (event, target, source).
    /// </summary>
    public static bool SkipIfRuthlessFirstDeathShield(PlayerControl target, PlayerControl source)
    {
        if (IsRuthlessAttacker(source))
        {
            DivaniPlugin.Instance.Log.LogInfo("Ruthless: Skipping first-death shield check (void)");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Prefix for <c>CheckForInvulnerability</c>: (miraEvent, source, target, killAttempt).
    /// </summary>
    public static bool SkipIfRuthlessVoidInvulnerability(PlayerControl source)
    {
        if (IsRuthlessAttacker(source))
        {
            DivaniPlugin.Instance.Log.LogInfo("Ruthless: Skipping invulnerability check (void)");
            return false;
        }

        return true;
    }
}
