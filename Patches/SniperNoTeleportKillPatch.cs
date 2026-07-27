using System;
using System.Reflection;
using HarmonyLib;
using MiraAPI.Networking;
using DivaniMods.Modifiers.Game.Neutral.NeutralPassive;
using TownOfUs.Buttons.Classic.Neutral.NeutralKilling;
using TownOfUs.Buttons.Neutral;

namespace DivaniMods.Patches;

public static class SniperNoTeleportKill
{
    private static readonly MethodInfo? VampireConvertCheck =
        AccessTools.Method(typeof(VampireBiteButton), "ConvertCheck");

    private static bool CanConvertVampireTarget(PlayerControl target)
    {
        return (bool?)VampireConvertCheck?.Invoke(null, [target]) == true;
    }

    public static bool TryMurderWithoutTeleport(PlayerControl? target, bool createDeadBody = true)
    {
        if (!SniperModifier.LocalPlayerHasSniper() || target == null)
        {
            return true;
        }

        PlayerControl.LocalPlayer.RpcCustomMurder(
            target,
            MeetingCheck.OutsideMeeting,
            teleportMurderer: false,
            createDeadBody: createDeadBody);
        return false;
    }

    public static bool TryVampireBiteWithoutTeleport(PlayerControl? target)
    {
        if (!SniperModifier.LocalPlayerHasSniper() || target == null || CanConvertVampireTarget(target))
        {
            return true;
        }

        PlayerControl.LocalPlayer.RpcCustomMurder(
            target,
            MeetingCheck.OutsideMeeting,
            teleportMurderer: false);
        return false;
    }
}

[HarmonyPatch(typeof(GlitchKillButton), "OnClick")]
public static class SniperGlitchKillNoTeleportPatch
{
    public static bool Prefix(GlitchKillButton __instance) =>
        SniperNoTeleportKill.TryMurderWithoutTeleport(__instance.Target);
}

[HarmonyPatch(typeof(JuggernautKillButton), "OnClick")]
public static class SniperJuggernautKillNoTeleportPatch
{
    public static bool Prefix(JuggernautKillButton __instance) =>
        SniperNoTeleportKill.TryMurderWithoutTeleport(__instance.Target);
}

[HarmonyPatch(typeof(PestilenceKillButton), "OnClick")]
public static class SniperPestilenceKillNoTeleportPatch
{
    public static bool Prefix(PestilenceKillButton __instance) =>
        SniperNoTeleportKill.TryMurderWithoutTeleport(__instance.Target);
}

[HarmonyPatch(typeof(MedusaPetrifyButton), "OnClick")]
public static class SniperMedusaPetrifyNoTeleportPatch
{
    public static bool Prefix(MedusaPetrifyButton __instance) =>
        SniperNoTeleportKill.TryMurderWithoutTeleport(__instance.Target, createDeadBody: false);
}

[HarmonyPatch(typeof(VampireBiteButton), "OnClick")]
public static class SniperVampireBiteNoTeleportPatch
{
    public static bool Prefix(VampireBiteButton __instance) =>
        SniperNoTeleportKill.TryVampireBiteWithoutTeleport(__instance.Target);
}

[HarmonyPatch(typeof(WerewolfKillButton), "OnClick")]
public static class SniperWerewolfKillNoTeleportPatch
{
    public static bool Prefix(WerewolfKillButton __instance) =>
        SniperNoTeleportKill.TryMurderWithoutTeleport(__instance.Target);
}
public static class SniperSerialKillerKill
{
    private static PropertyInfo? _targetProperty;
    private static MethodInfo? _isTargetValidMethod;

    public static void Initialize(Harmony harmony)
    {
        try
        {
            var assembly = Assembly.Load("TouMiraRolesExtension");
            if (assembly == null)
            {
                return;
            }

            var buttonType = assembly.GetType("TouMiraRolesExtension.Buttons.Neutral.SerialKillerKillButton");
            if (buttonType == null)
            {
                return;
            }

            var onClick = AccessTools.Method(buttonType, "OnClick");
            if (onClick == null)
            {
                return;
            }

            _targetProperty = AccessTools.Property(buttonType, "Target");
            _isTargetValidMethod = AccessTools.Method(buttonType, "IsTargetValid", [typeof(PlayerControl)]);

            var prefix = typeof(SniperSerialKillerKill).GetMethod(
                nameof(Prefix), BindingFlags.Public | BindingFlags.Static);
            harmony.Patch(onClick, prefix: new HarmonyMethod(prefix));
        }
        catch (Exception ex)
        {
            DivaniPlugin.Instance.Log.LogWarning($"Sniper: SerialKiller patch skipped: {ex.Message}");
        }
    }

    public static bool Prefix(object __instance)
    {
        if (!SniperModifier.LocalPlayerHasSniper())
        {
            return true;
        }

        var player = PlayerControl.LocalPlayer;
        if (player == null)
        {
            return true;
        }

        if (player.inVent && Vent.currentVent != null)
        {
            return true;
        }

        if (_targetProperty?.GetValue(__instance) is not PlayerControl target)
        {
            return true;
        }

        if (_isTargetValidMethod != null &&
            _isTargetValidMethod.Invoke(__instance, [target]) is bool valid && !valid)
        {
            return true;
        }

        return SniperNoTeleportKill.TryMurderWithoutTeleport(target);
    }
}
