using System.Collections;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using Reactor.Utilities;
using DivaniMods.Roles.Impostor.ImpostorAfterlife;
using TownOfUs.Events;
using TownOfUs.Modifiers;
using TownOfUs.Modules.Localization;
using TownOfUs.Utilities;
using TownOfUs.Modules;
using TownOfUs.Modules.Components;

namespace DivaniMods.Networking.Impostor.ImpostorAfterlife;

public static class RevenantRpc
{
    [MethodRpc((uint)DivaniRpcCalls.RevenantKill, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcRevenantKill(PlayerControl source, PlayerControl target)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(source);
            return;
        }

        if (source == null || target == null || !source.HasDied() || target.HasDied())
        {
            return;
        }

        if (source.Data?.Role is not RevenantRole)
        {
            return;
        }

        source.AddModifier<IndirectAttackerModifier>(true);

        Coroutines.Start(CoRevenantKill(source, target));
    }

    private static IEnumerator CoRevenantKill(PlayerControl source, PlayerControl target)
    {
        var cause = TouLocale.Get("DiedToRevenant");

        GameHistory.UpdatePlayerDeathData(
            target,
            cause,
            roundOfDeath: HudManagerHelper.Instance.CurrentRound,
            diedThisRound: DeathHandlerOverride.SetTrue,
            killedBy: TouLocale.GetParsed("DiedByStringBasic").Replace("<player>", source.Data.PlayerName),
            lockInfo: DeathHandlerOverride.SetTrue);

        GameHistory.UpdatePlayerDeathData(
            source,
            roundOfDeath: -1,
            diedThisRound: DeathHandlerOverride.SetFalse,
            lockInfo: DeathHandlerOverride.SetTrue);

        if (target.Data == null || target.Data.IsDead)
        {
            yield break;
        }

        source.CustomMurder(target, MurderResultFlags.Succeeded, createDeadBody: true,
            teleportMurderer: true, showKillAnim: true, playKillSound: true);
    }
}
