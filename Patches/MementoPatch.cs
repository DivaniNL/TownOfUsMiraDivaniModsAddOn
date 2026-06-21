using System.Collections.Generic;
using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using DivaniMods.Modifiers.Game.Universal;
using DivaniMods.Modifiers.Universal;
using DivaniMods.Options;
using TownOfUs.Events.TouEvents;
using TownOfUs.Modules;
using TownOfUs.Roles;
using TownOfUs.Utilities;
using UnityEngine;

namespace DivaniMods.Patches;

[HarmonyPatch]
public static class MementoPatch
{
    private static readonly HashSet<byte> EjectedPlayers = new();

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CoStartGame))]
    [HarmonyPostfix]
    public static void ResetOnGameStart()
    {
        MementoModifier.RoleBeforeDeath.Clear();
        EjectedPlayers.Clear();
    }

    [RegisterEvent]
    public static void OnEjection(EjectionEvent evt)
    {
        var exiled = evt.ExileController?.initData?.networkedPlayer?.Object;
        if (exiled != null)
        {
            EjectedPlayers.Add(exiled.PlayerId);
        }
    }

    [RegisterEvent]
    public static void OnPlayerDeath(PlayerDeathEvent evt)
    {
        var pc = evt.Player;
        if (pc == null || pc.Data == null || !pc.HasModifier<MementoModifier>() || pc.HasModifier<MementoRevealModifier>())
        {
            return;
        }

        if (!OptionGroupSingleton<MementoOptions>.Instance.ShowIfEjected.Value && EjectedPlayers.Contains(pc.PlayerId))
        {
            return;
        }

        var role = MementoModifier.ResolveRoleBeforeDeath(pc.PlayerId) ?? pc.GetRoleWhenAlive();
        if (role == null)
        {
            return;
        }

        pc.AddModifier<MementoRevealModifier>(role);
    }

    [RegisterEvent]
    public static void OnPlayerRevive(PlayerReviveEvent evt)
    {
        var pc = evt.Player;
        if (pc == null)
        {
            return;
        }

        EjectedPlayers.Remove(pc.PlayerId);

        if (pc.TryGetModifier<MementoRevealModifier>(out var reveal))
        {
            reveal.ModifierComponent?.RemoveModifier(reveal);
        }
    }

    public static ModdedRoleTeams Faction(RoleBehaviour role)
    {
        if (role is ICustomRole custom)
        {
            return custom.Team;
        }

        if (role.IsImpostor)
        {
            return ModdedRoleTeams.Impostor;
        }

        return role.IsCrewmate() ? ModdedRoleTeams.Crewmate : ModdedRoleTeams.Custom;
    }

    public static Color FactionColor(RoleBehaviour role) => Faction(role) switch
    {
        ModdedRoleTeams.Impostor => Palette.ImpostorRed,
        ModdedRoleTeams.Crewmate => Palette.CrewmateBlue,
        _ => Color.gray,
    };

    public static string FactionName(RoleBehaviour role) => Faction(role) switch
    {
        ModdedRoleTeams.Impostor => "Impostor",
        ModdedRoleTeams.Crewmate => "Crewmate",
        _ => "Neutral",
    };

    public static string AlignmentName(RoleBehaviour role)
    {
        if (role is ITownOfUsRole touRole)
        {
            return touRole.RoleAlignment.ToDisplayString();
        }

        return FactionName(role);
    }
}
