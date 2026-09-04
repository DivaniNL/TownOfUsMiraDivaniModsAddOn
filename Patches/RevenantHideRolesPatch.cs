using HarmonyLib;
using MiraAPI.GameOptions;
using DivaniMods.Options;
using DivaniMods.Roles.Impostor.ImpostorAfterlife;
using TownOfUs.Utilities;

namespace DivaniMods.Patches;

[HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
internal static class RevenantHideRolesPatch
{
    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last)]
    public static void Postfix()
    {
        if (PlayerControl.LocalPlayer?.Data?.Role is not RevenantRole)
        {
            return;
        }

        if (OptionGroupSingleton<SummonerOptions>.Instance.RevenantSeesRoles.Value)
        {
            return;
        }

        foreach (var pc in PlayerControl.AllPlayerControls)
        {
            if (!ShouldHide(pc))
            {
                continue;
            }

            var nameText = pc.cosmetics?.nameText;
            if (nameText != null && nameText.text != pc.Data.PlayerName)
            {
                nameText.text = pc.Data.PlayerName;
            }
        }

        if (MeetingHud.Instance != null)
        {
            foreach (var pva in MeetingHud.Instance.playerStates)
            {
                if (pva?.NameText == null)
                {
                    continue;
                }

                var target = GameData.Instance?.GetPlayerById(pva.PlayerId)?.Object;
                if (ShouldHide(target) && pva.NameText.text != target!.Data.PlayerName)
                {
                    pva.NameText.text = target.Data.PlayerName;
                }
            }
        }
    }

    private static bool ShouldHide(PlayerControl? pc)
    {
        return pc?.Data != null && !pc.HasDied() && !pc.IsImpostorAligned();
    }
}
