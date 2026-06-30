using Reactor.Networking.Attributes;
using DivaniMods.Modules.Watcher;
using DivaniMods.Roles.Neutral.NeutralKilling;
using TownOfUs.Utilities;

namespace DivaniMods.Networking.Neutral.NeutralKilling;

public static class WatcherRpc
{
    [MethodRpc((uint)DivaniRpcCalls.WatcherStartLights)]
    public static void RpcStartLights(PlayerControl sender, float greenDuration, float redDuration, float grace, int loops)
    {
        if (sender == null || !sender.IsRole<WatcherRole>())
        {
            return;
        }

        WatcherLightSystem.Start(sender.PlayerId, greenDuration, redDuration, grace, loops);
    }

    [MethodRpc((uint)DivaniRpcCalls.WatcherReportMover)]
    public static void RpcReportMover(PlayerControl sender, byte moverId)
    {
        WatcherLightSystem.RegisterMover(moverId);
    }

    [MethodRpc((uint)DivaniRpcCalls.WatcherNeutralizeGhost)]
    public static void RpcNeutralizeGhost(PlayerControl sender, byte ghostId)
    {
        WatcherLightSystem.NeutralizeGhostLocal(ghostId);
    }
}
