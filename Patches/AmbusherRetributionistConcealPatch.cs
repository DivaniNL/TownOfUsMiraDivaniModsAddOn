using System.Collections;
using HarmonyLib;
using MiraAPI.Modifiers;
using Reactor.Utilities;
using DivaniMods.Events.Crewmate.CrewmateKilling;
using TownOfUs.Modifiers.Impostor;

namespace DivaniMods.Patches;

[HarmonyPatch(typeof(AmbusherConcealedModifier), nameof(AmbusherConcealedModifier.OnActivate))]
internal static class AmbusherRetributionistConcealPatch
{
    [HarmonyPrefix]
    public static bool Prefix(AmbusherConcealedModifier __instance)
    {
        var target = __instance.Target;

        if (target == null)
        {
            return true;
        }

        if (!RetributionistEvents.IsRevengeEligible(target, __instance.Player) &&
            !RetributionistManager.IsRevengeActive(target.PlayerId))
        {
            return true;
        }

        Coroutines.Start(CoRemoveConceal(__instance));
        return false;
    }

    private static IEnumerator CoRemoveConceal(AmbusherConcealedModifier modifier)
    {
        yield return null;

        var player = modifier.Player;

        if (player == null)
        {
            yield break;
        }

        player.RemoveModifier(modifier);
    }
}
