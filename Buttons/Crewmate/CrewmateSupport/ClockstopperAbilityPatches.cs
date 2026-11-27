using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using DivaniMods.Options;
using TownOfUs.Buttons.Crewmate;
using TownOfUs.Roles.Crewmate;

namespace DivaniMods.Buttons.Crewmate.CrewmateSupport;

[HarmonyPatch(typeof(TimeLordRewindButton), "OnClick")]
public static class ClockstopperAbilityPatches
{
    [HarmonyPostfix]
    public static void SyncClockstopperCooldown()
    {
        if (PlayerControl.LocalPlayer?.Data?.Role is not TimeLordRole ||
            !OptionGroupSingleton<ClockstopperAbilityOptions>.Instance.Enabled)
        {
            return;
        }

        var clockstopperButton = CustomButtonSingleton<ClockstopperButton>.Instance;
        var rewindButton = CustomButtonSingleton<TimeLordRewindButton>.Instance;
        clockstopperButton.SetTimer(rewindButton.Cooldown + rewindButton.EffectDuration);
        ClockstopperButton.GrantRewindCharge();
    }
}
