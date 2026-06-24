using MiraAPI.GameOptions;
using MiraAPI.Hud;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using DivaniMods.Assets;
using DivaniMods.Options;
using DivaniMods.Roles.Crewmate.CrewmateSupport;
using TownOfUs.Roles;
using TownOfUs.Utilities;
using UnityEngine;
using MiraAPI.Modifiers;
using TownOfUs.Modifiers.Game.Alliance;

namespace DivaniMods.Networking.Crewmate.CrewmateSupport;

public static class ClockstopperRpc
{
    [MethodRpc((uint)DivaniRpcCalls.ClockstopperResetCooldowns, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcResetCooldowns(PlayerControl clockstopper)
    {
        if (clockstopper == null || clockstopper.Data?.Role is not ClockstopperRole || clockstopper.HasDied())
        {
            return;
        }

        var local = PlayerControl.LocalPlayer;
        if (local == null || local.Data?.Role == null || local.HasDied())
        {
            return;
        }

        var hex = ColorUtility.ToHtmlStringRGB(new Color32(175, 138, 162, 255));
        var icon = DivaniAssets.ClockstopperIcon.LoadAsset();
        var pos = new Vector3(0f, 1f, -20f);

        if (local.PlayerId == clockstopper.PlayerId)
        {
            var perReset = (int)OptionGroupSingleton<ClockstopperOptions>.Instance.TasksPerReset.Value;
            MiraAPI.Utilities.Helpers.CreateAndShowNotification(
                $"<b><color=#{hex}>Completed {perReset} more tasks, cooldowns are reset</color></b>",
                Color.white, pos, spr: icon);
            return;
        }

        if (!ShouldResetFor(local, clockstopper))
        {
            return;
        }

        var role = local.Data.Role;
        foreach (var button in CustomButtonManager.Buttons)
        {
            if (button == null || !button.Enabled(role))
            {
                continue;
            }

            button.EffectActive = false;
            button.SetTimer(Mathf.Max(button.Cooldown, button.Timer));
        }

        if (local.IsImpostor())
        {
            local.SetKillTimer(Mathf.Max(local.GetKillCooldown(), local.killTimer));
        }

        MiraAPI.Utilities.Helpers.CreateAndShowNotification(
            $"<b><color=#{hex}>Your cooldown has been reset by the Clockstopper</color></b>",
            Color.white, pos, spr: icon);
    }

    private static bool ShouldResetFor(PlayerControl player, PlayerControl clockstopper)
    {
        if (clockstopper.IsImpostorAligned())
        {
            return !player.IsImpostorAligned();
        }
        
        if (clockstopper.HasModifier<EgotistModifier>())
        {
            return player.IsCrewmate();
        }

        if (clockstopper.IsLover())
        {
            return !player.IsLover();
        }

        if (player.IsImpostorAligned())
        {
            return true;
        }

        var opt = OptionGroupSingleton<ClockstopperOptions>.Instance;
        return (player.Is(RoleAlignment.NeutralBenign) && opt.ResetNeutralBenign) ||
               (player.Is(RoleAlignment.NeutralEvil) && opt.ResetNeutralEvil) ||
               (player.Is(RoleAlignment.NeutralKilling) && opt.ResetNeutralKilling) ||
               (player.Is(RoleAlignment.NeutralOutlier) && opt.ResetNeutralOutlier);
    }
}
