using MiraAPI.Events;
using MiraAPI.Events.Mira;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.GameOptions;
using DivaniMods.Assets;
using DivaniMods.Networking.Crewmate.CrewmateKilling;
using DivaniMods.Options;
using DivaniMods.Roles.Crewmate.CrewmateAfterlife;
using DivaniMods.Roles.Crewmate.CrewmateKilling;
using TownOfUs.Events.TouEvents;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Game.Alliance;
using TownOfUs.Modules;
using TownOfUs.Utilities;
using TownOfUs.Roles.Neutral;
using MiraAPI.Modifiers;
using UnityEngine;

namespace DivaniMods.Events.Crewmate.CrewmateKilling;

public static class RetributionistEvents
{
    [RegisterEvent]
    public static void OnAfterMurder(AfterMurderEvent evt)
    {
        var target = evt.Target;
        var killer = evt.Source;

        NotifyFirstDeathShieldBlockedRevenge(target, killer);

        if (!AmongUsClient.Instance || !AmongUsClient.Instance.AmHost)
        {
            return;
        }

        if (target != null && RetributionistManager.IsCursed(target.PlayerId))
        {
            var soulId = RetributionistManager.GetSoulHunting(target.PlayerId);
            if (soulId >= 0)
            {
                var soul = GameData.Instance?.GetPlayerById((byte)soulId)?.Object;
                if (soul != null && soul.Data?.Role is VengefulSoulRole)
                {
                    RetributionistRpc.RpcRevengeFailed(soul);
                }
            }
        }

        if (target == null)
        {
            return;
        }

        if (!IsRevengeEligible(target, killer))
        {
            RetributionistManager.ClearRevengePending(target.PlayerId);
            return;
        }

        var pos = target.transform.position;
        RetributionistRpc.RpcStartRevenge(target, killer!, pos.x, pos.y);
    }

    public static bool IsRevengeEligible(PlayerControl? target, PlayerControl? killer)
    {
        return CanSeekRevengeOn(target, killer) && !HasFirstDeathShield(killer);
    }

    public static bool IsRevengeBlockedByFirstDeathShield(PlayerControl? target, PlayerControl? killer)
    {
        return CanSeekRevengeOn(target, killer) && HasFirstDeathShield(killer);
    }

    private static bool HasFirstDeathShield(PlayerControl? killer)
    {
        return killer != null && killer.HasModifier<FirstDeadShield>();
    }

    private static void NotifyFirstDeathShieldBlockedRevenge(PlayerControl? target, PlayerControl? killer)
    {
        if (target == null || killer == null || !target.AmOwner ||
            !IsRevengeBlockedByFirstDeathShield(target, killer))
        {
            return;
        }

        var hex = ColorUtility.ToHtmlStringRGB(RetributionistRole.RetributionistColor);

        MiraAPI.Utilities.Helpers.CreateAndShowNotification(
            $"<b><color=#{hex}>{killer.Data.PlayerName} is protected by the first death shield so you won't be able to seek revenge this time. Feelsbadman</color></b>",
            Color.white,
            new Vector3(0f, 1f, -20f),
            spr: DivaniAssets.RetributionistIcon.LoadAsset());
    }

    private static bool CanSeekRevengeOn(PlayerControl? target, PlayerControl? killer)
    {
        if (target == null || killer == null || killer == target || killer.HasDied() ||
            target.GetRoleWhenAlive() is not RetributionistRole ||
            killer.Data?.Role is PestilenceRole ||
            MeetingHud.Instance)
        {
            return false;
        }

        if (ExileController.Instance)
        {
            return false;
        }

        var opts = OptionGroupSingleton<RetributionistOptions>.Instance;
        if (opts.TurnIntoSoulOnce && RetributionistManager.UsedRevenge.Contains(target.PlayerId))
        {
            return false;
        }

        if (!opts.RevengeOnCrewmateKill && killer.IsCrewmate())
        {
            return false;
        }

        if (target.TryGetModifier<LoverModifier>(out var love) && love.OtherLover == killer)
        {
            return false;
        }

        return true;
    }

    [RegisterEvent]
    public static void OnChangeRole(ChangeRoleEvent evt)
    {
        if (evt.OldRole is not VengefulSoulRole || evt.NewRole is not RetributionistRole)
        {
            return;
        }

        var soulId = evt.Player.PlayerId;
        var externalRevive = RetributionistManager.IsRevengeActive(soulId);

        RetributionistManager.EndRevenge(soulId);

        if (externalRevive)
        {
            RetributionistManager.RestoreRevengeCharge(soulId);
        }
    }

    [RegisterEvent]
    public static void OnMiraButtonClick(MiraButtonClickEvent evt)
    {
        if (evt.Button is MiraAPI.Hud.CustomActionButton button &&
            Patches.RetributionistCursePatches.ShouldCurseDisable(button))
        {
            evt.Cancel();
        }
    }

    [RegisterEvent]
    public static void OnStartMeeting(StartMeetingEvent evt)
    {
        RetributionistManager.ClearAllRevengePending();
        RetributionistManager.PurgeDisconnected();

        if (!AmongUsClient.Instance || !AmongUsClient.Instance.AmHost)
        {
            return;
        }

        foreach (var pc in PlayerControl.AllPlayerControls)
        {
            if (pc?.Data?.Role is VengefulSoulRole)
            {
                RetributionistRpc.RpcRevengeFailed(pc);
            }
        }
    }

    [RegisterEvent]
    public static void OnRoundStart(RoundStartEvent evt)
    {
        if (evt.TriggeredByIntro)
        {
            RetributionistManager.Reset();
            VengefulSoulRole.ResetActiveCount();
        }
    }
}
