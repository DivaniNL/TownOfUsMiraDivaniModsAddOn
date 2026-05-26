using System.Linq;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using DivaniMods.Buttons.Impostor.ImpostorAfterlife;
using DivaniMods.Options;
using DivaniMods.Roles.Impostor.ImpostorAfterlife;
using DivaniMods.Roles.Impostor.ImpostorPower;
using TownOfUs.Events.TouEvents;
using TownOfUs.Utilities;

namespace DivaniMods.Events.Impostor.ImpostorPower;

public static class SummonerState
{
    public static readonly System.Collections.Generic.HashSet<string> RevenantNames = new();

    public static readonly System.Collections.Generic.HashSet<byte> KillVictims = new();

    public static int KillsSinceRevenant { get; set; }

    public static void ResetKills()
    {
        KillsSinceRevenant = 0;
        KillVictims.Clear();
    }

    public static int Required =>
        (int)OptionGroupSingleton<SummonerOptions>.Instance.KillsRequiredForSummon.Value;

    public static bool FinalFourOrLess =>
        Helpers.GetAlivePlayers().Count(p => p?.Data?.Role is not TownOfUs.Roles.IGhostRole) <= 4;

    public static bool SummonReady => KillsSinceRevenant >= Required && !FinalFourOrLess;
}

public static class SummonerEvents
{
    [RegisterEvent]
    public static void OnAfterMurder(AfterMurderEvent evt)
    {
        if (evt.Source != null && evt.Source.Data?.Role is SummonerRole && evt.Target != null)
        {
            SummonerState.KillsSinceRevenant++;
            SummonerState.KillVictims.Add(evt.Target.PlayerId);
        }
    }

    [RegisterEvent]
    public static void OnPlayerRevive(PlayerReviveEvent evt)
    {
        var revived = evt.Player;
        if (revived == null || revived.Data == null)
        {
            return;
        }

        if (SummonerState.KillVictims.Remove(revived.PlayerId) && SummonerState.KillsSinceRevenant > 0)
        {
            SummonerState.KillsSinceRevenant--;
        }
    }

    [RegisterEvent]
    public static void OnEndMeeting(EndMeetingEvent _)
    {
        if (!AmongUsClient.Instance || !AmongUsClient.Instance.AmHost)
        {
            return;
        }

        if (RevenantExists() || !SummonerState.SummonReady)
        {
            return;
        }

        foreach (var pc in PlayerControl.AllPlayerControls)
        {
            if (pc == null || pc.Data == null || pc.Data.Role is not SummonerRole summoner)
            {
                continue;
            }

            if (summoner.Player.Data == null || summoner.Player.Data.IsDead)
            {
                continue;
            }

            var id = summoner.PendingRecruitTargetId;
            if (id == byte.MaxValue)
            {
                continue;
            }

            var target = GameData.Instance.GetPlayerById(id)?.Object;
            if (!SummonerRole.IsValidRecruitTarget(target, summoner.Player))
            {
                continue;
            }

            target!.RpcChangeRole(RoleId.Get<RevenantRole>());
            break;
        }
    }

    [RegisterEvent]
    public static void OnRoundStart(RoundStartEvent evt)
    {
        if (evt.TriggeredByIntro)
        {
            SummonerState.ResetKills();
            SummonerState.RevenantNames.Clear();
        }

        if (PlayerControl.LocalPlayer?.Data?.Role is not RevenantRole)
        {
            return;
        }

        var killButton = CustomButtonSingleton<RevenantKillButton>.Instance;
        killButton.SetUses(killButton.MaxUses);
    }

    private static bool RevenantExists()
    {
        return PlayerControl.AllPlayerControls.ToArray()
            .Any(pc => pc != null && pc.Data?.Role is RevenantRole { GhostActive: true });
    }
}
