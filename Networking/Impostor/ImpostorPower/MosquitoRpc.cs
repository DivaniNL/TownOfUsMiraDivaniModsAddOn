using MiraAPI.Hud;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using DivaniMods.Buttons.Impostor.ImpostorPower;
using DivaniMods.Modules.Mosquito;
using DivaniMods.Roles.Impostor.ImpostorPower;
using UnityEngine;

namespace DivaniMods.Networking.Impostor.ImpostorPower;

public static class MosquitoRpc
{
    [MethodRpc((uint)DivaniRpcCalls.MosquitoSpawn, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcSpawnMosquito(PlayerControl shooter, byte targetId, float destX, float destY, bool aimbot)
    {
        if (shooter == null || shooter.Data == null || shooter.Data.Role is not MosquitoRole)
        {
            return;
        }

        var go = new GameObject("Mosquito");
        var start = shooter.GetTruePosition();
        go.transform.position = new Vector3(start.x, start.y, (start.y / 1000f) - 0.5f);

        var mosquito = go.AddComponent<MosquitoObject>();
        mosquito.Configure(shooter.PlayerId, targetId, new Vector2(destX, destY), aimbot);
    }

    [MethodRpc((uint)DivaniRpcCalls.MosquitoSwat, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcSwatMosquito(PlayerControl swatter, byte shooterId)
    {
        foreach (var mosquito in UnityEngine.Object.FindObjectsOfType<MosquitoObject>())
        {
            if (mosquito != null && mosquito.ShooterId == shooterId)
            {
                mosquito.Swat();
            }
        }

        // Getting swatted refreshes the shooter's cooldown too.
        ResetStingCooldown(shooterId);
    }

    public static void ResetStingCooldown(byte shooterId)
    {
        var local = PlayerControl.LocalPlayer;
        if (local == null || local.PlayerId != shooterId)
        {
            return;
        }

        var button = CustomButtonSingleton<MosquitoStingButton>.Instance;
        if (button != null)
        {
            button.Timer = button.Cooldown;
        }
    }
}
