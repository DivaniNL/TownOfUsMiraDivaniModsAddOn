using HarmonyLib;
using DivaniMods.Buttons;

namespace DivaniMods.Patches;

[HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
internal static class UsePortalButtonVisibilityPatch
{
    public static UsePortalButton? ButtonInstance { get; set; }
    
    private static void Postfix(HudManager __instance)
    {
        if (ButtonInstance?.Button == null) return;
        
        var player = PlayerControl.LocalPlayer;
        if (player == null || player.Data == null || player.Data.IsDead)
        {
            ButtonInstance.Button.gameObject.SetActive(false);
            return;
        }
        
        // Hide during meetings
        if (MeetingHud.Instance || ExileController.Instance)
        {
            ButtonInstance.Button.gameObject.SetActive(false);
            return;
        }
        
        if (!PortalManager.BothPortalsPlaced)
        {
            ButtonInstance.Button.gameObject.SetActive(false);
            return;
        }
        
        var position = player.GetTruePosition();
        bool nearPortal = PortalManager.IsNearPortal(position);
        
        if (nearPortal)
        {
            ButtonInstance.Button.gameObject.SetActive(true);
            
            // Show disabled state during comms sabotage
            if (PlayerTask.PlayerHasTaskOfType<IHudOverrideTask>(player))
            {
                ButtonInstance.Button.SetDisabled();
            }
        }
        else
        {
            ButtonInstance.Button.gameObject.SetActive(false);
        }
    }
}
