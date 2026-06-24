using System.Collections.Generic;
using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using DivaniMods.Buttons.Modifiers;
using DivaniMods.Modifiers.Game.Universal;
using DivaniMods.Options;
using TownOfUs.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DivaniMods.Patches;

[HarmonyPatch]
public static class UAVMapRevealPatch
{
    private static readonly Dictionary<byte, SpriteRenderer> Dots = new();
    private static float _nextSweep;

    [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.FixedUpdate))]
    [HarmonyPostfix]
    public static void FixedUpdatePostfix(MapBehaviour __instance)
    {
        var local = PlayerControl.LocalPlayer;
        var hasVision = local != null && !local.HasDied() &&
                        (local.HasModifier<UAVActiveModifier>() || FriendlyUavActive(local));
        var active = hasVision && __instance.isActiveAndEnabled && ShipStatus.Instance != null && !MeetingHud.Instance;

        if (!active)
        {
            Clear();
            return;
        }

        var opts = OptionGroupSingleton<UAVOptions>.Instance;

        if (opts.Sweeping)
        {
            if (Time.time < _nextSweep)
            {
                return;
            }

            _nextSweep = Time.time + opts.SweepInterval;
        }

        var showColors = opts.ShowPlayerColors;
        var scale = ShipStatus.Instance!.MapScale;
        var signX = Mathf.Sign(ShipStatus.Instance.transform.localScale.x);

        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player == null || player.PlayerId == local!.PlayerId)
            {
                continue;
            }

            if (player.HasDied())
            {
                if (Dots.TryGetValue(player.PlayerId, out var hidden) && hidden)
                {
                    hidden.gameObject.SetActive(false);
                }

                continue;
            }

            if (!Dots.TryGetValue(player.PlayerId, out var dot) || dot == null)
            {
                dot = Object.Instantiate(__instance.HerePoint, __instance.HerePoint.transform.parent);
                dot.name = $"UAV Dot {player.PlayerId}";
                Dots[player.PlayerId] = dot;
            }

            dot.gameObject.SetActive(true);
            dot.enabled = true;

            var pos = player.transform.position / scale;
            pos.x *= signX;
            pos.z = -2.1f;
            dot.transform.localPosition = pos;

            if (showColors)
            {
                player.SetPlayerMaterialColors(dot);
            }
            else
            {
                PlayerMaterial.SetColors(Color.grey, dot);
            }
        }
    }

    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Begin))]
    [HarmonyPostfix]
    public static void ShipBeginPostfix()
    {
        Clear();
    }

    private static bool FriendlyUavActive(PlayerControl local)
    {
        if (!OptionGroupSingleton<UAVOptions>.Instance.FriendliesShareVision)
        {
            return false;
        }

        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player == null || player.PlayerId == local.PlayerId || player.HasDied())
            {
                continue;
            }

            if (player.HasModifier<UAVActiveModifier>() && UAVButton.AreFriendly(player, local))
            {
                return true;
            }
        }

        return false;
    }

    private static void Clear()
    {
        foreach (var dot in Dots.Values)
        {
            if (dot)
            {
                Object.Destroy(dot.gameObject);
            }
        }

        Dots.Clear();
        _nextSweep = 0f;
    }
}
