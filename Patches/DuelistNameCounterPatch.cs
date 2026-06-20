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
using UnityEngine;

namespace DivaniMods.Patches;

[HarmonyPatch(typeof(HudManagerPatches), nameof(HudManagerPatches.UpdateRoleNameText))]
public static class DuelistNameCounterPatch
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

        var duel = PlayerControl.AllPlayerControls.ToArray()
            .FirstOrDefault(p => p != null && p.Data?.Role is DuelistRole);
        if (duel == null)
        {
            return;
        }

        var genOpt = OptionGroupSingleton<GeneralOptions>.Instance;
        if (!duel.AmOwner && !(local.DiedOtherRound() && genOpt.TheDeadKnow))
        {
            return;
        }

        var nameText = duel.cosmetics?.nameText;
        if (nameText == null)
        {
            return;
        }

        var role = (DuelistRole)duel.Data.Role;
        var winsNeeded = (int)OptionGroupSingleton<DuelistOptions>.Instance.DuelsToWin.Value;
        var lossesToDie = (int)OptionGroupSingleton<DuelistOptions>.Instance.DuelsLostToDie.Value;
        var wins = Math.Min(role.DuelWins, winsNeeded);
        var losses = Math.Min(role.DuelLosses, lossesToDie);
        var counter =
            $"<size=80%>{Color.green.ToTextColor()}({wins}/{winsNeeded})</color> {Color.red.ToTextColor()}({losses}/{lossesToDie})</color></size>";

        var text = nameText.text;
        var taskStr = $"<size=80%>{duel.TaskInfo()}</size>";
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
