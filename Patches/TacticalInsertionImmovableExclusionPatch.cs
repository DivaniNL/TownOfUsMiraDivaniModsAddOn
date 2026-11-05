using HarmonyLib;
using DivaniMods.Utilities;
using TownOfUs.Modifiers.Game.Universal;

namespace DivaniMods.Patches;

[HarmonyPatch(typeof(ImmovableModifier), nameof(ImmovableModifier.IsModifierValidOn))]
internal static class TacticalInsertionImmovableExclusionPatch
{
    private static void Postfix(ImmovableModifier __instance, RoleBehaviour role, ref bool __result)
    {
        if (__result && ModifierExclusions.ConflictsWithOwned(role.Player, __instance))
        {
            __result = false;
        }
    }
}
