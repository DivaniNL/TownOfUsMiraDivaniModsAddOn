using HarmonyLib;
using DivaniMods.Events.Impostor.ImpostorPassive;
using TownOfUs.Events.Crewmate;

namespace DivaniMods.Patches;

[HarmonyPatch(typeof(MedicEvents), "CheckForMedicShield")]
public static class RuthlessMedicShieldPatch
{
    public static bool Prefix(PlayerControl source, PlayerControl target, ref bool __result)
    {
        if (!RuthlessEvents.CanPierceShields(source, target))
        {
            return true;
        }

        __result = false;
        return false;
    }
}

[HarmonyPatch(typeof(ClericEvents), "CheckForClericBarrier")]
public static class RuthlessClericBarrierPatch
{
    public static bool Prefix(PlayerControl source, PlayerControl target, ref bool __result)
    {
        if (!RuthlessEvents.CanPierceShields(source, target))
        {
            return true;
        }

        __result = false;
        return false;
    }
}

[HarmonyPatch(typeof(WardenEvents), "CheckForWardenFortify")]
public static class RuthlessWardenFortifyPatch
{
    public static bool Prefix(PlayerControl source, PlayerControl target)
    {
        return !RuthlessEvents.CanPierceShields(source, target);
    }
}
