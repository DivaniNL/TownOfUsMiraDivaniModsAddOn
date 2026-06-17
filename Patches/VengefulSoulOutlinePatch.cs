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
    [HarmonyPostfix]
    public static void Postfix()
    {
        if (VengefulSoulRole.ActiveCount == 0 || PlayerControl.AllPlayerControls == null)
        {
            return;
        }

        foreach (var pc in PlayerControl.AllPlayerControls)
        {
            if (pc?.Data?.Role is not VengefulSoulRole soul)
            {
                continue;
            }

            var body = pc.cosmetics?.currentBodySprite?.BodySprite;
            if (body == null)
            {
                continue;
            }

            if (!soul.GhostActive)
            {
                body.SetOutline(null);
                continue;
            }

            if (!ShouldLocalSee(pc))
            {
                body.SetOutline(null);
                var hidden = body.color;
                body.color = new Color(hidden.r, hidden.g, hidden.b, 0f);
                continue;
            }

            body.SetOutline(Palette.CrewmateBlue);

            var c = body.color;
            body.color = new Color(Palette.CrewmateBlue.r, Palette.CrewmateBlue.g, Palette.CrewmateBlue.b, c.a);

            var mat = body.material;
            if (mat != null && mat.HasProperty("_OutlineWidth"))
            {
                mat.SetFloat("_OutlineWidth", 0.2f);
            }
        }
    }

    private static bool ShouldLocalSee(PlayerControl soul)
    {
        var local = PlayerControl.LocalPlayer;
        if (local == null || local.PlayerId == soul.PlayerId || local.HasDied())
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
