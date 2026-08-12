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
        if (source == null || source.Data?.Role is not MonsterRole || !MeetingHud.Instance)
        {
            return;
        }

        if (!source.HasModifier<AssassinModifier>())
        {
            return;
        }

        if (!GameHistory.PlayerStats.TryGetValue(source.PlayerId, out var stats))
        {
            return;
        }

        if (source != @event.Target)
        {
            stats.CorrectAssassinKills = Math.Max(0, stats.CorrectAssassinKills - 1);
        }
        else
        {
            stats.IncorrectAssassinKills = Math.Max(0, stats.IncorrectAssassinKills - 1);
        }
    }
}
