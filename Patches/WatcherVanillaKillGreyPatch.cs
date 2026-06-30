using HarmonyLib;
using DivaniMods.Modules.Watcher;

namespace DivaniMods.Patches;

// Visual only: the actual block is in WatcherEvents.BeforeMurderHandler. Grey the vanilla
// kill button (UseVanillaKillButton roles) while Red Light is watching the local player,
// so a watched player sees it disabled instead of clicking a live-looking button that misfires.
[HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
public static class WatcherVanillaKillGreyPatch
{
    [HarmonyPostfix]
    public static void Postfix(HudManager __instance)
    {
        var me = PlayerControl.LocalPlayer;
        var kill = __instance.KillButton;
        if (me == null || kill == null || !kill.isActiveAndEnabled)
        {
            return;
        }

        if (WatcherLightSystem.BlocksKill(me))
        {
            kill.SetDisabled();
        }
    }
}
