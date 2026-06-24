using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Modifiers;
using DivaniMods.Modifiers.Game.Universal;

namespace DivaniMods.Events.Modifiers;

public static class TacticalInsertionEvents
{
    [RegisterEvent]
    public static void RoundStartHandler(RoundStartEvent @event)
    {
        if (@event.TriggeredByIntro)
        {
            return;
        }

        ModifierUtils.GetActiveModifiers<TacticalInsertionModifier>().Do(x => x.OnRoundStart());
    }
}
