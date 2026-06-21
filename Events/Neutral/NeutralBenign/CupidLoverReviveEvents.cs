using System.Collections;
using System.Collections.Generic;
using AmongUs.GameOptions;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using Reactor.Utilities;
using DivaniMods.Assets;
using DivaniMods.Options;
using DivaniMods.Roles.Neutral.NeutralBenign;
using TownOfUs.Events.TouEvents;
using TownOfUs.Modifiers.Game.Alliance;
using TownOfUs.Modules;
using TownOfUs.Options.Modifiers.Alliance;
using TownOfUs.Utilities;
using UnityEngine;

namespace DivaniMods.Events.Neutral.NeutralBenign;

public static class CupidLoverReviveEvents
{
    public static readonly Dictionary<byte, (byte LoverOne, byte LoverTwo)> FinalizedCouples = new();

    private static readonly HashSet<byte> ReviveInProgress = new();

    [RegisterEvent]
    public static void OnRoundStart(RoundStartEvent evt)
    {
        if (evt.TriggeredByIntro)
        {
            FinalizedCouples.Clear();
            ReviveInProgress.Clear();
        }
    }

    [RegisterEvent]
    public static void OnPlayerRevive(PlayerReviveEvent evt)
    {
        if (!OptionGroupSingleton<CupidOptions>.Instance.CupidRevivedOnLoversRevive.Value ||
            !OptionGroupSingleton<LoversOptions>.Instance.BothLoversDie)
        {
            return;
        }

        var revived = evt.Player;
        if (revived == null || !revived.HasModifier<LoverModifier>())
        {
            return;
        }

        foreach (var pair in FinalizedCouples)
        {
            var (loverOneId, loverTwoId) = pair.Value;
            if (revived.PlayerId != loverOneId && revived.PlayerId != loverTwoId)
            {
                continue;
            }

            var loverOne = MiscUtils.PlayerById(loverOneId);
            var loverTwo = MiscUtils.PlayerById(loverTwoId);
            if (loverOne == null || loverTwo == null || loverOne.HasDied() || loverTwo.HasDied())
            {
                return;
            }

            var cupid = MiscUtils.PlayerById(pair.Key);
            if (cupid == null || !cupid.HasDied() || ReviveInProgress.Contains(cupid.PlayerId))
            {
                return;
            }

            if (cupid.GetRoleWhenAlive() is not CupidRole)
            {
                return;
            }

            var liveRole = RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<CupidRole>());
            if (liveRole is null)
            {
                return;
            }

            ReviveInProgress.Add(cupid.PlayerId);

            var pos = (Vector2)revived.transform.position + new Vector2(0.4f, 0f);
            ReviveUtilities.RevivePlayer(
                reviver: cupid,
                revived: cupid,
                position: pos,
                roleWhenAlive: liveRole,
                flashColor: CupidRole.CupidColor,
                revivedOwnerNotificationText: "Your lovers were revived. You returned to the ship",
                reviverOwnerNotificationText: null,
                notificationIcon: DivaniAssets.CupidIcon.LoadAsset());

            Coroutines.Start(ClearInProgress(cupid.PlayerId));
            Coroutines.Start(RestoreCoupleAfterRevive(cupid, loverOneId, loverTwoId));
            return;
        }
    }

    private static IEnumerator ClearInProgress(byte id)
    {
        yield return new WaitForSeconds(1.0f);
        ReviveInProgress.Remove(id);
    }

    private static IEnumerator RestoreCoupleAfterRevive(PlayerControl cupid, byte loverOneId, byte loverTwoId)
    {
        var timeout = 5f;
        while (timeout > 0f)
        {
            if (cupid != null && cupid.Data?.Role is CupidRole { Finalized: false })
            {
                break;
            }

            timeout -= Time.deltaTime;
            yield return null;
        }

        if (cupid != null && cupid.Data?.Role is CupidRole role)
        {
            role.RestoreFinalizedCouple(MiscUtils.PlayerById(loverOneId), MiscUtils.PlayerById(loverTwoId));
        }
    }
}
