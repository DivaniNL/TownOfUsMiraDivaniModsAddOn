using System;
using System.Linq;
using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using DivaniMods.Options;
using DivaniMods.Roles.Neutral.NeutralOutlier;
using TownOfUs.Options;
using TownOfUs.Patches;
using TownOfUs.Utilities;

namespace DivaniMods.Patches;

[HarmonyPatch(typeof(HudManagerPatches), nameof(HudManagerPatches.UpdateRoleNameText))]
public static class OpportunistNameCounterPatch
{
    [HarmonyPostfix]
    public static void Postfix()
    {
        if (MeetingHud.Instance)
        {
            return;
        }

        var local = PlayerControl.LocalPlayer;
        if (local == null || local.Data == null)
        {
            return;
        }

        var opp = PlayerControl.AllPlayerControls.ToArray()
            .FirstOrDefault(p => p != null && p.Data?.Role is OpportunistRole);
        if (opp == null)
        {
            return;
        }

        var genOpt = OptionGroupSingleton<GeneralOptions>.Instance;
        if (!opp.AmOwner && !(local.DiedOtherRound() && genOpt.TheDeadKnow))
        {
            return;
        }

        var nameText = opp.cosmetics?.nameText;
        if (nameText == null)
        {
            return;
        }

        var role = (OpportunistRole)opp.Data.Role;
        var needed = (int)OptionGroupSingleton<OpportunistOptions>.Instance.VotesNeeded.Value;
        var capped = Math.Min(role.VotesCollected, needed);
        var counter =
            $"<size=80%>{role.RoleColor.ToTextColor()}({capped}/{needed})</color></size>";

        var text = nameText.text;
        var taskStr = $"<size=80%>{opp.TaskInfo()}</size>";
        var idx = text.IndexOf(taskStr);
        if (idx >= 0)
        {
            nameText.text = text.Insert(idx, counter + " ");
        }
        else
        {
            var newline = text.IndexOf('\n');
            nameText.text = newline >= 0 ? text.Insert(newline, " " + counter) : text + " " + counter;
        }
    }
}
