using System.Linq;
using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using DivaniMods.Events.Crewmate.CrewmateKilling;
using DivaniMods.Options;
using DivaniMods.Roles.Crewmate.CrewmateKilling;
using TownOfUs.Modules.Components;
using TownOfUs.Options;
using TownOfUs.Patches;
using TownOfUs.Utilities;

namespace DivaniMods.Patches;

[HarmonyPatch(typeof(HudManagerHelper), nameof(HudManagerHelper.UpdateRoleNameText))]
public static class RetributionistRevengeStatusPatch
{
    [HarmonyPostfix]
    public static void Postfix()
    {
        if (MeetingHud.Instance)
        {
            return;
        }

        if (!OptionGroupSingleton<RetributionistOptions>.Instance.TurnIntoSoulOnce)
        {
            return;
        }

        var local = PlayerControl.LocalPlayer;
        if (local == null || local.Data == null)
        {
            return;
        }

        var ret = PlayerControl.AllPlayerControls.ToArray()
            .FirstOrDefault(p => p != null && p.Data?.Role is RetributionistRole);
        if (ret == null)
        {
            return;
        }

        var genOpt = OptionGroupSingleton<GeneralOptions>.Instance;
        if (!ret.AmOwner && !(local.DiedOtherRound() && genOpt.TheDeadKnow))
        {
            return;
        }

        var nameText = ret.cosmetics?.nameText;
        if (nameText == null)
        {
            return;
        }

        var role = (RetributionistRole)ret.Data.Role;
        var available = !RetributionistManager.UsedRevenge.Contains(ret.PlayerId);
        var box = available ? "☐" : "✓";
        var prefix = $"<size=80%>{role.RoleColor.ToTextColor()}({box})</color></size>";

        var text = nameText.text;
        var taskStr = $"<size=80%>{ret.TaskInfo()}</size>";
        var idx = text.IndexOf(taskStr);
        if (idx >= 0)
        {
            nameText.text = text.Insert(idx, prefix + " ");
        }
        else
        {
            var newline = text.IndexOf('\n');
            nameText.text = newline >= 0 ? text.Insert(newline, " " + prefix) : text + " " + prefix;
        }
    }
}
