using HarmonyLib;
using MiraAPI.GameOptions;
using DivaniMods.Options;
using DivaniMods.Roles.Crewmate.CrewmateAfterlife;

namespace DivaniMods.Patches;

[HarmonyPatch(typeof(LogicOptions), nameof(LogicOptions.GetPlayerSpeedMod))]
internal static class VengefulSoulSpeedPatch
{
    [HarmonyPostfix]
    public static void Postfix(PlayerControl pc, ref float __result)
    {
        if (pc?.Data?.Role is VengefulSoulRole { GhostActive: true })
        {
            __result *= OptionGroupSingleton<RetributionistOptions>.Instance.VengefulSoulSpeed.Value;
        }
    }
}
