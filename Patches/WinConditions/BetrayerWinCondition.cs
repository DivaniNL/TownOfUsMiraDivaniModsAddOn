using System.Linq;
using MiraAPI.GameEnd;
using MiraAPI.Modifiers;
using DivaniMods.GameOver;
using DivaniMods.Modifiers.Game.Alliance;
using TownOfUs.Interfaces;
using TownOfUs.Utilities;

namespace DivaniMods.Patches.WinConditions;

/// <summary>
///     Win condition for the Betrayer alliance modifier.
///     The Betrayer wins like a Neutral Killer: last killer alive with kill majority.
/// </summary>
public sealed class BetrayerWinCondition : IWinCondition
{
    /// <summary>
    ///     Priority 7 - after neutral roles (5), before lovers (10).
    /// </summary>
    public int Priority => 7;

    public bool IsMet(LogicGameFlowNormal gameFlow)
    {
        return BetrayerModifier.WinConditionMet();
    }

    public void TriggerGameOver(LogicGameFlowNormal gameFlow)
    {
        var winners = ModifierUtils.GetActiveModifiers<BetrayerModifier>()
            .Where(x => x.Player != null && !x.Player.HasDied() && x.Player.Data != null)
            .Select(x => x.Player.Data)
            .Distinct()
            .ToArray();

        if (winners.Length == 0)
        {
            return;
        }

        CustomGameOver.Trigger<BetrayerGameOver>(winners);
    }
}
