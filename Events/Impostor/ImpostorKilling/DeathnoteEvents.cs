using DivaniMods.Modifiers.Impostors;
using DivaniMods.Roles.Impostor.ImpostorKilling;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using MiraAPI.Translation;
using MiraAPI.Utilities;

public class DeathNoteEvents
{
    [RegisterEvent]
    public static void OnEjection(EjectionEvent evt)
    {
        if (!AmongUsClient.Instance.AmHost)
        {
            return;
        }

        var exiled = evt.ExileController?.initData?.networkedPlayer?.Object;
        if (exiled == null || exiled.Data == null || exiled.Data.Role is not DeathnoteRole)
        {
            return;
        }

        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player == null || player.Data == null || player.Data.IsDead || player.PlayerId == exiled.PlayerId)
            {
                continue;
            }

            if (!player.HasModifier<DeathnoteModifier>() || player.Data.Role.IsImpostor)
            {
                continue;
            }

            TownOfUs.Modules.GameHistory.UpdatePlayerDeathData(player, MiraLocaleManager.Get("DiedToDeathnote"),
                roundOfDeath: TownOfUs.Modules.Components.HudManagerHelper.Instance.CurrentRound,
                diedThisRound: TownOfUs.Modules.DeathHandlerOverride.SetFalse,
                lockInfo: TownOfUs.Modules.DeathHandlerOverride.SetTrue);

            exiled.RpcCustomMurder(player, MeetingCheck.Ignore);
        }
    }
}
