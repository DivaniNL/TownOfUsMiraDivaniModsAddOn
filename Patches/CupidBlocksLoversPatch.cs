using System.Linq;
using HarmonyLib;
using DivaniMods.Roles.Neutral.NeutralBenign;
using TownOfUs.Modifiers.Game.Alliance;

namespace DivaniMods.Patches;

internal static class CupidLoverExclusion
{
    internal static bool CupidInGame()
    {
        if (PlayerControl.AllPlayerControls == null)
        {
            return false;
        }

        return PlayerControl.AllPlayerControls.ToArray()
            .Any(p => p != null && p.Data != null && p.Data.Role is CupidRole);
    }
}

[HarmonyPatch(typeof(LoverModifier), nameof(LoverModifier.AssignTargets))]
public static class CupidBlockLoverAssignTargetsPatch
{
    [HarmonyPrefix]
    public static bool Prefix()
    {
        return !CupidLoverExclusion.CupidInGame();
    }
}

[HarmonyPatch(typeof(LoverModifier), nameof(LoverModifier.CustomAmount), MethodType.Getter)]
public static class CupidBlockLoverAmountPatch
{
    [HarmonyPostfix]
    public static void Postfix(ref int __result)
    {
        if (CupidLoverExclusion.CupidInGame())
        {
            __result = 0;
        }
    }
}
