using HarmonyLib;
using MiraAPI.Modifiers;
using DivaniMods.Modifiers.Game.Crewmate;

namespace DivaniMods.Patches;

[HarmonyPatch(typeof(Console), nameof(Console.CanUse))]
public static class IncompetentSabotagePatch
{
    public static bool Prefix(Console __instance, NetworkedPlayerInfo pc, ref bool canUse, ref bool couldUse, ref float __result)
    {
        if (__instance == null)
        {
            return true;
        }

        var player = pc?.Object;
        if (player == null || !player.AmOwner || !player.HasModifier<IncompetentModifier>())
        {
            return true;
        }

        if (!IsSabotageFixConsole(__instance))
        {
            return true;
        }

        canUse = false;
        couldUse = false;
        __result = float.MaxValue;
        return false;
    }

    private static bool IsSabotageFixConsole(Console console)
    {
        var types = console.TaskTypes;
        if (types == null)
        {
            return false;
        }

        for (var i = 0; i < types.Length; i++)
        {
            switch (types[i])
            {
                case TaskTypes.ResetReactor:
                case TaskTypes.FixLights:
                case TaskTypes.FixComms:
                case TaskTypes.RestoreOxy:
                case TaskTypes.StopCharles:
                case TaskTypes.MushroomMixupSabotage:
                    return true;
            }
        }

        return false;
    }
}
