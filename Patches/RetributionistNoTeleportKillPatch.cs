using HarmonyLib;
using MiraAPI.Networking;
using DivaniMods.Roles.Crewmate.CrewmateKilling;
using TownOfUs.Modules;

namespace DivaniMods.Patches;

// Any kill landed on a Retributionist never teleports the killer onto the victim. The killer
// stays where it struck from, avoiding the jarring lunge-onto-the-body that the revenge spawn
// would otherwise produce. Runs on every client (the confirm RPC handler) so it stays in sync.
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
