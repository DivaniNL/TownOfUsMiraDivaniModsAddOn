using System.Collections;
using System.Linq;
using AmongUs.GameOptions;
using HarmonyLib;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using Reactor.Utilities;
using DivaniMods.Roles;
using TownOfUs.Modifiers.Game.Alliance;
using TownOfUs.Roles;
using TownOfUs.Utilities;
using UnityEngine;

namespace DivaniMods.Patches;

/// <summary>
/// Stalker only exists in the game if Lovers ended up being assigned this round.
/// Lovers are assigned asynchronously inside TOU's CoAssignTargets coroutine
/// (via LoverModifier.AssignTargets + ModifierManager.AssignModifiers), so we
/// defer the check a short moment after RoleManager.SelectRoles returns and
/// revert any rolled Stalkers to Crewmates when no Lovers are present.
/// </summary>
[HarmonyPatch]
public static class StalkerGatePatch
{
    [HarmonyPatch(typeof(RoleManager), nameof(RoleManager.SelectRoles))]
    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last)]
    public static void AfterSelectRoles()
    {
        if (!AmongUsClient.Instance || !AmongUsClient.Instance.AmHost)
        {
            return;
        }

        StalkerRole.ClearAndReload();
        Coroutines.Start(WaitAndGateStalkers());
    }

    private static IEnumerator WaitAndGateStalkers()
    {
        // CoAssignTargets does multiple WaitForSeconds(0.01f) yields for every role/modifier
        // that implements IAssignableTargets, then calls ModifierManager.AssignModifiers.
        // 1 second is plenty of headroom.
        yield return new WaitForSeconds(1.0f);

        var anyLovers = PlayerControl.AllPlayerControls.ToArray()
            .Any(p => p != null && p.HasModifier<LoverModifier>());

        if (anyLovers)
        {
            DivaniPlugin.Instance.Log.LogInfo("StalkerGate: Lovers present, Stalker allowed to stay.");
            yield break;
        }

        var stalkers = PlayerControl.AllPlayerControls.ToArray()
            .Where(p => p != null && p.Data != null && p.Data.Role is StalkerRole)
            .ToList();

        if (stalkers.Count == 0)
        {
            yield break;
        }

        // Primary pool: any NeutralEvil role that isn't the Stalker itself.
        var neutralEvilPool = MiscUtils.AllRegisteredRoles
            .Where(r => r != null
                        && r is not StalkerRole
                        && r.GetRoleAlignment() == RoleAlignment.NeutralEvil)
            .ToList();

        // Fallback pool: custom Crewmate roles (Investigative/Killing/Power/...),
        // explicitly excluding the plain vanilla Crewmate. We also filter out
        // any role that's already been assigned to another player so the
        // Stalker is replaced with something fresh.
        var pickedRoleTypes = new HashSet<RoleTypes>(PlayerControl.AllPlayerControls.ToArray()
            .Where(p => p != null && p.Data != null && p.Data.Role != null)
            .Select(p => p.Data.Role.Role));

        var crewmateFallbackPool = MiscUtils.AllRegisteredRoles
            .Where(r => r != null
                        && r.Role != RoleTypes.Crewmate
                        && r.IsCrewmate()
                        && !pickedRoleTypes.Contains(r.Role))
            .ToList();

        DivaniPlugin.Instance.Log.LogInfo(
            "StalkerGate: No Lovers in game; rerolling " + stalkers.Count +
            " Stalker(s). NeutralEvil pool: " + neutralEvilPool.Count +
            ", Crewmate fallback pool: " + crewmateFallbackPool.Count + ".");

        foreach (var stalker in stalkers)
        {
            if (neutralEvilPool.Count > 0)
            {
                var pick = neutralEvilPool[UnityEngine.Random.Range(0, neutralEvilPool.Count)];
                DivaniPlugin.Instance.Log.LogInfo(
                    "StalkerGate: " + stalker.Data.PlayerName +
                    " reassigned to " + pick.GetType().Name + " (NeutralEvil).");
                stalker.RpcSetRole(pick.Role, true);
                continue;
            }

            if (crewmateFallbackPool.Count > 0)
            {
                // Pull the picked role out of the fallback pool so subsequent
                // Stalkers (rare, but possible) each get a unique crew role.
                var idx = UnityEngine.Random.Range(0, crewmateFallbackPool.Count);
                var pick = crewmateFallbackPool[idx];
                crewmateFallbackPool.RemoveAt(idx);
                DivaniPlugin.Instance.Log.LogInfo(
                    "StalkerGate: " + stalker.Data.PlayerName +
                    " reassigned to " + pick.GetType().Name + " (unpicked Crewmate fallback).");
                stalker.RpcSetRole(pick.Role, true);
                continue;
            }

            DivaniPlugin.Instance.Log.LogWarning(
                "StalkerGate: Both NeutralEvil and unpicked Crewmate pools are empty; " +
                "falling back to plain Crewmate for " + stalker.Data.PlayerName + ".");
            stalker.RpcSetRole(RoleTypes.Crewmate, true);
        }
    }
}
