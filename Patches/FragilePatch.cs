using System;
using System.Reflection;
using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using DivaniMods.Assets;
using DivaniMods.Modifiers;
using DivaniMods.Options;
using UnityEngine;

namespace DivaniMods.Patches;

/// <summary>
/// When any player-targeting ability button is used on another player, if that player has Fragile they die.
/// Same networking rule as Veteran: only <c>source.AmOwner</c> runs the RPC.
/// Uses <c>target.RpcCustomMurder(target, …)</c> so the death plays as the fragile player killing themselves (suicide-style anim),
/// not as the interactor killing them.
/// </summary>
public static class FragileInteraction
{
    private static int _lastFragileKillFrame = -1;
    private static byte _lastFragileKillVictimId = byte.MaxValue;

    /// <summary>
    /// If the button targets a fragile player, applies fragile death and returns <c>true</c> so the caller can skip
    /// <see cref="CustomActionButton.ClickHandler"/> / TownOfUs <c>OnClick</c> (no shield, douse, etc.).
    /// </summary>
    public static bool TryConsumeFragilePlayerTargetedClick(PlayerControl? source, object? buttonInstance)
    {
        var target = TryGetPlayerTarget(buttonInstance);
        return TryApplyFragileDeath(source, target);
    }

    /// <summary>
    /// Returns <c>true</c> if a fragile death RPC was sent (caller should cancel the rest of the click).
    /// </summary>
    public static bool TryApplyFragileDeath(PlayerControl? source, PlayerControl? target)
    {
        if (MeetingHud.Instance || ExileController.Instance)
            return false;

        if (source == null || target == null || source == target)
            return false;

        if (target.Data == null || target.Data.IsDead)
            return false;

        if (!target.HasModifier<FragileModifier>())
            return false;

        // Same as Veteran: only the client that performed the interaction runs the RPC.
        if (!TutorialManager.InstanceExists && !source.AmOwner)
            return false;

        if (Time.frameCount == _lastFragileKillFrame && target.PlayerId == _lastFragileKillVictimId)
            return false;
        var chanceToBreak = OptionGroupSingleton<FragileOptions>.Instance.ChanceToBreak.Value;
        var randNum = UnityEngine.Random.Range(0f, 100f);
        DivaniPlugin.Instance.Log.LogInfo($"Fragile: Chance to break is {chanceToBreak}%, random roll is {randNum:F1}");
        if (chanceToBreak < 100f && randNum >= chanceToBreak)
        {
            DivaniPlugin.Instance.Log.LogInfo("Fragile: Player survived (roll >= chance)");
            return false;
        }

        _lastFragileKillFrame = Time.frameCount;
        _lastFragileKillVictimId = target.PlayerId;

        target.RpcCustomMurder(target, MeetingCheck.OutsideMeeting);
        PlayFragileBreakSound();
        return true;
    }

    /// <summary>
    /// Glass-break SFX for the interactor that just broke a fragile player.
    /// Only runs on source.AmOwner path, so only the killer hears it (by design).
    /// </summary>
    private static void PlayFragileBreakSound()
    {
        if (SoundManager.Instance == null) return;
        try
        {
            var clip = DivaniAssets.FragileBreak.LoadAsset();
            if (clip == null) return;
            SoundManager.Instance.PlaySound(clip, false, 1f);
        }
        catch (System.Exception ex)
        {
            DivaniPlugin.Instance.Log.LogWarning($"Fragile: break sfx failed: {ex.Message}");
        }
    }

    private static PlayerControl? TryGetPlayerTarget(object? buttonInstance)
    {
        if (buttonInstance == null)
            return null;

        var t = buttonInstance.GetType();

        // CustomActionButton<PlayerControl>.Target
        foreach (var prop in t.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
        {
            if (prop.Name != "Target")
                continue;
            if (!typeof(PlayerControl).IsAssignableFrom(prop.PropertyType))
                continue;
            try
            {
                return prop.GetValue(buttonInstance) as PlayerControl;
            }
            catch
            {
                return null;
            }
        }

        return null;
    }
}

/// <summary>
/// Prefix on Mira <see cref="CustomActionButton.ClickHandler"/> so fragile is handled before <see cref="CustomActionButton.OnClick"/>.
/// TownOfUs buttons use their own <c>ClickHandler</c> overrides and are patched separately.
/// </summary>
[HarmonyPatch(typeof(CustomActionButton), nameof(CustomActionButton.ClickHandler))]
internal static class FragileCustomActionButtonClickPatch
{
    private static bool Prefix(CustomActionButton __instance)
    {
        if (!__instance.CanClick())
            return true;

        if (FragileInteraction.TryConsumeFragilePlayerTargetedClick(PlayerControl.LocalPlayer, __instance))
            return false;

        return true;
    }
}

/// <summary>
/// Patches TownOfUs <c>ClickHandler</c> overrides — TOU replaces <see cref="CustomActionButton.ClickHandler"/> entirely
/// (<see href="https://github.com/AU-Avengers/TOU-Mira/blob/main/TownOfUs/Buttons/TownOfUsButton.cs">TownOfUsButton.cs</see>),
/// so a postfix runs <i>after</i> <c>OnClick</c> (shield/douse already applied). A prefix skips the original when fragile triggers.
/// </summary>
public static class FragileTownOfUsButtonPatch
{
    private static bool _initialized;

    public static void Initialize(Harmony harmony)
    {
        if (_initialized)
            return;
        _initialized = true;

        try
        {
            var assembly = Assembly.Load("TownOfUsMira");
            if (assembly == null)
            {
                DivaniPlugin.Instance.Log.LogWarning("Fragile: TownOfUsMira assembly not found; only Mira buttons will trigger Fragile.");
                return;
            }

            Type? touButton = null;
            Type? touTargetButtonGeneric = null;

            foreach (var type in assembly.GetTypes())
            {
                if (type.Name == "TownOfUsButton" && !type.IsGenericTypeDefinition)
                    touButton = type;
                if (type.Name.StartsWith("TownOfUsTargetButton", StringComparison.Ordinal) && type.IsGenericTypeDefinition)
                    touTargetButtonGeneric = type;
            }

            var prefix = typeof(FragileTownOfUsButtonPatch).GetMethod(
                nameof(Prefix),
                BindingFlags.Static | BindingFlags.NonPublic);

            if (prefix == null)
                return;

            var harmonyMethod = new HarmonyMethod(prefix);
            var count = 0;

            if (touButton != null)
            {
                var m = touButton.GetMethod("ClickHandler", BindingFlags.Public | BindingFlags.Instance);
                if (m != null)
                {
                    harmony.Patch(m, prefix: harmonyMethod);
                    count++;
                }
            }

            if (touTargetButtonGeneric != null)
            {
                var closed = touTargetButtonGeneric.MakeGenericType(typeof(PlayerControl));
                var m = closed.GetMethod("ClickHandler", BindingFlags.Public | BindingFlags.Instance);
                if (m != null)
                {
                    harmony.Patch(m, prefix: harmonyMethod);
                    count++;
                }
            }

            foreach (var type in assembly.GetTypes())
            {
                if (type.IsAbstract || type.IsInterface)
                    continue;
                if (!type.Name.EndsWith("Button", StringComparison.Ordinal))
                    continue;

                var m = type.GetMethod("ClickHandler", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                if (m == null)
                    continue;

                try
                {
                    harmony.Patch(m, prefix: harmonyMethod);
                    count++;
                }
                catch
                {
                    // already patched or incompatible
                }
            }

            DivaniPlugin.Instance.Log.LogInfo($"Fragile: Patched {count} TownOfUs ClickHandler prefix(es) for Fragile.");
        }
        catch (Exception ex)
        {
            DivaniPlugin.Instance.Log.LogError($"Fragile: TownOfUs button patch init failed: {ex.Message}");
        }
    }

    private static bool Prefix(object __instance)
    {
        if (__instance is not CustomActionButton btn || !btn.CanClick())
            return true;

        if (FragileInteraction.TryConsumeFragilePlayerTargetedClick(PlayerControl.LocalPlayer, __instance))
            return false;

        return true;
    }
}
