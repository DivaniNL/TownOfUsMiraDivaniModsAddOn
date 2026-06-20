using System;
using System.Linq;
using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using DivaniMods.Options;
using DivaniMods.Roles.Neutral.NeutralEvil;
using TownOfUs.Options;
using TownOfUs.Patches;
using TownOfUs.Utilities;

namespace DivaniMods.Patches;

[HarmonyPatch(typeof(HudManagerPatches), nameof(HudManagerPatches.UpdateRoleNameText))]
public static class DemolitionistNameCounterPatch
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

        var demo = PlayerControl.AllPlayerControls.ToArray()
            .FirstOrDefault(p => p != null && p.Data?.Role is DemolitionistRole);
        if (demo == null)
        {
            return;
        }

        var genOpt = OptionGroupSingleton<GeneralOptions>.Instance;
        if (!demo.AmOwner && !(local.DiedOtherRound() && genOpt.TheDeadKnow))
        {
            return;
        }

        var nameText = demo.cosmetics?.nameText;
        if (nameText == null)
        {
            return;
        }

        var role = (DemolitionistRole)demo.Data.Role;
        var needed = (int)OptionGroupSingleton<DemolitionistOptions>.Instance.SabotagesToWin.Value;
        var capped = Math.Min(DemolitionistSabotageState.SuccessfulSabotages, needed);
        var counter =
            $"<size=80%>{role.RoleColor.ToTextColor()}({capped}/{needed})</color></size>";

        var text = nameText.text;
        var taskStr = $"<size=80%>{demo.TaskInfo()}</size>";
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
