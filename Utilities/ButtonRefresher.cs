using System.Collections;
using MiraAPI.Hud;
using Reactor.Utilities;
using UnityEngine;

namespace DivaniMods.Utilities;

public static class ButtonRefresher
{
    public static void RefreshAllButtons()
    {
        Coroutines.Start(RefreshButtonsDelayed());
    }
    
    private static IEnumerator RefreshButtonsDelayed()
    {
        yield return new WaitForSeconds(0.1f);
        
        if (!HudManager.InstanceExists || HudManager.Instance == null)
            yield break;
        
        var player = PlayerControl.LocalPlayer;
        if (player == null || player.Data == null)
            yield break;
        
        var role = player.Data.Role;
        
        foreach (var button in CustomButtonManager.Buttons)
        {
            if (button.Button == null)
            {
                var bottomLeft = HudManager.Instance.transform.Find("Buttons/BottomLeft");
                if (bottomLeft != null)
                {
                    button.CreateButton(bottomLeft);
                }
            }
            
            if (button.Button != null)
            {
                var shouldBeVisible = button.Enabled(role);
                button.Button.ToggleVisible(shouldBeVisible);
            }
        }
    }
}
