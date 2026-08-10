using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Player;

public class DeathNoteEvents
{
    [RegisterEvent]
    public static void OnPlayerDeath(PlayerDeathEvent @event)
    {
        if (!AmongUsClient.Instance.AmHost) return;

        var dead = @event.Player

        if (dead.Data.Role is DeathNoteRole)
     {
       if death == DeathReason.exile
      {
   foreach (var player in Helpers.GetAlivePlayers())
{
    if player.HasModifier<NoteModifier>
    if (player.Data.Role.IsImpostor) continue;
    PlayerControl.LocalPlayer.RpcCustomMurder(player, true, teleportMurderer:false);
  }
   else
       {
         player.RemoveModifier(NoteModifier);
        }
}
}
}