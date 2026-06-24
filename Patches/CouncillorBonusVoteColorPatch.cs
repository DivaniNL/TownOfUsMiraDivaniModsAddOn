using System.Collections.Generic;
using HarmonyLib;
using DivaniMods.Roles.Impostor.ImpostorSupport;
using TownOfUs.Utilities;
using UnityEngine;

namespace DivaniMods.Patches;

[HarmonyPatch(typeof(MeetingHud))]
public static class CouncillorBonusVoteColorPatch
{
    private static readonly Dictionary<byte, int> RenderedVotes = new();

    [HarmonyPatch(nameof(MeetingHud.VotingComplete))]
    [HarmonyPrefix]
    public static void VotingCompletePrefix()
    {
        RenderedVotes.Clear();
    }

    [HarmonyPatch(nameof(MeetingHud.BloopAVoteIcon))]
    [HarmonyPostfix]
    public static void BloopPostfix([HarmonyArgument(0)] NetworkedPlayerInfo voterPlayer,
        [HarmonyArgument(2)] Transform parent)
    {
        var voter = MiscUtils.PlayerById(voterPlayer.PlayerId);
        if (voter == null || voter.Data?.Role is not CouncillorRole)
        {
            return;
        }

        var count = RenderedVotes.TryGetValue(voterPlayer.PlayerId, out var c) ? c : 0;
        RenderedVotes[voterPlayer.PlayerId] = count + 1;

        // Keep the first (original) vote in the player's colour. Grey out every bonus vote regardleess if camo votes are on.
        if (count == 0)
        {
            return;
        }

        var icon = FindLatestVoteIcon(parent);
        if (icon != null)
        {
            PlayerMaterial.SetColors(Palette.DisabledGrey, icon);
        }
    }

    private static SpriteRenderer? FindLatestVoteIcon(Transform parent)
    {
        SpriteRenderer? latest = null;
        for (var i = 0; i < parent.childCount; i++)
        {
            var child = parent.GetChild(i);
            if (child.name != "playerVote(Clone)")
            {
                continue;
            }

            var rend = child.GetComponent<SpriteRenderer>();
            if (rend != null)
            {
                latest = rend;
            }
        }

        return latest;
    }
}
