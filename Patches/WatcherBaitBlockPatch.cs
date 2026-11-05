using HarmonyLib;
using MiraAPI.Events.Vanilla.Gameplay;
using DivaniMods.Modules.Watcher;
using TownOfUs.Events.Modifiers;

namespace DivaniMods.Patches;

[HarmonyPatch(typeof(BaitEvents), nameof(BaitEvents.AfterMurderEventHandler))]
public static class WatcherBaitBlockPatch
{
    [HarmonyPrefix]
    public static bool Prefix(AfterMurderEvent @event)
    {
        return !WatcherLightSystem.IsRedLightKill(@event.Source, @event.Target);
    }
}
