using HarmonyLib;
using DivaniMods.Roles.Crewmate.CrewmateAfterlife;
using DivaniMods.Roles.Crewmate.CrewmateKilling;
using TownOfUs.Modules;
using TownOfUs.Utilities;

namespace DivaniMods.Patches;

[HarmonyPatch(typeof(TouRoleUtils), nameof(TouRoleUtils.CanGetGhostRole))]
internal static class RetributionistNoGhostRolePatch
{
    [HarmonyPostfix]
    public static void Postfix(PlayerControl player, ref bool __result)
    {
        if (!__result || player == null)
        {
            return;
        }

        if (player.GetRoleWhenAlive() is RetributionistRole || player.Data?.Role is VengefulSoulRole)
        {
            __result = false;
        }
    }
}
