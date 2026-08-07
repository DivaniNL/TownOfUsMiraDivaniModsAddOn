using DivaniMods.Roles.Impostor.ImpostorPower;
using HarmonyLib;
using MiraAPI.Roles;

namespace DivaniMods.Patches;

[HarmonyPatch(typeof(MainMenuManager))]
public static class GameStartupPatch
{
    private static bool _runOnce;

    [HarmonyPostfix]
    [HarmonyPatch(nameof(MainMenuManager.Start))]
    [HarmonyAfter(nameof(MiraAPI.Patches.Roles.GameStartupPatch))]
    public static void StartPostfix()
    {
        TownOfUs.Modules.DraftMode.DraftExclusiveImpostorRoles.Register(CustomRoleSingleton<RecruiterRole>.Instance);
    }
}