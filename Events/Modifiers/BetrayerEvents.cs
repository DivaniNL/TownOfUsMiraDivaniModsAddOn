using System.Linq;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using DivaniMods.Modifiers.Game.Alliance;
using DivaniMods.Options;
using TownOfUs.Utilities;

namespace DivaniMods.Events.Modifiers;

public static class BetrayerEvents
{
    public static int ImpostorAlignedAtStart { get; private set; }

    [RegisterEvent]
    public static void OnRoundStart(RoundStartEvent @event)
    {
        if (@event.TriggeredByIntro)
        {
            ImpostorAlignedAtStart = Helpers.GetAlivePlayers().Count(x => x.IsImpostorAligned());
        }

        CheckPlayersLeftReveal();
    }

    [RegisterEvent]
    public static void OnAfterMurder(AfterMurderEvent @event)
    {
        if (!AmongUsClient.Instance.AmHost)
        {
            return;
        }

        var source = @event.Source;
        var target = @event.Target;

        if (ImpostorAlignedAtStart > 2 && source != null && target != null &&
            source.HasModifier<BetrayerModifier>() &&
            target.IsImpostorAligned() && !target.HasModifier<BetrayerModifier>())
        {
            Reveal(source);
        }

        CheckPlayersLeftReveal();
    }

    private static void CheckPlayersLeftReveal()
    {
        if (!AmongUsClient.Instance.AmHost)
        {
            return;
        }

        var options = OptionGroupSingleton<BetrayerOptions>.Instance;
        var threshold = ImpostorAlignedAtStart > 2
            ? options.RevealAtPlayersLeftMultiImp.Value
            : options.RevealAtPlayersLeftDuoImp.Value;

        if (Helpers.GetAlivePlayers().Count > threshold)
        {
            return;
        }

        foreach (var betrayer in ModifierUtils.GetActiveModifiers<BetrayerModifier>().ToList())
        {
            if (betrayer.Player != null)
            {
                Reveal(betrayer.Player);
            }
        }
    }

    private static void Reveal(PlayerControl player)
    {
        if (player.HasDied() || player.HasModifier<BetrayerRevealedModifier>())
        {
            return;
        }

        player.RpcAddModifier<BetrayerRevealedModifier>();
    }
}
