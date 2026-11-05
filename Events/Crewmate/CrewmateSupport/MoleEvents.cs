using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.Events.Vanilla.Usables;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using DivaniMods.Buttons.Crewmate.CrewmateSupport;
using DivaniMods.Options;
using DivaniMods.Roles.Crewmate.CrewmateSupport;

namespace DivaniMods.Events.Crewmate.CrewmateSupport;

public static class MoleEvents
{
    private static int ActiveTaskCount;
    private static uint LastUseTaskId = uint.MaxValue;

    [RegisterEvent]
    public static void RoundStartEventHandler(RoundStartEvent @event)
    {
        if (@event.TriggeredByIntro)
        {
            ActiveTaskCount = 0;
            LastUseTaskId = uint.MaxValue;
            MoleRole.ClearAll();
            return;
        }

        MoleRole.ProcessRoundEnd();

        var local = PlayerControl.LocalPlayer;
        if (local != null && local.Data?.Role is MoleRole mole)
        {
            mole.PlacePendingVents();
        }
    }

    [RegisterEvent]
    public static void OnCompleteTask(CompleteTaskEvent @event)
    {
        if (@event.Player == null || !@event.Player.AmOwner)
        {
            return;
        }

        if (@event.Player.Data?.Role is not MoleRole)
        {
            return;
        }

        var opt = OptionGroupSingleton<MoleOptions>.Instance;
        if (!opt.EarnMoreVents)
        {
            return;
        }

        if (@event.Task != null && @event.Task.Id != LastUseTaskId)
        {
            ++ActiveTaskCount;
            LastUseTaskId = @event.Task.Id;
        }

        var needed = (int)opt.TasksPerVent.Value;
        if (needed <= 0 || ActiveTaskCount < needed)
        {
            return;
        }

        ActiveTaskCount = 0;

        var button = CustomButtonSingleton<MoleDigButton>.Instance;
        if (button != null && button.LimitedUses)
        {
            ++button.UsesLeft;
            button.SetUses(button.UsesLeft);
        }
    }

    [RegisterEvent]
    public static void PlayerCanUseEventHandler(PlayerCanUseEvent @event)
    {
        if (OptionGroupSingleton<MoleOptions>.Instance.VentVisibility == MoleVentVisibility.Immediate)
        {
            return;
        }

        if (!@event.IsVent)
        {
            return;
        }

        var vent = @event.Usable.TryCast<Vent>();

        if (vent == null)
        {
            return;
        }

        if (vent.name.Contains("MoleVent") && PlayerControl.LocalPlayer.Data.Role is not MoleRole &&
            !vent.myRend.enabled)
        {
            @event.Cancel();
        }
    }

    [RegisterEvent]
    public static void EnterVentEventHandler(EnterVentEvent @event)
    {
        if (OptionGroupSingleton<MoleOptions>.Instance.VentVisibility == MoleVentVisibility.Immediate)
        {
            return;
        }

        var player = @event.Player;
        var vent = @event.Vent;

        if (player.Data.Role is not MoleRole)
        {
            return;
        }

        if (vent == null || !vent.name.Contains($"MoleVent-{player.PlayerId}"))
        {
            return;
        }

        MoleRole.RpcShowVent(player, vent.Id);
    }

    [RegisterEvent]
    public static void ExitVentEventHandler(ExitVentEvent @event)
    {
        if (OptionGroupSingleton<MoleOptions>.Instance.VentVisibility == MoleVentVisibility.Immediate)
        {
            return;
        }

        var player = @event.Player;
        var vent = @event.Vent;

        if (player.Data.Role is not MoleRole)
        {
            return;
        }

        if (vent == null || !vent.name.Contains($"MoleVent-{player.PlayerId}"))
        {
            return;
        }

        MoleRole.RpcShowVent(player, vent.Id);
    }
}
