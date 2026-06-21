using MiraAPI.Modifiers;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using DivaniMods.Assets;
using DivaniMods.Modifiers.Neutral.NeutralOutlier;
using DivaniMods.Modules.Duelist;
using DivaniMods.Roles.Neutral.NeutralOutlier;
using TownOfUs.Modifiers.Game.Universal;
using TownOfUs.Utilities;
using UnityEngine;

namespace DivaniMods.Networking.Neutral.NeutralOutlier;

public static class DuelistRpc
{
    [MethodRpc((uint)DivaniRpcCalls.DuelistStartDuel, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcStartDuel(PlayerControl duelist, byte targetId,
        Vector2 duelistDest, Vector2 targetDest, Vector2 duelistReturn, Vector2 targetReturn)
    {
        if (duelist == null || duelist.Data == null || duelist.Data.Role is not DuelistRole)
        {
            return;
        }

        var target = MiscUtils.PlayerById(targetId);
        if (target == null || target.HasDied())
        {
            return;
        }

        DuelManager.MarkInDuel(duelist.PlayerId);
        DuelManager.MarkInDuel(target.PlayerId);

        if (duelist.TryGetComponent<ModifierComponent>(out var duelistComp))
        {
            duelistComp.AddModifier(new DuelModifier(targetId, true, duelistReturn));
            Teleport(duelist, duelistDest);
        }
        if (target.TryGetComponent<ModifierComponent>(out var targetComp))
        {
            targetComp.AddModifier(new DuelModifier(duelist.PlayerId, false, targetReturn));
            Teleport(target, targetDest);
        }

        ShowStartNotifs(duelist, target);
    }

    [MethodRpc((uint)DivaniRpcCalls.DuelistAddWin, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcAddDuelWin(PlayerControl duelist)
    {
        if (duelist == null || duelist.Data == null || duelist.Data.Role is not DuelistRole)
        {
            return;
        }

        DuelManager.AddWin(duelist.PlayerId);
    }

    private static void ShowStartNotifs(PlayerControl duelist, PlayerControl target)
    {
        var local = PlayerControl.LocalPlayer;
        if (local == null)
        {
            return;
        }

        var hex = ColorUtility.ToHtmlStringRGB(DuelistRole.DuelistColor);
        var icon = DivaniAssets.DuelistIcon.LoadAsset();
        var pos = new Vector3(0f, 1f, -20f);

        if (local.PlayerId == target.PlayerId)
        {
            MiraAPI.Utilities.Helpers.CreateAndShowNotification(
                $"<b><color=#{hex}>You have been challenged to a duel by the Duelist! Show 'em what you're worth!</color></b>",
                Color.white, pos, spr: icon);
        }
        else if (local.PlayerId == duelist.PlayerId)
        {
            MiraAPI.Utilities.Helpers.CreateAndShowNotification(
                $"<b><color=#{hex}>Your duel against {target.Data.PlayerName} started. Show no mercy!</color></b>",
                Color.white, pos, spr: icon);
        }
    }

    private static void Teleport(PlayerControl player, Vector2 dest)
    {

        if (player.HasModifier<ImmovableModifier>())
        {
            return;
        }

        if (player.inVent)
        {
            player.MyPhysics.ExitAllVents();
        }

        player.MyPhysics.ResetMoveState();
        player.transform.position = dest;

        if (player.AmOwner)
        {
            player.NetTransform.RpcSnapTo(dest);
            MiscUtils.SnapPlayerCamera(player);
        }
    }
}
