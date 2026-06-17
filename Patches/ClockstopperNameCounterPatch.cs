using System.Linq;
using HarmonyLib;
using MiraAPI.Utilities;
using DivaniMods.Events.Crewmate.CrewmateSupport;
using DivaniMods.Roles.Crewmate.CrewmateSupport;
using TownOfUs.Patches;
using TownOfUs.Utilities;

namespace DivaniMods.Patches;

[HarmonyPatch(typeof(HudManagerPatches), nameof(HudManagerPatches.UpdateRoleNameText))]
public static class ClockstopperNameCounterPatch
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

        var clock = PlayerControl.AllPlayerControls.ToArray()
            .FirstOrDefault(p => p != null && p.Data?.Role is ClockstopperRole);
        if (clock == null)
        {
            return;
        }

        if (!clock.AmOwner && !local.HasDied())
        {
            return;
        }

        var nameText = clock.cosmetics?.nameText;
        if (nameText == null)
        {
            return;
        }

        var role = (ClockstopperRole)clock.Data.Role;
        var counter =
            $"<size=80%>{role.RoleColor.ToTextColor()}({ClockstopperEvents.GetProgress(clock)}/{ClockstopperEvents.GetNeeded()})</color></size>";

        var text = nameText.text;
        var taskStr = $"<size=80%>{clock.TaskInfo()}</size>";
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
