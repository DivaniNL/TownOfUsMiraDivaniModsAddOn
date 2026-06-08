using HarmonyLib;
using MiraAPI.GameOptions;
using DivaniMods.Options;
using TownOfUs.Modules.RainbowMod;
using TownOfUs.Patches;
using TownOfUs.Utilities.Appearances;

namespace DivaniMods.Patches;

[HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
public static class RainbowCamoCommsPatch
{
    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last)]
    public static void Postfix()
    {
        if (!HudManagerPatches.CamouflageCommsEnabled)
        {
            return;
        }

        if (!OptionGroupSingleton<DivaniOptions>.Instance.RainbowCamoComms)
        {
            return;
        }

        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player == null || player.cosmetics == null)
            {
                continue;
            }

            if (player.GetAppearanceType() != TownOfUsAppearances.Camouflage)
            {
                continue;
            }

            var body = player.cosmetics.currentBodySprite?.BodySprite;
            if (body != null)
            {
                RainbowUtils.SetRainbow(body);
            }
        }
    }
}
