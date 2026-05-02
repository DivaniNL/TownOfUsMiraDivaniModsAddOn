using System.Reflection;
using AmongUs.GameOptions;
using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using DivaniMods.Modifiers;
using TownOfUs.Buttons.Neutral;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Options.Roles.Neutral;

namespace DivaniMods.Patches;

[HarmonyPatch]
public static class SniperDistancePatch
{
    private static MethodBase TargetMethod()
    {
        return AccessTools.PropertyGetter(typeof(CustomActionButton<PlayerControl>), nameof(CustomActionButton<PlayerControl>.Distance));
    }

    public static void Postfix(ref float __result)
    {
        if (!SniperModifier.LocalPlayerHasSniper())
        {
            return;
        }

        __result = SniperModifier.ApplyRangeMultiplier(__result);
    }
}

[HarmonyPatch]
public static class SniperArsonistIgniteRadiusPatch
{
    private static MethodBase TargetMethod()
    {
        return AccessTools.PropertyGetter(typeof(ArsonistIgniteButton), "PlayersInRange");
    }

    public static void Postfix(ref List<PlayerControl> __result)
    {
        if (!SniperModifier.LocalPlayerHasSniper() ||
            OptionGroupSingleton<ArsonistOptions>.Instance.LegacyArsonist ||
            ShipStatus.Instance == null)
        {
            return;
        }

        var baseRadius = OptionGroupSingleton<ArsonistOptions>.Instance.IgniteRadius.Value *
            ShipStatus.Instance.MaxLightRadius;
        __result = Helpers.GetClosestPlayers(PlayerControl.LocalPlayer, SniperModifier.ApplyRangeMultiplier(baseRadius));
    }
}

[HarmonyPatch(typeof(ArsonistIgniteButton), "FixedUpdate")]
public static class SniperLegacyArsonistIgnitePatch
{
    public static void Postfix(ArsonistIgniteButton __instance)
    {
        if (!SniperModifier.LocalPlayerHasSniper() ||
            MeetingHud.Instance ||
            !OptionGroupSingleton<ArsonistOptions>.Instance.LegacyArsonist)
        {
            return;
        }

        var killDistances =
            GameOptionsManager.Instance.currentNormalGameOptions.GetFloatArray(FloatArrayOptionNames.KillDistances);
        var baseDistance = killDistances[GameOptionsManager.Instance.currentNormalGameOptions.KillDistance];
        __instance.ClosestTarget = Helpers.GetClosestPlayers(PlayerControl.LocalPlayer,
                SniperModifier.ApplyRangeMultiplier(baseDistance))
            .FirstOrDefault(x => x.HasModifier<ArsonistDousedModifier>());
    }
}
