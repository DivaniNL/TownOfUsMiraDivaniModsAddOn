using System.Collections.Generic;
using HarmonyLib;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using Reactor.Utilities;
using DivaniMods.Buttons.Crewmate.CrewmateInvestigative;
using DivaniMods.Roles.Crewmate.CrewmateKilling;
using DivaniMods.Roles.Impostor.ImpostorConcealing;
using TownOfUs.Modifiers.Game.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;
using TownOfUs.Modules;

namespace DivaniMods.Patches;

public static class CunctatorBodyManager
{
    private sealed class PendingBody
    {
        public byte TargetId;
        public byte KillerId;
        public Vector3 Position;
        public float Remaining;
    }

    private static readonly List<PendingBody> Pending = [];

    public static void Schedule(PlayerControl target, PlayerControl killer, float delaySeconds)
    {
        if (target == null)
        {
            return;
        }

        var body = Object.FindObjectsOfType<DeadBody>()
            .FirstOrDefault(b => b != null && b.ParentId == target.PlayerId);

        var position = body != null ? body.transform.position : (Vector3)target.GetTruePosition();

        if (body != null)
        {
            body.gameObject.SetActive(false);
            Object.Destroy(body.gameObject);
        }

        BeaconManager.ForgetBody(target.PlayerId);

        Pending.RemoveAll(p => p.TargetId == target.PlayerId);
        Pending.Add(new PendingBody
        {
            TargetId = target.PlayerId,
            KillerId = killer != null ? killer.PlayerId : target.PlayerId,
            Position = position,
            Remaining = delaySeconds,
        });
    }

    public static void Clear()
    {
        Pending.Clear();
    }

    private static void Tick()
    {
        if (Pending.Count == 0)
        {
            return;
        }

        if (!ShipStatus.Instance || MeetingHud.Instance || ExileController.Instance)
        {
            return;
        }

        for (var i = Pending.Count - 1; i >= 0; i--)
        {
            var pending = Pending[i];
            pending.Remaining -= Time.deltaTime;
            if (pending.Remaining > 0f)
            {
                continue;
            }

            SpawnBody(pending);
            Pending.RemoveAt(i);
        }
    }

    private static void SpawnBody(PendingBody pending)
    {
        var target = MiscUtils.PlayerById(pending.TargetId);
        if (target == null)
        {
            return;
        }

        var deadBody = Object.Instantiate(GameManager.Instance.deadBodyPrefab[0]);
        deadBody.enabled = false;
        deadBody.ParentId = pending.TargetId;
        deadBody.bodyRenderers.Do(r => target.SetPlayerMaterialColors(r));
        target.SetPlayerMaterialColors(deadBody.bloodSplatter);
        deadBody.transform.position = pending.Position;
        deadBody.enabled = true;

        if (target.HasModifier<RottingModifier>())
        {
            var killer = MiscUtils.PlayerById(pending.KillerId) ?? target;
            Coroutines.Start(RottingModifier.StartRotting(target, killer));
        }
    }
    [HarmonyPatch(typeof(CustomMurderRpc), nameof(CustomMurderRpc.CustomMurder))]
    public static class SuppressBodyPatch
    {
        public static void Prefix(PlayerControl source, PlayerControl target, ref bool createDeadBody)
        {
            if (createDeadBody &&
                source != null &&
                target != null &&
                source.PlayerId != target.PlayerId &&
                source.Data?.Role is CunctatorRole &&
                target.GetRoleWhenAlive() is not RetributionistRole &&
                !MeetingHud.Instance &&
                !ExileController.Instance)
            {
                createDeadBody = false;
            }
        }
    }

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    public static class HudUpdatePatch
    {
        public static void Postfix()
        {
            Tick();
        }
    }

    // A meeting cancels any body that hasn't dropped yet — it must never appear afterwards.
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
    public static class MeetingResetPatch
    {
        public static void Postfix()
        {
            Clear();
        }
    }

    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.CoBegin))]
    public static class IntroResetPatch
    {
        public static void Postfix()
        {
            Clear();
        }
    }

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
    public static class GameEndResetPatch
    {
        public static void Postfix()
        {
            Clear();
        }
    }
}
