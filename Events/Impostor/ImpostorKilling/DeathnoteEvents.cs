using DivaniMods.Modifiers.Impostors;
using DivaniMods.Roles.Impostor.ImpostorKilling;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using MiraAPI.Utilities;

public class DeathNoteEvents
{
    [RegisterEvent]
    public static void OnPlayerDeath(PlayerDeathEvent @event)
    {
        if (!AmongUsClient.Instance.AmHost) return;

        var dead = @event.Player;
        var death = @event.DeathReason;

        if (dead.Data.Role is DeathnoteRole)
        {
            if (death == DeathReason.Exile)
            {
                foreach (var player in Helpers.GetAlivePlayers())
                {
                    if (player.HasModifier<DeathnoteModifier>())
                    {
                        if (player.Data.Role.IsImpostor) continue;
                        PlayerControl.LocalPlayer.RpcCustomMurder(player, true, teleportMurderer: false);
                    }
                    else
                    {
                        player.RemoveModifier<DeathnoteModifier>();
                    }
                }
            }
        }
    }
}
