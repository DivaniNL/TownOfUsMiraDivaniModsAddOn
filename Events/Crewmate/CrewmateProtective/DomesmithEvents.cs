using MiraAPI.Events;
using MiraAPI.Events.Mira;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.Events.Vanilla.Usables;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Roles;
using Reactor.Utilities;
using DivaniMods.Buttons.Crewmate.CrewmateProtective;
using DivaniMods.Networking.Crewmate.CrewmateProtective;
using DivaniMods.Options;
using DivaniMods.Patches;
using DivaniMods.Roles.Crewmate.CrewmateProtective;
using TownOfUs.Buttons;
using TownOfUs.Options;
using TownOfUs.Utilities;
using MiraAPI.Modifiers;
using TownOfUs.Modifiers.Game.Alliance;
using TownOfUs.Roles;

namespace DivaniMods.Events.Crewmate.CrewmateProtective;

public static class DomesmithEvents
{
    private static int ActiveTaskCount;
    private static uint LastUseTaskId = uint.MaxValue;

    [RegisterEvent]
    public static void OnRoundStart(RoundStartEvent _)
    {
        ActiveTaskCount = 0;
        LastUseTaskId = uint.MaxValue;

        if (PlayerControl.LocalPlayer?.Data?.Role is DomesmithRole)
        {
            var charges = (int)OptionGroupSingleton<DomesmithOptions>.Instance.Charges.Value;
            var instance = PlaceDomeButton.Instance;
            instance?.ResetPlacing();
            instance?.SetUses(charges);
        }

        DomeManager.RefreshVisibility();
    }

    [RegisterEvent]
    public static void OnMeetingStart(StartMeetingEvent _)
    {
        DomeManager.SetVisibleAll(false);
    }

    [RegisterEvent]
    public static void OnMeetingEnd(EndMeetingEvent _)
    {
        DomeManager.RefreshVisibility();
    }

    [RegisterEvent]
    public static void OnCompleteTask(CompleteTaskEvent @event)
    {
        if (@event.Player == null || !@event.Player.AmOwner)
        {
            return;
        }

        if (@event.Player.Data?.Role is not DomesmithRole)
        {
            return;
        }

        if (@event.Task != null && @event.Task.Id != LastUseTaskId)
        {
            ++ActiveTaskCount;
            LastUseTaskId = @event.Task.Id;
        }

        var opt = OptionGroupSingleton<DomesmithOptions>.Instance;
        var btn = PlaceDomeButton.Instance;
        if (btn != null && btn.LimitedUses && opt.UsesPerTasks.Value != 0 && opt.UsesPerTasks.Value <= ActiveTaskCount)
        {
            ++btn.UsesLeft;
            btn.SetUses(btn.UsesLeft);
            ActiveTaskCount = 0;
        }
    }

    [RegisterEvent]
    public static void OnEjection(EjectionEvent evt)
    {
        var exiled = evt.ExileController?.initData?.networkedPlayer?.Object;
        if (exiled == null)
        {
            return;
        }

        if (exiled.Data?.Role is DomesmithRole)
        {
            DomeManager.Clear();
        }
    }

    [RegisterEvent]
    public static void OnAfterMurder(AfterMurderEvent evt)
    {
        if (evt.Target?.Data?.Role is DomesmithRole)
        {
            DomeManager.Clear();
        }

        if (evt.Target != null && evt.Target.AmOwner)
        {
            DomeManager.RefreshVisibility();
        }
    }

    [RegisterEvent]
    public static void OnBeforeMurder(BeforeMurderEvent evt)
    {
        if (evt.Source == null || evt.Target == null)
        {
            return;
        }

        CheckForDome(evt, evt.Source, evt.Target, button: null);
    }

    [RegisterEvent]
    public static void OnButtonClick(MiraButtonClickEvent evt)
    {
        var source = PlayerControl.LocalPlayer;
        var button = evt.Button as CustomActionButton<PlayerControl>;
        var target = button?.Target;
        if (source == null || target == null || button is not IKillButton || !button.CanClick())
        {
            return;
        }

        CheckForDome(evt, source, target, button);
    }

    private static bool CheckForDome(
        MiraCancelableEvent evt,
        PlayerControl source,
        PlayerControl target,
        CustomActionButton<PlayerControl>? button)
    {
        var opts = OptionGroupSingleton<DomesmithOptions>.Instance;

        if (MeetingHud.Instance || ExileController.Instance)
        {
            return false;
        }
        if (source.PlayerId == target.PlayerId)
        {
            return false;
        }

        if (RuthlessRpcPatches.IsRuthlessAttacker(source))
        {
            return false;
        }

        if (opts.AllowCrewmateKillsInDome && source.IsCrewmate())
        {
            return false;
        }

        var dome = DomeManager.FindContaining(source.GetTruePosition())
                   ?? DomeManager.FindContaining(target.GetTruePosition());
        if (dome == null)
        {
            return false;
        }

        var domeOwner = GameData.Instance.GetPlayerById(dome.OwnerId)?.Object;

        if (domeOwner == null)
        {
            return false;
        }

        if (domeOwner.HasModifier<EgotistModifier>() && target.IsCrewmate() && !source.Is(RoleAlignment.CrewmateKilling) && target.PlayerId != domeOwner.PlayerId)
        {
            return false;
        }

        if (domeOwner.HasModifier<CrewpostorModifier>() && !target.IsImpostorAligned() && source.IsImpostorAligned())
        {
            return false;
        }

        evt.Cancel();

        if (source.AmOwner)
        {
            ResetKillerTimer(source, button);
            DomesmithRpc.RpcDomeBlocked(source, dome.OwnerId);
        }
        return true;
    }

    private static void ResetKillerTimer(PlayerControl source, CustomActionButton<PlayerControl>? button)
    {
        if (!source.AmOwner)
        {
            return;
        }

        var reset = OptionGroupSingleton<GeneralOptions>.Instance.TempSaveCdReset;
        button?.SetTimer(reset);
        source.SetKillTimer(reset);
    }
}
