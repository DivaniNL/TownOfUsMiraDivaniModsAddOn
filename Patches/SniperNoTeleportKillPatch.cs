using System.Reflection;
using HarmonyLib;
using MiraAPI.Networking;
using DivaniMods.Modifiers;
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

[HarmonyPatch(typeof(SoulCollectorReapButton), "OnClick")]
public static class SniperSoulCollectorReapNoTeleportPatch
{
    public static bool Prefix(SoulCollectorReapButton __instance) =>
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
