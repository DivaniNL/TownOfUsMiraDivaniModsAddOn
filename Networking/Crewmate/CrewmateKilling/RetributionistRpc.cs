using System.Collections;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using Reactor.Utilities;
using DivaniMods.Assets;
using DivaniMods.Events.Crewmate.CrewmateKilling;
using DivaniMods.Modifiers.Crewmate.CrewmateKilling;
using DivaniMods.Options;
using DivaniMods.Roles.Crewmate.CrewmateAfterlife;
using DivaniMods.Roles.Crewmate.CrewmateKilling;
using TownOfUs.Events;
using TownOfUs.Modifiers;
using TownOfUs.Modules;
using TownOfUs.Modules.Localization;
using TownOfUs.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;
using AmongUs.GameOptions;
using MiraAPI.Networking;

namespace DivaniMods.Networking.Crewmate.CrewmateKilling;

public static class RetributionistRpc
{
    [MethodRpc((uint)DivaniRpcCalls.RetributionistStartRevenge, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcStartRevenge(PlayerControl soul, PlayerControl killer, float x, float y)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(soul);
            return;
        }

        if (soul == null || killer == null)
        {
            return;
        }

        var deathPos = new Vector2(x, y);

        RetributionistManager.StartRevenge(soul.PlayerId, killer.PlayerId, deathPos);

        VanishBody(soul.PlayerId);
        Coroutines.Start(CoVanishLingeringBody(soul.PlayerId, 1.0f));

        soul.ChangeRole((ushort)RoleId.Get<VengefulSoulRole>(), false);

        if (soul.Data?.Role is VengefulSoulRole vengefulSoul)
        {
            vengefulSoul.Spawn();
        }

        if (soul.AmOwner)
        {
            var duration = OptionGroupSingleton<RetributionistOptions>.Instance.RevengeTimer;
            if (!soul.HasModifier<RevengeTimerModifier>())
            {
                soul.AddModifier<RevengeTimerModifier>(duration);
            }
        }

        var hex = ColorUtility.ToHtmlStringRGB(RetributionistRole.RetributionistColor);

        if (killer.AmOwner)
        {
            Coroutines.Start(MiscUtils.CoFlash(RetributionistRole.RetributionistColor));
            Helpers.CreateAndShowNotification(
                $"<b><color=#{hex}>A Vengeful Soul is on the loose...</color></b>",
                Color.white,
                new Vector3(0f, 1f, -20f),
                spr: DivaniAssets.RetributionistIcon.LoadAsset());
        }

        if (soul.AmOwner)
        {
            Helpers.CreateAndShowNotification(
                $"<b><color=#{hex}>You are a Vengeful Soul. Seek revenge on your killer!</color></b>",
                Color.white,
                new Vector3(0f, 1f, -20f),
                spr: DivaniAssets.RetributionistIcon.LoadAsset());
        }
    }

    [MethodRpc((uint)DivaniRpcCalls.RetributionistRevenge, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcRevenge(PlayerControl soul, PlayerControl killer)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(soul);
            return;
        }

        if (soul == null || killer == null || !soul.HasDied())
        {
            return;
        }

        if (soul.Data?.Role is not VengefulSoulRole)
        {
            return;
        }

        Coroutines.Start(CoRevenge(soul, killer));
    }

    private static IEnumerator CoRevenge(PlayerControl soul, PlayerControl killer)
    {
        var cause = "Retaliated";

        DeathHandlerModifier.UpdateDeathHandlerImmediate(
            killer,
            cause,
            DeathEventHandlers.CurrentRound,
            DeathHandlerOverride.SetTrue,
            TouLocale.GetParsed("DiedByStringBasic").Replace("<player>", soul.Data.PlayerName),
            lockInfo: DeathHandlerOverride.SetTrue);

        while (DeathHandlerModifier.IsAltCoroutineRunning)
        {
            yield return null;
        }

        if (killer.TryGetModifier<DeathHandlerModifier>(out var deathHandler))
        {
            deathHandler.CauseOfDeath = cause;
            deathHandler.RoundOfDeath = DeathEventHandlers.CurrentRound;
            deathHandler.DiedThisRound = true;
            deathHandler.LockInfo = true;
        }

        var revivePos = (Vector2)soul.transform.position;
        var roleWhenAlive = RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<RetributionistRole>());
        RetributionistManager.EndRevenge(soul.PlayerId);

        if (killer.Data != null && !killer.Data.IsDead &&
            AmongUsClient.Instance && AmongUsClient.Instance.AmHost)
        {
            soul.RpcCustomMurder(killer, MeetingCheck.OutsideMeeting, didSucceed: true,
                resetKillTimer: false, createDeadBody: true, teleportMurderer: false,
                showKillAnim: false, playKillSound: true);
        }

        ReviveUtilities.RevivePlayer(
            reviver: soul,
            revived: soul,
            position: revivePos,
            roleWhenAlive: roleWhenAlive,
            flashColor: RetributionistRole.RetributionistColor,
            revivedOwnerNotificationText: "Your revenge is complete. You returned to the ship",
            reviverOwnerNotificationText: null,
            notificationIcon: DivaniAssets.RetributionistIcon.LoadAsset());
    }

    [MethodRpc((uint)DivaniRpcCalls.RetributionistRevengeFailed, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcRevengeFailed(PlayerControl soul)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(soul);
            return;
        }

        if (soul == null)
        {
            return;
        }

        RetributionistManager.TryGetKiller(soul.PlayerId, out var killer);

        if (soul.Data?.Role is VengefulSoulRole)
        {
            soul.ChangeRole((ushort)RoleTypes.CrewmateGhost, false);
        }

        RetributionistManager.EndRevenge(soul.PlayerId);

        var hex = ColorUtility.ToHtmlStringRGB(RetributionistRole.RetributionistColor);

        if (soul.AmOwner)
        {
            Helpers.CreateAndShowNotification(
                $"<b><color=#{hex}>Your revenge failed.</color></b>",
                Color.white,
                new Vector3(0f, 1f, -20f),
                spr: DivaniAssets.RetributionistIcon.LoadAsset());
        }

        if (killer != null && killer.AmOwner)
        {
            Coroutines.Start(MiscUtils.CoFlash(RetributionistRole.RetributionistColor));
            Helpers.CreateAndShowNotification(
                $"<b><color=#{hex}>The Vengeful Soul failed to take its revenge. You live to fight another day.</color></b>",
                Color.white,
                new Vector3(0f, 1f, -20f),
                spr: DivaniAssets.RetributionistIcon.LoadAsset());
        }
    }

    private static void VanishBody(byte bodyId)
    {
        foreach (var body in Object.FindObjectsOfType<DeadBody>())
        {
            if (body != null && body.ParentId == bodyId)
            {
                Object.Destroy(body.gameObject);
            }
        }
    }

    private static IEnumerator CoVanishLingeringBody(byte bodyId, float timeoutSeconds)
    {
        const float step = 0.05f;
        var waited = 0f;

        while (waited < timeoutSeconds)
        {
            VanishBody(bodyId);
            waited += step;
            yield return new WaitForSeconds(step);
        }
    }
}
