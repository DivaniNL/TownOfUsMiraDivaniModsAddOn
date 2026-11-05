using HarmonyLib;
using DivaniMods.Roles.Impostor.ImpostorAfterlife;
using TownOfUs.Patches.Options;

namespace DivaniMods.Patches;

[HarmonyPatch(typeof(TeamChatPatches), nameof(TeamChatPatches.RpcSendImpTeamChat))]
internal static class RevenantImpChatPatch
{
    [HarmonyPrefix]
    public static bool Prefix()
    {
        return PlayerControl.LocalPlayer?.Data?.Role is not RevenantRole;
    }
}
