using HarmonyLib;
using DivaniMods.Roles.Impostor.ImpostorAfterlife;

namespace DivaniMods.Patches;

[HarmonyPatch(typeof(SabotageButton), nameof(SabotageButton.DoClick))]
internal static class RevenantSabotagePatch
{
    [HarmonyPrefix]
    public static bool Prefix()
    {
        return PlayerControl.LocalPlayer?.Data?.Role is not RevenantRole;
    }
}
