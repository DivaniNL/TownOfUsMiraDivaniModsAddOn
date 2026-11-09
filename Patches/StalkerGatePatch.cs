using System.Collections;
using System.Linq;
using AmongUs.GameOptions;
using HarmonyLib;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using Reactor.Utilities;
using DivaniMods.Roles;
using DivaniMods.Roles.Neutral.NeutralBenign;
using TownOfUs.Modifiers.Game.Alliance;
using TownOfUs.Roles;
using TownOfUs.Utilities;
using UnityEngine;

namespace DivaniMods.Patches;

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
        yield return new WaitForSeconds(1.0f);

        var players = PlayerControl.AllPlayerControls.ToArray();
        var stalkers = players
            .Where(p => p != null && p.Data != null && p.Data.Role is StalkerRole)
            .ToList();

        if (stalkers.Count == 0)
        {
            yield break;
        }

        var cupidInGame = players.Any(p => p != null && p.Data?.Role is CupidRole);
        var loverCount = players.Count(p =>
            p != null && p.Data?.Role is not StalkerRole && p.HasModifier<LoverModifier>());

        if (!cupidInGame && loverCount >= 2)
        {
            DivaniPlugin.Instance.Log.LogInfo("StalkerGate: Two Lovers present and no Cupid; Stalker allowed to stay.");
            yield break;
        }

        // Primary pool: any NeutralEvil role that isn't the Stalker itself.
        var neutralEvilPool = MiscUtils.AllRegisteredRoles
            .Where(r => r != null
                        && r is not StalkerRole
                        && r.GetRoleAlignment() == RoleAlignment.NeutralEvil)
            .ToList();
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
            "StalkerGate: " + (cupidInGame
                ? "Cupid is in game"
                : "fewer than two Lovers are assigned") +
            "; rerolling " + stalkers.Count +
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
