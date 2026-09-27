using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MiraAPI.Translation;
using DivaniMods.Roles.Impostor.ImpostorAfterlife;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DivaniMods.Patches;

[HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.Start))]
internal static class RevenantEndScreenPatch
{
    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last)]
    public static void Postfix()
    {
        var names = DivaniMods.Events.Impostor.ImpostorPower.SummonerState.RevenantNames;
        if (names.Count == 0)
        {
            return;
        }

        var hex = ColorUtility.ToHtmlStringRGBA(RevenantRole.RevenantColor);
        var revenantName = MiraLocaleManager.Get(
            "DivaniMods.Role.Revenant");

        foreach (var pp in Object.FindObjectsOfType<PoolablePlayer>())
        {
            var txt = pp.cosmetics?.nameText;
            if (txt == null)
            {
                continue;
            }

            var name = names.FirstOrDefault(n => txt.text.Contains(n));
            if (name == null)
            {
                continue;
            }

            pp.SetName(
               $"\n<size=85%>{name}</size>\n<size=65%><color=#{hex}>{revenantName}</color></size>",
                new Vector3(1.1619f, 1.1619f, 1f),
                Color.white,
                -15f);
            pp.SetNamePosition(new Vector3(0f, -1.31f, -0.5f));
        }
    }
}
