using MiraAPI.Events;
using MiraAPI.Events.Mira;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.GameOptions;
using DivaniMods.Networking.Crewmate.CrewmateKilling;
using DivaniMods.Options;
using DivaniMods.Roles.Crewmate.CrewmateAfterlife;
using DivaniMods.Roles.Crewmate.CrewmateKilling;
using TownOfUs.Events.TouEvents;
using TownOfUs.Modules;
using TownOfUs.Utilities;
using TownOfUs.Roles.Neutral;

namespace DivaniMods.Events.Crewmate.CrewmateKilling;

public static class RetributionistEvents
{
    [RegisterEvent]
    public static void OnAfterMurder(AfterMurderEvent evt)
    {
        if (!AmongUsClient.Instance || !AmongUsClient.Instance.AmHost)
        {
            return;
        }

        var target = evt.Target;
        var killer = evt.Source;

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

        if (target != null && target.GetRoleWhenAlive() is RetributionistRole &&
            killer != null && killer != target && !killer.HasDied() &&
            killer.Data?.Role is not PestilenceRole &&
            !MeetingHud.Instance)
        {
            var opts = OptionGroupSingleton<RetributionistOptions>.Instance;
            if (opts.TurnIntoSoulOnce && RetributionistManager.UsedRevenge.Contains(target.PlayerId))
            {
                return;
            }

            if (!opts.RevengeOnCrewmateKill && killer.IsCrewmate())
            {
                return;
            }

            var pos = target.transform.position;
            RetributionistRpc.RpcStartRevenge(target, killer, pos.x, pos.y);
        }
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
