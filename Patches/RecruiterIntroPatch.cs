using HarmonyLib;
using DivaniMods.Roles.Impostor.ImpostorPower;

namespace DivaniMods.Patches;

[HarmonyPatch]
internal static class RecruiterIntroPatch
{
    private const string RecruiterIntroText =
        "There are DEFINETELY <color=#FF1919>2 Impostors</color> Among Us";

    private static IEnumerable<System.Reflection.MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(typeof(IntroCutscene), "BeginCrewmate");
        yield return AccessTools.Method(typeof(IntroCutscene), "BeginImpostor");
    }

    [HarmonyPostfix]
    private static void Postfix(IntroCutscene __instance)
    {
        if (!PlayerControl.AllPlayerControls.ToArray()
                .Any(player => player?.Data?.Role is RecruiterRole))
        {
            return;
        }

        __instance.ImpostorText.text = RecruiterIntroText;
    }
}
