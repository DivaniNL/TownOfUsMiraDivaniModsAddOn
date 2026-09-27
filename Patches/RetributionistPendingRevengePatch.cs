using HarmonyLib;
using MiraAPI.Networking;
using DivaniMods.Events.Crewmate.CrewmateKilling;

namespace DivaniMods.Patches;

// The revenge only starts in AfterMurder, which fires after the kill animation. Without this,
// the end-game check runs first on a dead Retributionist and hands the win to the killer's team
// (or to a Betrayer) before the Vengeful Soul ever spawns.
[HarmonyPatch(typeof(CustomMurderRpc), nameof(CustomMurderRpc.RpcConfirmCustomMurder),
    typeof(PlayerControl), typeof(PlayerControl), typeof(PlayerControl), typeof(bool), typeof(bool),
    typeof(MurderResultFlags), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(bool))]
internal static class RetributionistPendingRevengePatch
{
    [HarmonyPriority(Priority.First)]
    [HarmonyPrefix]
    public static void Prefix(PlayerControl source, PlayerControl target)
    {
        if (target == null || !RetributionistEvents.IsRevengeEligible(target, source))
        {
            return;
        }

        RetributionistManager.MarkRevengePending(target.PlayerId);
    }
}