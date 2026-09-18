using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.LocalSettings;
using MiraAPI.Modifiers;
using DivaniMods.Modules;
using DivaniMods.Options;
using TownOfUs.Modifiers.Impostor.Venerer;
using TownOfUs.Modules;
using TownOfUs.Modules.RainbowMod;
using TownOfUs.Patches;
using TownOfUs.Utilities.Appearances;
using UnityEngine;

namespace DivaniMods.Patches;

[HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
public static class RainbowCamoCommsPatch
{
    private static int _lastRefreshFrame;

    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last)]
    public static void Postfix()
    {
        if (!OptionGroupSingleton<DivaniOptions>.Instance.RainbowCamoComms)
        {
            return;
        }

        // Local opt-out: when set, camo bodies keep their default grey instead of rainbow.
        if (LocalSettingsTabSingleton<DivaniLocalSettings>.Instance.DisableRainbowComms.Value)
        {
            return;
        }

        // This is a purely cosmetic effect; refreshing every frame is unnecessary and
        // causes recurring work on every client when camo is active. Re-check on a short
        // cadence instead of every HUD tick while preserving the same appearance behavior.
        if (Time.frameCount - _lastRefreshFrame < 8)
        {
            return;
        }

        _lastRefreshFrame = Time.frameCount;

        var commsCamo = HudManagerPatches.CamouflageCommsEnabled;

        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player == null || player.cosmetics == null)
            {
                continue;
            }

            if (!player.TryGetComponent<ModifierComponent>(out var modComp))
            {
                continue;
            }

            var isCommsCamo = commsCamo && player.GetAppearanceType() == TownOfUsAppearances.Camouflage;
            if (!isCommsCamo && !modComp.HasModifier<VenererCamouflageModifier>())
            {
                continue;
            }

            var body = player.cosmetics.currentBodySprite?.BodySprite;
            if (body != null)
            {
                RainbowUtils.SetRainbow(body);
            }
        }

        if (!commsCamo)
        {
            return;
        }

        foreach (var fakePlayer in FakePlayer.FakePlayers)
        {
            if (fakePlayer?.body == null)
            {
                continue;
            }

            var cosmetics = fakePlayer.body.GetComponentInChildren<CosmeticsLayer>();
            var body = cosmetics?.currentBodySprite?.BodySprite;
            if (body != null)
            {
                RainbowUtils.SetRainbow(body);
            }
        }
    }
}
