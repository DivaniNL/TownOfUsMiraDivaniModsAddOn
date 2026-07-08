using HarmonyLib;
using DivaniMods.Roles.Impostor.ImpostorAfterlife;

namespace DivaniMods.Patches;

[HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
public static class RevenantHideVanillaVentButtonPatch
{
    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last)]
    public static void Postfix(HudManager __instance)
    {
        if (PlayerControl.LocalPlayer?.Data?.Role is not RevenantRole)
        {
            return;
        }

        var ventButton = __instance.ImpostorVentButton;
        if (ventButton != null)
        {
            ventButton.ToggleVisible(false);
        }
    }
}
