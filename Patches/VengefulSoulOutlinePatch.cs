using System.Collections.Generic;
using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using Reactor.Utilities.Extensions;
using DivaniMods.Events.Crewmate.CrewmateKilling;
using DivaniMods.Options;
using DivaniMods.Roles.Crewmate.CrewmateAfterlife;
using TownOfUs.Utilities;
using UnityEngine;

namespace DivaniMods.Patches;

[HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
internal static class VengefulSoulOutlinePatch
{
    private static readonly Dictionary<byte, float> HiddenAlphas = new();

    [HarmonyPostfix]
    public static void Postfix()
    {
        if (VengefulSoulRole.ActiveCount == 0)
        {
            HiddenAlphas.Clear();
            return;
        }

        if (PlayerControl.AllPlayerControls == null)
        {
            return;
        }

        var currentSouls = new HashSet<byte>();
        foreach (var pc in PlayerControl.AllPlayerControls)
        {
            if (pc?.Data?.Role is not VengefulSoulRole soul)
            {
                continue;
            }

            currentSouls.Add(pc.PlayerId);
            var body = pc.cosmetics?.currentBodySprite?.BodySprite;
            if (body == null)
            {
                continue;
            }

            if (!soul.GhostActive)
            {
                body.SetOutline(null);
                RestoreAlpha(pc.PlayerId, body);
                continue;
            }

            if (!ShouldLocalSee(pc))
            {
                body.SetOutline(null);
                var hidden = body.color;
                if (!HiddenAlphas.ContainsKey(pc.PlayerId) && hidden.a > 0f)
                {
                    HiddenAlphas[pc.PlayerId] = hidden.a;
                }

                body.color = new Color(hidden.r, hidden.g, hidden.b, 0f);
                continue;
            }

            var alpha = RestoreAlpha(pc.PlayerId, body);
            body.SetOutline(Palette.CrewmateBlue);

            var c = body.color;
            body.color = new Color(Palette.CrewmateBlue.r, Palette.CrewmateBlue.g, Palette.CrewmateBlue.b, alpha);

            var mat = body.material;
            if (mat != null && mat.HasProperty("_OutlineWidth"))
            {
                mat.SetFloat("_OutlineWidth", 0.2f);
            }
        }

        foreach (var playerId in new List<byte>(HiddenAlphas.Keys))
        {
            if (!currentSouls.Contains(playerId))
            {
                HiddenAlphas.Remove(playerId);
            }
        }
    }

    private static float RestoreAlpha(byte playerId, SpriteRenderer body)
    {
        var current = body.color;
        var alpha = HiddenAlphas.Remove(playerId, out var hiddenAlpha)
            ? hiddenAlpha
            : current.a > 0f ? current.a : 1f;

        body.color = new Color(current.r, current.g, current.b, alpha);
        return alpha;
    }

    private static bool ShouldLocalSee(PlayerControl soul)
    {
        var local = PlayerControl.LocalPlayer;
        if (local == null || local.PlayerId == soul.PlayerId || local.DiedOtherRound())
        {
            return true;
        }

        return OptionGroupSingleton<RetributionistOptions>.Instance.SoulVisibleTo switch
        {
            VengefulSoulVisibility.Evil => local.IsImpostorAligned() || local.IsNeutral(),
            VengefulSoulVisibility.Killer =>
                RetributionistManager.TryGetKiller(soul.PlayerId, out var killer) && killer != null &&
                killer.PlayerId == local.PlayerId,
            _ => true
        };
    }
}
