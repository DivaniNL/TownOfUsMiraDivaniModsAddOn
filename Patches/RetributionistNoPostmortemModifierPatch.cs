using HarmonyLib;
using DivaniMods.Roles.Crewmate.CrewmateKilling;
using TownOfUs.Modifiers.Game;

namespace DivaniMods.Patches;

[HarmonyPatch(typeof(TouGameModifier), nameof(TouGameModifier.IsModifierValidOn))]
public static class RetributionistNoPostmortemModifierPatch
{
    [HarmonyPrefix]
    public static bool Prefix(TouGameModifier __instance, RoleBehaviour role, ref bool __result)
    {
        if (role is not RetributionistRole)
        {
            return true;
        }

        if (__instance.FactionType.ToString().Contains("Postmortem"))
        {
            __result = false;
            return false;
        }

        return true;
    }
}
