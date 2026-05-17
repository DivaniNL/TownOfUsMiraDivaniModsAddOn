using HarmonyLib;
using MiraAPI.Modifiers;
using DivaniMods.Modifiers.Game.Crewmate;

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
    }
}

[HarmonyPatch(typeof(PlanetSurveillanceMinigame))]
public static class PlanetSurveillanceMinigamePatch
{
    [HarmonyPatch(nameof(PlanetSurveillanceMinigame.Begin))]
    [HarmonyPostfix]
    public static void BeginPostfix(PlanetSurveillanceMinigame __instance)
    {
    }
}

[HarmonyPatch(typeof(FungleSurveillanceMinigame))]
public static class FungleSurveillanceMinigamePatch
{
    [HarmonyPatch(nameof(FungleSurveillanceMinigame.Begin))]
    [HarmonyPostfix]
    public static void BeginPostfix(FungleSurveillanceMinigame __instance)
    {
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
        }
        
        if (systemType != SystemTypes.Security) return true;
        if (!BlindspotHelper.HasBlindspot) return true;
        
        return false;
    }
}