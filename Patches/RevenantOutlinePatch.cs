using HarmonyLib;
using MiraAPI.Utilities;
using Reactor.Utilities.Extensions;
using DivaniMods.Roles.Impostor.ImpostorAfterlife;
using UnityEngine;

namespace DivaniMods.Patches;

[HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
internal static class RevenantOutlinePatch
{
    [HarmonyPostfix]
    public static void Postfix()
    {
        if (RevenantRole.ActiveCount == 0)
        {
            return;
        }

        if (PlayerControl.AllPlayerControls == null)
        {
            return;
        }

        foreach (var pc in PlayerControl.AllPlayerControls)
        {
            if (pc?.Data?.Role is not RevenantRole rev)
            {
                continue;
            }

            if (!string.IsNullOrEmpty(pc.Data.PlayerName))
            {
                DivaniMods.Events.Impostor.ImpostorPower.SummonerState.RevenantNames.Add(pc.Data.PlayerName);
            }

            var body = pc.cosmetics?.currentBodySprite?.BodySprite;
            if (body == null)
            {
                continue;
            }

            if (!rev.GhostActive)
            {
                body.SetOutline(null);
                continue;
            }

            body.SetOutline(RevenantRole.RevenantColor);

            var c = body.color;
            body.color = new Color(RevenantRole.RevenantColor.r, RevenantRole.RevenantColor.g,
                RevenantRole.RevenantColor.b, c.a);

            var mat = body.material;
            if (mat != null && mat.HasProperty("_OutlineWidth"))
            {
                mat.SetFloat("_OutlineWidth", 0.2f);
            }
        }
    }
}
