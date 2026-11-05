using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using DivaniMods.Modifiers.Game.Universal;
using DivaniMods.Options;
using TownOfUs.Modifiers.Game.Crewmate;

namespace DivaniMods.Patches;

[HarmonyPatch(typeof(BaitModifier), nameof(BaitModifier.IsModifierValidOn))]
internal static class MementoBaitExclusionPatch
{
    private static void Postfix(RoleBehaviour role, ref bool __result)
    {
        if (__result && OptionGroupSingleton<MementoOptions>.Instance.PreventBaitPairing &&
            role.Player.HasModifier<MementoModifier>())
        {
            __result = false;
        }
    }
}
