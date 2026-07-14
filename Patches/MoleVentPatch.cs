using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using DivaniMods.Buttons.Crewmate.CrewmateSupport;
using DivaniMods.Options;
using DivaniMods.Roles.Crewmate.CrewmateSupport;
using UnityEngine;

namespace DivaniMods.Patches;

[HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
public static class MoleVentTimeLimitPatch
{
    [HarmonyPostfix]
    public static void Postfix()
    {
        var player = PlayerControl.LocalPlayer;
        if (player == null || player.Data == null || ShipStatus.Instance == null)
        {
            return;
        }

        if (MoleRole.IsNativeVenter(player.Data.Role))
        {
            return;
        }

        var limit = OptionGroupSingleton<MoleOptions>.Instance.VentTimeLimit.Value;
        var button = CustomButtonSingleton<MoleVentButton>.Instance;
        var vent = Vent.currentVent;

        if (!player.inVent || vent == null || !vent.name.StartsWith("MoleVent"))
        {
            MoleRole.VentTimeLeft = limit;

            if (button != null && button.EffectActive)
            {
                button.EffectActive = false;
                button.SetTimer(button.Cooldown);
            }

            return;
        }

        MoleRole.VentTimeLeft -= Time.deltaTime;

        if (button != null)
        {
            button.EffectActive = true;
            button.SetTimer(Mathf.Max(MoleRole.VentTimeLeft, 0f));
        }

        if (MoleRole.VentTimeLeft > 0f)
        {
            return;
        }

        MoleRole.VentTimeLeft = limit;

        vent.SetButtons(false);
        player.MyPhysics.RpcExitVent(vent.Id);

        if (button != null)
        {
            button.EffectActive = false;
            button.SetTimer(button.Cooldown);
        }
    }
}

[HarmonyPatch(typeof(Vent), nameof(Vent.CanUse))]
public static class MoleVentCanUsePatch
{
    [HarmonyPostfix]
    public static void Postfix(Vent __instance, ref float __result, [HarmonyArgument(0)] NetworkedPlayerInfo pc,
        [HarmonyArgument(1)] ref bool canUse, [HarmonyArgument(2)] ref bool couldUse)
    {
        if (!__instance.name.StartsWith("MoleVent"))
        {
            return;
        }

        var player = pc.Object;
        if (player == null)
        {
            return;
        }

        var hiddenFromPlayer = OptionGroupSingleton<MoleOptions>.Instance.VentVisibility == MoleVentVisibility.AfterUse &&
                               player.Data.Role is not MoleRole && !__instance.myRend.enabled;

        if (!MoleRole.CanUseMoleVents(player) || hiddenFromPlayer)
        {
            canUse = false;
            couldUse = false;
            __result = float.MaxValue;
            return;
        }

        if (couldUse)
        {
            return;
        }

        if (MoleRole.VentsDisabledByPlayerCount())
        {
            return;
        }

        couldUse = !pc.IsDead && (player.CanMove || player.inVent);
        canUse = couldUse;

        if (canUse)
        {
            var center = player.Collider.bounds.center;
            var position = __instance.transform.position;
            var distance = Vector2.Distance(center, position);
            __result = distance;
            canUse &= distance <= __instance.UsableDistance &&
                      !PhysicsHelpers.AnythingBetween(player.Collider, center, position, Constants.ShipOnlyMask,
                          false);
        }
    }
}

[HarmonyPatch(typeof(Vent), nameof(Vent.SetButtons))]
public static class MoleVentSetButtonsPatch
{
    [HarmonyPostfix]
    public static void Postfix(Vent __instance, [HarmonyArgument(0)] bool enabled)
    {
        if (OptionGroupSingleton<MoleOptions>.Instance.VentVisibility == MoleVentVisibility.Immediate)
        {
            return;
        }

        if (!PlayerControl.LocalPlayer)
        {
            return;
        }

        if (!PlayerControl.LocalPlayer.Data)
        {
            return;
        }

        if (!enabled)
        {
            return;
        }

        if (PlayerControl.LocalPlayer.Data.Role is MoleRole)
        {
            return;
        }

        if (!__instance.name.Contains("MoleVent"))
        {
            return;
        }

        Vent[] nearbyVents = __instance.NearbyVents;
        for (var i = 0; i < __instance.Buttons.Length; i++)
        {
            var buttonBehavior = __instance.Buttons[i];
            var vent = nearbyVents[i];

            if (vent != null && !vent.myRend.enabled)
            {
                buttonBehavior.gameObject.SetActive(false);
            }
        }
    }
}
