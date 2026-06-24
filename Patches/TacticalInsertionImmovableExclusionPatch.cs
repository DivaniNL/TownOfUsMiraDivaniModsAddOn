using HarmonyLib;
using MiraAPI.Modifiers;
using DivaniMods.Modifiers.Game.Universal;
using TownOfUs.Modifiers.Game.Universal;

namespace DivaniMods.Patches;

[HarmonyPatch(typeof(ImmovableModifier), nameof(ImmovableModifier.IsModifierValidOn))]
internal static class TacticalInsertionImmovableExclusionPatch
{
    private static void Postfix(RoleBehaviour role, ref bool __result)
    {
        if (__result && role.Player.HasModifier<TacticalInsertionModifier>())
        {
            __result = false;
        }
    }
}
