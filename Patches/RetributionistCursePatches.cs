using HarmonyLib;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using DivaniMods.Modifiers.Crewmate.CrewmateKilling;
using TownOfUs.Buttons;
using TownOfUs.Buttons.Neutral;
using TownOfUs.Roles;
using TownOfUs.Utilities;

namespace DivaniMods.Patches;

[HarmonyPatch]
internal static class RetributionistCursePatches
{
    private static bool LocalCursed()
    {
        var local = PlayerControl.LocalPlayer;
        return local != null && local.HasModifier<CursedModifier>();
    }

    [HarmonyPatch(typeof(Vent), nameof(Vent.SetButtons))]
    [HarmonyPrefix]
    public static bool VentSetButtonsPrefix()
    {
        return !LocalCursed();
    }

    [HarmonyPatch(typeof(InfectedOverlay), nameof(InfectedOverlay.CanUseDoors), MethodType.Getter)]
    [HarmonyPostfix]
    public static void CanUseDoorsPostfix(ref bool __result)
    {
        if (LocalCursed())
        {
            __result = false;
        }
    }

    [HarmonyPatch(typeof(CustomActionButton), nameof(CustomActionButton.CanUse))]
    [HarmonyPostfix]
    public static void CanUsePostfix(CustomActionButton __instance, ref bool __result)
    {
        if (__result && ShouldCurseDisable(__instance))
        {
            __result = false;
        }
    }

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    [HarmonyPostfix]
    public static void ForceDisableButtonsPostfix()
    {
        var local = PlayerControl.LocalPlayer;
        if (local == null || local.gameObject == null || local.Data?.Role == null
            || !local.HasModifier<CursedModifier>())
        {
            return;
        }

        foreach (var button in CustomButtonManager.Buttons)
        {
            if (button.Enabled(local.Data.Role) && ShouldCurseDisable(button))
            {
                button.Button?.SetDisabled();
            }
        }
    }

    // A cursed killer cannot use ability buttons that would let it slip the hunt: every
    // Impostor Concealing ability, plus the Glitch's mimic. The kill button is always allowed.
    internal static bool ShouldCurseDisable(CustomActionButton button)
    {
        if (button is IKillButton)
        {
            return false;
        }

        var local = PlayerControl.LocalPlayer;
        if (local == null || !local.HasModifier<CursedModifier>())
        {
            return false;
        }

        return local.Is(RoleAlignment.ImpostorConcealing) || button is GlitchMimicButton;
    }
}
