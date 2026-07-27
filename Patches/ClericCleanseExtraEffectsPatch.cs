using HarmonyLib;
using MiraAPI.GameOptions;
using Reactor.Networking.Attributes;
using DivaniMods.Assets;
using DivaniMods.Options;
using DivaniMods.Roles.Neutral.NeutralKilling;
using DivaniMods.Utilities;
using TownOfUs;
using TownOfUs.Buttons.Crewmate;
using UnityEngine;

namespace DivaniMods.Patches;

public static class ClericCleanseExtraEffectsPatch
{
    [HarmonyPatch(typeof(ClericCleanseButton), "OnClick")]
    private static class OnClickPatch
    {
        private static void Postfix(ClericCleanseButton __instance)
        {
            if (__instance.Target == null)
            {
                return;
            }

            DivaniNegativeEffects.CleanseAll(__instance.Target);
            TryDefuseFrag(__instance.Target);
        }
    }

    private static void TryDefuseFrag(PlayerControl target)
    {
        if (!OptionGroupSingleton<FragOptions>.Instance.ClericCanDefuse.Value) return;
        if (!FragBombState.IsHolder(target.PlayerId)) return;

        var local = PlayerControl.LocalPlayer;
        if (local == null) return;

        RpcDefuseFrag(local, target.PlayerId);
    }

    [MethodRpc((uint)DivaniRpcCalls.FragDefused)]
    public static void RpcDefuseFrag(PlayerControl cleric, byte holderId)
    {
        if (!FragBombState.IsHolder(holderId)) return;

        var fragId = FragBombState.FragId;
        FragBombState.Clear(startGiveCooldown: true);

        var local = PlayerControl.LocalPlayer;
        if (local == null) return;

        var clericHex = ColorUtility.ToHtmlStringRGB(TownOfUsColors.Cleric);
        var fragHex = ColorUtility.ToHtmlStringRGB(FragRole.FragColor);
        var icon = DivaniAssets.FragIcon.LoadAsset();

        if (local.PlayerId == holderId || local.PlayerId == fragId)
        {
            MiraAPI.Utilities.Helpers.CreateAndShowNotification(
                $"<b>Your Frag bomb got defused by a <color=#{clericHex}>Cleric</color></b>",
                Color.white,
                new Vector3(0f, 1f, -20f),
                spr: icon);
        }
        else if (cleric != null && local.PlayerId == cleric.PlayerId)
        {
            MiraAPI.Utilities.Helpers.CreateAndShowNotification(
                $"<b>You defused the <color=#{fragHex}>Frag</color> bomb!</b>",
                Color.white,
                new Vector3(0f, 1f, -20f),
                spr: icon);
        }
    }
}
