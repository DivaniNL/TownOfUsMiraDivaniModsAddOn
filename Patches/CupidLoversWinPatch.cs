using System.Linq;
using HarmonyLib;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using DivaniMods.Roles.Neutral.NeutralBenign;
using TownOfUs.Modifiers.Game.Alliance;

namespace DivaniMods.Patches;

[HarmonyPatch(typeof(LoverModifier), nameof(LoverModifier.WinConditionMet))]
public static class CupidLoversWinPatch
{
    [HarmonyPostfix]
    public static void Postfix(LoverModifier[] lovers, ref bool __result)
    {
        if (__result || lovers == null || lovers.Length != 2)
        {
            return;
        }

        var cupidAlive = PlayerControl.AllPlayerControls.ToArray()
            .Any(p => p != null && p.Data != null && !p.Data.IsDead && p.Data.Role is CupidRole);
        if (!cupidAlive)
        {
            return;
        }

        var alive = Helpers.GetAlivePlayers();
        var bothLoversAlive = alive.Count(x => x.HasModifier<LoverModifier>()) >= 2;

        if (alive.Count <= 4 && bothLoversAlive)
        {
            __result = true;
        }
    }
}
