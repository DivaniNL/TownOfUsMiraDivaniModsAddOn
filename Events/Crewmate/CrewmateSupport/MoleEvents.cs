using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Usables;
using MiraAPI.GameOptions;
using DivaniMods.Options;
using DivaniMods.Roles.Crewmate.CrewmateSupport;

namespace DivaniMods.Events.Crewmate.CrewmateSupport;

public static class MoleEvents
{
    [RegisterEvent]
    public static void RoundStartEventHandler(RoundStartEvent @event)
    {
        if (@event.TriggeredByIntro)
        {
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
