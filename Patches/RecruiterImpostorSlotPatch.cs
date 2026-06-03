using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using HarmonyLib;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using DivaniMods.Roles.Impostor.ImpostorSupport;
using TownOfUs.Roles;
using TownOfUs.Utilities;

namespace DivaniMods.Patches;

[HarmonyPatch]
public static class RecruiterImpostorSlotPatch
{
    private static readonly RoleAlignment[] CrewAlignments =
    {
        RoleAlignment.CrewmateInvestigative,
        RoleAlignment.CrewmateKilling,
        RoleAlignment.CrewmateProtective,
        RoleAlignment.CrewmatePower,
        RoleAlignment.CrewmateSupport,
    };

    [HarmonyPatch(typeof(RoleManager), nameof(RoleManager.SelectRoles))]
    [HarmonyPriority(Priority.High)]
    [HarmonyPostfix]
    public static void ReserveSlotForRecruit()
    {
        if (!AmongUsClient.Instance || !AmongUsClient.Instance.AmHost)
        {
            return;
        }

        var all = PlayerControl.AllPlayerControls.ToArray();

        var recruiter = all.FirstOrDefault(p =>
            p != null && p.Data != null && !p.Data.Disconnected && !p.Data.IsDead && p.Data.Role is RecruiterRole);
        if (recruiter == null)
        {
            return;
        }

        var impostors = all.Where(p =>
            p != null && p.Data != null && !p.Data.Disconnected && p.Data.Role is ImpostorRole).ToList();
        if (impostors.Count < 2)
        {
            return;
        }

        var candidates = impostors.Where(p => p.PlayerId != recruiter.PlayerId).ToList();
        if (candidates.Count == 0)
        {
            return;
        }

        candidates.Shuffle();
        candidates[0].RpcSetRole(PickUnassignedCrewRole(all), true);
    }

    private static RoleTypes PickUnassignedCrewRole(PlayerControl[] all)
    {
        var assigned = new HashSet<ushort>();
        foreach (var p in all)
        {
            if (p?.Data?.Role != null)
            {
                assigned.Add((ushort)p.Data.Role.Role);
            }
        }

        var pool = new List<ushort>();
        foreach (var alignment in CrewAlignments)
        {
            foreach (var (roleType, chance) in MiscUtils.GetRolesToAssign(alignment))
            {
                if (chance > 0 && !assigned.Contains(roleType) && !pool.Contains(roleType))
                {
                    pool.Add(roleType);
                }
            }
        }

        if (pool.Count == 0)
        {
            return RoleTypes.Crewmate;
        }

        pool.Shuffle();
        return (RoleTypes)pool[0];
    }
}
