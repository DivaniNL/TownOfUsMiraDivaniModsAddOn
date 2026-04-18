using HarmonyLib;
using MiraAPI.Modifiers;
using DivaniMods.Modifiers;

namespace DivaniMods.Patches;

public static class BlindspotHelper
{
    public static bool HasBlindspot => 
        PlayerControl.LocalPlayer != null && 
        PlayerControl.LocalPlayer.HasModifier<BlindspotModifier>();
}

[HarmonyPatch(typeof(SurveillanceMinigame))]
public static class SurveillanceMinigamePatch
{
    [HarmonyPatch(nameof(SurveillanceMinigame.Begin))]
    [HarmonyPostfix]
    public static void BeginPostfix(SurveillanceMinigame __instance)
    {
        DivaniPlugin.Instance.Log.LogInfo($"Blindspot: SurveillanceMinigame.Begin called, HasBlindspot={BlindspotHelper.HasBlindspot}");
    }
}

[HarmonyPatch(typeof(PlanetSurveillanceMinigame))]
public static class PlanetSurveillanceMinigamePatch
{
    [HarmonyPatch(nameof(PlanetSurveillanceMinigame.Begin))]
    [HarmonyPostfix]
    public static void BeginPostfix(PlanetSurveillanceMinigame __instance)
    {
        DivaniPlugin.Instance.Log.LogInfo($"Blindspot: PlanetSurveillanceMinigame.Begin called, HasBlindspot={BlindspotHelper.HasBlindspot}");
    }
}

[HarmonyPatch(typeof(FungleSurveillanceMinigame))]
public static class FungleSurveillanceMinigamePatch
{
    [HarmonyPatch(nameof(FungleSurveillanceMinigame.Begin))]
    [HarmonyPostfix]
    public static void BeginPostfix(FungleSurveillanceMinigame __instance)
    {
        DivaniPlugin.Instance.Log.LogInfo($"Blindspot: FungleSurveillanceMinigame.Begin called, HasBlindspot={BlindspotHelper.HasBlindspot}");
    }
}

[HarmonyPatch(typeof(ShipStatus))]
public static class ShipStatusPatch
{
    [HarmonyPatch(nameof(ShipStatus.RpcUpdateSystem), typeof(SystemTypes), typeof(byte))]
    [HarmonyPrefix]
    public static bool RpcUpdateSystemPrefix(ShipStatus __instance, SystemTypes systemType, byte amount)
    {
        if (systemType == SystemTypes.Security)
        {
            DivaniPlugin.Instance.Log.LogInfo($"Blindspot: ShipStatus.RpcUpdateSystem Security called, amount={amount}, HasBlindspot={BlindspotHelper.HasBlindspot}");
        }
        
        if (systemType != SystemTypes.Security) return true;
        if (!BlindspotHelper.HasBlindspot) return true;
        
        DivaniPlugin.Instance.Log.LogInfo($"Blindspot: BLOCKING security RPC, amount={amount}");
        return false;
    }
}