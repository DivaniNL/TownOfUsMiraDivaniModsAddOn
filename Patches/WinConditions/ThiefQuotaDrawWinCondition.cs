using System.Linq;
using MiraAPI.GameEnd;
using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;
using DivaniMods.Roles.Neutral.NeutralKilling;
using TownOfUs.GameOver;
using TownOfUs.Interfaces;

namespace DivaniMods.Patches.WinConditions;

/// <summary>
///     Ends the game in a draw when the last surviving killer is a Thief that stole a
///     Deadly Quota it never completed. Without this the game cannot end: the Thief is
///     blocked from winning by the quota, but a living neutral killer keeps the vanilla
///     end criteria from firing.
/// </summary>
public sealed class ThiefQuotaDrawWinCondition : IWinCondition
{
    /// <summary>
    ///     Priority 4 - executes before neutral roles (5).
    /// </summary>
    public int Priority => 4;

    public bool IsMet(LogicGameFlowNormal gameFlow)
    {
        return GetStalledThief() != null;
    }

    public void TriggerGameOver(LogicGameFlowNormal gameFlow)
    {
        if (GetStalledThief() == null)
        {
            return;
        }

        var drawReason = CustomGameOver.GameOverReason<DrawGameOver>();
        var loser = PlayerControl.AllPlayerControls.ToArray()
            .FirstOrDefault(x => x != null && x.Data != null &&
                                 !x.Data.Role.DidWin(drawReason) &&
                                 !x.GetModifiers<GameModifier>().Any(y => y.DidWin(drawReason) == true));

        CustomGameOver.Trigger<DrawGameOver>([
            loser != null ? loser.Data : PlayerControl.LocalPlayer.Data
        ]);
    }

    private static ThiefRole? GetStalledThief()
    {
        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player == null || player.Data == null)
            {
                continue;
            }

            if (player.Data.Role is not ThiefRole thief)
            {
                continue;
            }

            if (thief.ParityWinMet() && thief.HasUnmetDeadlyQuota())
            {
                return thief;
            }
        }

        return null;
    }
}
