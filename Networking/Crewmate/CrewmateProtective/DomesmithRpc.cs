using Reactor.Networking.Attributes;
using Reactor.Utilities;
using DivaniMods.Buttons.Crewmate.CrewmateProtective;
using DivaniMods.Roles.Crewmate.CrewmateProtective;
using TownOfUs.Utilities;
using UnityEngine;

namespace DivaniMods.Networking.Crewmate.CrewmateProtective;

public static class DomesmithRpc
{
    [MethodRpc((uint)DivaniRpcCalls.DomesmithPlaceDome)]
    public static void RpcPlaceDome(PlayerControl sender, float x, float y, float z)
    {
        if (sender == null)
        {
            return;
        }
        DomeManager.PlaceDome(sender.PlayerId, new Vector3(x, y, z));
    }

    [MethodRpc((uint)DivaniRpcCalls.DomesmithRemoveDome)]
    public static void RpcClearDomes(PlayerControl sender)
    {
        DomeManager.Clear();
    }

    [MethodRpc((uint)DivaniRpcCalls.DomesmithBlockedKill)]
    public static void RpcDomeBlocked(PlayerControl sender, byte ownerId)
    {
        if (sender == null)
        {
            return;
        }

        var local = PlayerControl.LocalPlayer;
        if (local == null)
        {
            return;
        }

        if (local.PlayerId == sender.PlayerId || local.PlayerId == ownerId)
        {
            Coroutines.Start(MiscUtils.CoFlash(DomesmithRole.DomesmithColor, alpha: 0.5f));
        }
    }
}
