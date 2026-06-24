using HarmonyLib;
using MiraAPI.Modifiers;
using Reactor.Localization.Utilities;
using DivaniMods.Assets;
using DivaniMods.Modifiers.Game.Universal;
using UnityEngine;

namespace DivaniMods.Patches;

[HarmonyPatch(typeof(SpawnInMinigame))]
internal static class TacticalInsertionSpawnPatch
{
    private static readonly StringNames TacticalLabel =
        CustomStringName.CreateAndRegister("Tactical Insertion");

    [HarmonyPatch(nameof(SpawnInMinigame.Begin))]
    [HarmonyPrefix]
    [HarmonyPriority(Priority.Last)]
    [HarmonyAfter("auavengers.tou.mira")]
    public static void BeginPrefix(SpawnInMinigame __instance)
    {
        if (__instance.Locations == null || __instance.Locations.Length == 0)
        {
            return;
        }

        var player = PlayerControl.LocalPlayer;

        if (player == null || player.Data == null || player.Data.IsDead)
        {
            return;
        }

        var modifier = player.GetModifier<TacticalInsertionModifier>();
        if (modifier?.MarkedLocation is not { } mark)
        {
            return;
        }

        var template = __instance.Locations[0];
        var tactical = new SpawnInMinigame.SpawnLocation
        {
            Name = TacticalLabel,
            Location = new Vector3(mark.x, mark.y, template.Location.z),
            Image = DivaniAssets.TacInsertSpawnSprite.LoadAsset(),
            Rollover = DivaniAssets.FlareHover.LoadAsset(),
            RolloverSfx = DivaniAssets.TacInsertHoverSound.LoadAsset(),
        };

        var slots = __instance.LocationButtons != null && __instance.LocationButtons.Length > 0
            ? __instance.LocationButtons.Length
            : __instance.Locations.Length;

        var keep = Math.Max(0, Math.Min(__instance.Locations.Length, slots - 1));
        var final = new SpawnInMinigame.SpawnLocation[keep + 1];
        for (var i = 0; i < keep; i++)
        {
            final[i] = __instance.Locations[i];
        }

        final[keep] = tactical;
        __instance.Locations = new(final);
    }

    [HarmonyPatch(nameof(SpawnInMinigame.Close))]
    [HarmonyPostfix]
    public static void ClosePostfix()
    {
        var modifier = PlayerControl.LocalPlayer?.GetModifier<TacticalInsertionModifier>();
        if (modifier == null)
        {
            return;
        }

        modifier.ClearMark();
        modifier.UsedThisRound = false;
    }
}
