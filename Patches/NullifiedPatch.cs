using System;
using System.Reflection;
using HarmonyLib;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using DivaniMods.Modifiers.Game.Impostor.ImpostorPassive;
using DivaniMods.Options;
using TownOfUs.Modifiers.Game.Crewmate;

namespace DivaniMods.Patches;

public static class NullifiedPatch
{
    public static bool ShouldNullify(PlayerControl? source)
    {
        return source != null && source.HasModifier<NullifiedModifier>();
    }

    public static bool SkipDebuffIfNullified(AfterMurderEvent @event)
    {
        return !ShouldNullify(@event?.Source);
    }

    public static bool SkipCelebrityIfNullified(AfterMurderEvent @event)
    {
        if (!OptionGroupSingleton<NullifiedOptions>.Instance.SilencesCelebrity)
        {
            return true;
        }

        if (!ShouldNullify(@event?.Source))
        {
            return true;
        }

        var target = @event!.Target;
        if (target != null && target.TryGetModifier<CelebrityModifier>(out var celeb))
        {
            celeb.Announced = true;
        }

        return false;
    }

    public static void Initialize(Harmony harmony)
    {
        try
        {
            var touAssembly = Assembly.Load("TownOfUsMira");
            if (touAssembly == null)
            {
                DivaniPlugin.Instance.Log.LogWarning("Nullified: TownOfUsMira assembly not found");
                return;
            }

            var debuffPrefix = typeof(NullifiedPatch).GetMethod(
                nameof(SkipDebuffIfNullified), BindingFlags.Public | BindingFlags.Static);
            var celebrityPrefix = typeof(NullifiedPatch).GetMethod(
                nameof(SkipCelebrityIfNullified), BindingFlags.Public | BindingFlags.Static);

            var debuffHandlers = new[]
            {
                "TownOfUs.Events.Modifiers.BaitEvents",
                "TownOfUs.Events.Modifiers.FrostyEvents",
                "TownOfUs.Events.Modifiers.DiseasedEvents",
                "TownOfUs.Events.Modifiers.AftermathEvents",
                "TownOfUs.Events.Modifiers.NoisemakerEvents",
            };

            foreach (var typeName in debuffHandlers)
            {
                var type = touAssembly.GetType(typeName);
                var method = type?.GetMethod("AfterMurderEventHandler",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                if (method != null && debuffPrefix != null)
                {
                    harmony.Patch(method, prefix: new HarmonyMethod(debuffPrefix));
                }
            }

            var celebType = touAssembly.GetType("TownOfUs.Events.Modifiers.CelebrityEvents");
            var celebMethod = celebType?.GetMethod("AfterMurderEventHandler",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            if (celebMethod != null && celebrityPrefix != null)
            {
                harmony.Patch(celebMethod, prefix: new HarmonyMethod(celebrityPrefix));
            }
        }
        catch (Exception ex)
        {
            DivaniPlugin.Instance.Log.LogError($"Nullified: Failed to initialize: {ex.Message}");
        }
    }
}
