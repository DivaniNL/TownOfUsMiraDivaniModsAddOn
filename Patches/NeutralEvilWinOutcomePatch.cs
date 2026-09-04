using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Roles;
using DivaniMods.Roles.Neutral.NeutralEvil;
using DivaniMods.Utilities;

namespace DivaniMods.Patches;

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
}
