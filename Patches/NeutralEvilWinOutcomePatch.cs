using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Hud;
using DivaniMods.Buttons.Neutral;
using MiraAPI.Roles;
using DivaniMods.Roles.Neutral.NeutralEvil;
using DivaniMods.Utilities;

namespace DivaniMods.Patches;

[HarmonyPatch]
public static class NeutralEvilWinOutcomePatch
{
    [RegisterEvent]
    public static void OnRoundStart(RoundStartEvent evt)
    {
        if (evt.TriggeredByIntro)
        {
            return;
        }

        foreach (var demo in CustomRoleUtils.GetActiveRolesOfType<DemolitionistRole>())
        {
            NeutralEvilWinOutcomeUtils.TryResolveQuietWin(demo, demo);
        }

        foreach (var innocent in InnocentRole.ActiveInnocents.Values)
        {
            NeutralEvilWinOutcomeUtils.TryResolveQuietWin(innocent, innocent);
        }

        foreach (var opp in OpportunistRole.ActiveOpportunists.Values)
        {
            NeutralEvilWinOutcomeUtils.TryResolveQuietWin(opp, opp);
        }

        foreach (var pd in CustomRoleUtils.GetActiveRolesOfType<PlagueDoctorRole>())
        {
            NeutralEvilWinOutcomeUtils.TryResolveQuietWin(pd, pd);
        }
    }

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
    [HarmonyPostfix]
    public static void ResetTormentButtonOnGameEnd()
    {
        ResetTormentButton();
    }

    [HarmonyPatch(typeof(LobbyBehaviour), nameof(LobbyBehaviour.Start))]
    [HarmonyPostfix]
    public static void ResetTormentButtonOnLobbyStart()
    {
        ResetTormentButton();
    }

    private static void ResetTormentButton()
    {
        var button = CustomButtonSingleton<NeutralEvilTormentButton>.Instance;
        if (button == null)
        {
            return;
        }

        button.Show = false;
        button.Owner = null;
    }
}
