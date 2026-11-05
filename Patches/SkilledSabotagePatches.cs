using HarmonyLib;
using MiraAPI.Modifiers;
using DivaniMods.Modifiers.Game.Crewmate;

namespace DivaniMods.Patches;
[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.RpcUpdateSystem), typeof(SystemTypes), typeof(byte))]
public static class SkilledSabotagePatches
{
    private static bool _inSkilledFix;

    [HarmonyPostfix]
    public static void Postfix(ShipStatus __instance, SystemTypes systemType, byte amount)
    {
        if (_inSkilledFix) return;

        var local = PlayerControl.LocalPlayer;
        if (local == null || !local.HasModifier<SkilledModifier>()) return;

        _inSkilledFix = true;
        try
        {
            switch (systemType)
            {
                case SystemTypes.Reactor:
                case SystemTypes.Laboratory:
                    if ((amount & 64) == 0) return;
                    __instance.RpcUpdateSystem(systemType, 16);
                    break;

                case SystemTypes.LifeSupp:
                    if ((amount & 64) == 0) return;
                    __instance.RpcUpdateSystem(SystemTypes.LifeSupp, 16);
                    break;

                case SystemTypes.Comms:
                    var commsSystem = ShipStatus.Instance.Systems[SystemTypes.Comms];
                    if (commsSystem == null || commsSystem.TryCast<HqHudSystemType>() == null) return;
                    if ((amount & 16) == 0) return;
                    __instance.RpcUpdateSystem(SystemTypes.Comms, 16 | 0);
                    __instance.RpcUpdateSystem(SystemTypes.Comms, 16 | 1);
                    break;

                case SystemTypes.HeliSabotage:
                    if ((amount & 16) == 0) return;
                    __instance.RpcUpdateSystem(SystemTypes.HeliSabotage, 16 | 0);
                    __instance.RpcUpdateSystem(SystemTypes.HeliSabotage, 16 | 1);
                    break;
            }
        }
        finally
        {
            _inSkilledFix = false;
        }
    }
}
