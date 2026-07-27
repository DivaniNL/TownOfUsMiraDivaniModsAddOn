using System.Linq;
using MiraAPI.GameEnd;
using MiraAPI.Modifiers;
using DivaniMods.Roles.Neutral.NeutralEvil;
using TownOfUs.GameOver;
using TownOfUs.Interfaces;
using TownOfUs.Modifiers.Game.Alliance;

namespace DivaniMods.Patches.WinConditions;

/// <summary>
///     When an Innocent who is also a Lover wins (their taunted killer is voted out next meeting),
///     both Lovers win together via the Lovers game over, mirroring the Jester/Executioner/Doomsayer + Lover behaviour.
/// </summary>
public sealed class InnocentLoverWinCondition : IWinCondition
{
    /// <summary>
    ///     Priority 4 - runs before NeutralRoleWinCondition (5) so an Innocent+Lover win is
    ///     redirected to a Lovers win instead of a solo Neutral win.
    /// </summary>
    public int Priority => 4;

    public bool IsMet(LogicGameFlowNormal gameFlow)
    {
        return TryGetWinningInnocentLover(out _, out _);
    }

    public void TriggerGameOver(LogicGameFlowNormal gameFlow)
    {
        if (!TryGetWinningInnocentLover(out var innocent, out var partner))
        {
            return;
        }

        var winners = new[] { innocent.Player.Data, partner.Data }
            .Distinct()
            .ToArray();

        CustomGameOver.Trigger<LoverGameOver>(winners);
    }

    private static bool TryGetWinningInnocentLover(out InnocentRole innocent, out PlayerControl partner)
    {
        innocent = null!;
        partner = null!;

        foreach (var candidate in InnocentRole.ActiveInnocents.Values)
        {
            if (candidate.Player == null || !candidate.WinConditionMet())
            {
                continue;
            }

            if (!candidate.Player.TryGetModifier<LoverModifier>(out var lover) || lover.OtherLover == null ||
                !lover.OtherLover.HasModifier<LoverModifier>())
            {
                continue;
            }

            innocent = candidate;
            partner = lover.OtherLover;
            return true;
        }

        return false;
    }
}
