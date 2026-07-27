using HarmonyLib;
using MiraAPI.Networking;
using DivaniMods.Roles.Crewmate.CrewmateKilling;
using TownOfUs.Modules;

namespace DivaniMods.Patches;

[HarmonyPatch(typeof(CustomMurderRpc), nameof(CustomMurderRpc.RpcConfirmCustomMurder),
    typeof(PlayerControl), typeof(PlayerControl), typeof(PlayerControl), typeof(MurderResultFlags),
    typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(bool))]
internal static class RetributionistNoTeleportKillPatch
{
    [HarmonyPriority(Priority.First)]
    [HarmonyPrefix]
    public static void Prefix(PlayerControl target, ref bool teleportMurderer)
    {
        if (target != null && target.GetRoleWhenAlive() is RetributionistRole)
        {
            teleportMurderer = false;
        }
    }
}
