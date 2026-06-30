using MiraAPI.Events;
using MiraAPI.Events.Mira;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using DivaniMods.Buttons.Neutral.NeutralKilling;
using DivaniMods.Modules.Watcher;
using DivaniMods.Options;
using DivaniMods.Roles.Neutral.NeutralKilling;
using TownOfUs.Modifiers;
using TownOfUs.Utilities;

namespace DivaniMods.Events.Neutral.NeutralKilling;

public static class WatcherEvents
{
    [RegisterEvent]
    public static void AfterMurderHandler(AfterMurderEvent @event)
    {
        var killer = @event.Source;
        if (killer == null || !killer.IsRole<WatcherRole>())
        {
            return;
        }

        if (WatcherLightSystem.IsActive)
        {
            var manual = WatcherLightSystem.ConsumeManualKill(@event.Target.PlayerId);
            if (manual && killer.AmOwner
                && OptionGroupSingleton<WatcherOptions>.Instance.KillsDuringLightsCount.Value)
            {
                CustomButtonSingleton<WatcherWatchButton>.Instance?.AccrueKill();
            }
            else
            {
                WatcherLightSystem.OnConfirmedRedLightKill(@event.Target);
            }

            return;
        }

        if (killer.AmOwner)
        {
            CustomButtonSingleton<WatcherWatchButton>.Instance?.AccrueKill();
        }
    }

    // Block DIRECT kills by a watched player during Red Light (e.g. a UseVanillaKillButton
    // role like Blackmailer, whose kill bypasses the watched modifier via Mira's own
    // KillButton.DoClick). Indirect kills carry IndirectAttackerModifier on the source
    // (added before this event fires), so bombs/frag planted in Green Light still detonate.
    [RegisterEvent]
    public static void BeforeMurderHandler(BeforeMurderEvent @event)
    {
        var killer = @event.Source;
        if (killer == null)
        {
            return;
        }

        if (WatcherLightSystem.BlocksKill(killer) && !killer.HasModifier<IndirectAttackerModifier>())
        {
            @event.Cancel();
        }
    }

    [RegisterEvent]
    public static void OnButtonClick(MiraButtonClickEvent @event)
    {
        var me = PlayerControl.LocalPlayer;
        if (me == null || !me.IsRole<WatcherRole>() || !WatcherLightSystem.IsRedLightActive)
        {
            return;
        }

        if (@event.Button is WatcherKillButton or WatcherWatchButton)
        {
            return;
        }

        @event.Cancel();
    }
}
