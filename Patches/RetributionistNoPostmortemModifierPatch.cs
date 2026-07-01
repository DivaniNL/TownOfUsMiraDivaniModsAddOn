using System.Reflection;
using HarmonyLib;
using DivaniMods.Modifiers.Game.Crewmate;
using DivaniMods.Roles.Crewmate.CrewmateKilling;
using TownOfUs.Modifiers.Game;

namespace DivaniMods.Patches;

[HarmonyPatch(typeof(TouGameModifier), nameof(TouGameModifier.IsModifierValidOn))]
public static class RetributionistNoPostmortemModifierPatch
{
    private static readonly PropertyInfo? FactionProperty = typeof(TouGameModifier).GetProperty("FactionType");

    [HarmonyPrefix]
    public static bool Prefix(TouGameModifier __instance, RoleBehaviour role, ref bool __result)
    {
        if (role is not RetributionistRole)
        {
            return true;
        }

        if (__instance is BearTrapModifier or BloodyModifier)
        {
            __result = false;
            return false;
        }

        string? faction;
        try
        {
            faction = FactionProperty?.GetValue(__instance)?.ToString();
        }
        catch
        {
            return true;
        }

        if (faction != null && faction.Contains("Postmortem"))
        {
            __result = false;
            return false;
        }

        return true;
    }
}
