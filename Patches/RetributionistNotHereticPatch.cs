using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MiraAPI.Modifiers;
using DivaniMods.Roles.Crewmate.CrewmateKilling;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Roles.Neutral;
using TownOfUs.Roles.Other;
using TownOfUs.Utilities;
using TownOfUs.Modules;

namespace DivaniMods.Patches;

[HarmonyPatch(typeof(InquisitorRole), nameof(InquisitorRole.AssignTargets))]
internal static class RetributionistNotHereticPatch
{
    [HarmonyPostfix]
    public static void Postfix(InquisitorRole __instance)
    {
        if (!AmongUsClient.Instance || !AmongUsClient.Instance.AmHost)
        {
            return;
        }

        var retribTargets = __instance.Targets
            .Where(t => t != null && t.GetRoleWhenAlive() is RetributionistRole)
            .ToList();

        if (retribTargets.Count == 0)
        {
            return;
        }

        var taken = new HashSet<byte>(__instance.Targets.Where(t => t != null).Select(t => t.PlayerId));

        foreach (var retrib in retribTargets)
        {
            retrib.RpcRemoveModifier<InquisitorHereticModifier>();

            var idx = __instance.Targets.IndexOf(retrib);
            if (idx >= 0)
            {
                __instance.Targets.RemoveAt(idx);
                if (idx < __instance.TargetRoles.Count)
                {
                    __instance.TargetRoles.RemoveAt(idx);
                }
            }

            taken.Remove(retrib.PlayerId);

            var candidates = PlayerControl.AllPlayerControls.ToArray()
                .Where(x => x != null && !x.HasDied() &&
                            x.Data?.Role is not InquisitorRole &&
                            x.Data?.Role is not SpectatorRole &&
                            x.GetRoleWhenAlive() is not RetributionistRole &&
                            !x.HasModifier<InquisitorHereticModifier>() &&
                            x.PlayerId != __instance.Player.PlayerId &&
                            !taken.Contains(x.PlayerId) &&
                            x.Data != null &&
                            !SpectatorRole.TrackedSpectators.Contains(x.Data.PlayerName))
                .ToList();

            if (candidates.Count == 0)
            {
                continue;
            }

            var replacement = candidates[UnityEngine.Random.RandomRangeInt(0, candidates.Count)];
            taken.Add(replacement.PlayerId);
            InquisitorRole.RpcAddInquisTarget(__instance.Player, replacement);
        }
    }
}
