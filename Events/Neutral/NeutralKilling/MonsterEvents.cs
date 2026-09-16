using DivaniMods.Modules.Monster;
using DivaniMods.Roles.Neutral.NeutralKilling;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Modifiers;
using TownOfUs.Modifiers.Game.Assailant;
using TownOfUs.Modules;

namespace DivaniMods.Events.Neutral.NeutralKilling;

public static class MonsterEvents
{
    [RegisterEvent(100)]
    public static void AfterMurderHandler(AfterMurderEvent @event)
    {
        var source = @event.Source;
        if (source == null || source.Data?.Role is not MonsterRole)
        {
            return;
        }

        if (!GameHistory.PlayerStats.TryGetValue(source.PlayerId, out var stats))
        {
            return;
        }

        if (MonsterState.IsDigesting)
        {
            // Devoured kills count as regular kills (CorrectKills is incremented by default murder handler).
            // If Monster has AssassinModifier and digestion happened during a meeting,
            // TownOfUs's AssassinEvents handler also incorrectly incremented stats.CorrectAssassinKills / IncorrectAssassinKills.
            // Undo that incorrect assassin stat increment for devours:
            if (source.HasModifier<AssassinModifier>())
            {
                if (source != @event.Target)
                {
                    stats.CorrectAssassinKills = Math.Max(0, stats.CorrectAssassinKills - 1);
                }
                else
                {
                    stats.IncorrectAssassinKills = Math.Max(0, stats.IncorrectAssassinKills - 1);
                }
            }
            return;
        }

        // Meeting murder that is NOT digestion: Monster guessed via Assassin!
        if (MeetingHud.Instance && source.HasModifier<AssassinModifier>())
        {
            // Guesses count as guesses (CorrectAssassinKills / IncorrectAssassinKills incremented by TownOfUs).
            // Undo the default regular kill count so guesses don't also count as regular kills towards stats:
            if (source != @event.Target)
            {
                stats.CorrectKills = Math.Max(0, stats.CorrectKills - 1);
            }
            else
            {
                stats.IncorrectKills = Math.Max(0, stats.IncorrectKills - 1);
            }
        }
    }
}
