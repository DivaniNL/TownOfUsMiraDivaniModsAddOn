using HarmonyLib;
using DivaniMods.Roles.Impostor.ImpostorAfterlife;

namespace DivaniMods.Patches;

[HarmonyPatch(typeof(VentButton), nameof(VentButton.DoClick))]
internal static class RevenantVentBlockPatch
{
    [HarmonyPrefix]
    public static bool Prefix()
    {
        return PlayerControl.LocalPlayer?.Data?.Role is not RevenantRole;
    }
}
