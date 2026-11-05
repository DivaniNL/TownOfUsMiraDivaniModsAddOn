using HarmonyLib;
using DivaniMods.Roles.Crewmate.CrewmateAfterlife;
using DivaniMods.Roles.Crewmate.CrewmateKilling;
using DivaniMods.Roles.Neutral.NeutralEvil;
using TownOfUs.Modules;
using TownOfUs.Utilities;

namespace DivaniMods.Patches;

[HarmonyPatch(typeof(TouRoleUtils), nameof(TouRoleUtils.CanGetGhostRole))]
internal static class GhostRoleExclusionPatch
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

        if (player.GetRoleWhenAlive() is PlagueDoctorRole && PlagueDoctorRole.CanWinWhileDead)
        {
            __result = false;
        }
    }
}
