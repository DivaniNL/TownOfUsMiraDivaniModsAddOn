using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.Modifiers;
using DivaniMods.Modifiers.Neutral.NeutralEvil;
using DivaniMods.Roles.Neutral.NeutralEvil;

namespace DivaniMods.Patches;

[HarmonyPatch]
public static class InnocentPatch
{
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CoStartGame))]
    [HarmonyPostfix]
    public static void ResetOnGameStart()
    {
        StripAllInnocentTargetMarkers();
        InnocentRole.ClearAndReload();
    }

    [RegisterEvent(1000)]
    public static void OnBeforeMurderLate(BeforeMurderEvent evt)
    {
        if (!evt.IsCancelled || evt.Target == null || evt.Source == null)
        {
            return;
        }

        if (InnocentRole.ActiveInnocents.TryGetValue(evt.Target.PlayerId, out var innocent) &&
            innocent.PendingTauntKillerId == evt.Source.PlayerId)
        {
            innocent.PendingTauntKillerId = null;
        }
    }

    [RegisterEvent]
    public static void OnAfterMurder(AfterMurderEvent evt)
    {
        if (evt.Target == null || evt.Source == null ||
            !InnocentRole.ActiveInnocents.TryGetValue(evt.Target.PlayerId, out var innocent))
        {
            return;
        }

        if (innocent.PendingTauntKillerId != evt.Source.PlayerId)
        {
            return;
        }

        innocent.PendingTauntKillerId = null;
        innocent.TauntedKillerId = evt.Source.PlayerId;
        innocent.TargetVoted = false;
        innocent.AboutToWin = false;
        innocent.AwaitingNextMeetingExile = true;
        innocent.WinWindowExpired = false;

        GiveKillerTauntMarker(evt.Source, evt.Target.PlayerId);
    }

    [RegisterEvent]
    public static void OnEjection(EjectionEvent evt)
    {
        var exiled = evt.ExileController?.initData?.networkedPlayer?.Object;
        if (exiled == null)
        {
            return;
        }

        foreach (var innocent in GetInnocents())
        {
            if (!innocent.AwaitingNextMeetingExile || innocent.TauntedKillerId != exiled.PlayerId)
            {
                continue;
            }

            // Require the runtime marker so ejection matches the same state as meeting UI.
            if (!InnocentTauntMeetingDisplay.KillerHasTauntMarkerForInnocent(exiled, innocent.Player.PlayerId))
            {
                continue;
            }

            innocent.AboutToWin = true;
            innocent.AwaitingNextMeetingExile = false;
            innocent.TargetVoted = true;
            // First meeting after taunt resolved with correct exile — strip marker immediately (RoundStart still clears as backup).
            RemoveInnocentTauntMarker(exiled.PlayerId, innocent.Player.PlayerId);
        }
    }

    [RegisterEvent]
    public static void OnPlayerDeath(PlayerDeathEvent evt)
    {
        if (evt.DeathReason != DeathReason.Exile)
        {
            return;
        }

        foreach (var innocent in GetInnocents())
        {
            if (innocent.TauntedKillerId == evt.Player.PlayerId && innocent.AboutToWin && !innocent.WinWindowExpired)
            {
                innocent.TargetVoted = true;
            }
        }
    }

    /// <summary>
    /// After the first meeting following a taunt kill, drop the meeting marker: either the win branch
    /// (target voted out) or the taunt window expired without voting out the killer.
    /// </summary>
    [RegisterEvent]
    public static void OnRoundStart(RoundStartEvent evt)
    {
        if (evt.TriggeredByIntro)
        {
            return;
        }

        foreach (var innocent in GetInnocents())
        {
            if (innocent.AboutToWin && innocent.TauntedKillerId.HasValue)
            {
                innocent.TargetVoted = true;
                RemoveInnocentTauntMarker(innocent.TauntedKillerId.Value, innocent.Player.PlayerId);
            }
            else if (innocent.AwaitingNextMeetingExile)
            {
                if (innocent.TauntedKillerId.HasValue)
                {
                    RemoveInnocentTauntMarker(innocent.TauntedKillerId.Value, innocent.Player.PlayerId);
                }

                innocent.AwaitingNextMeetingExile = false;
                innocent.WinWindowExpired = true;
                innocent.TauntedKillerId = null;
            }
        }
    }

    private static IEnumerable<InnocentRole> GetInnocents()
    {
        return InnocentRole.ActiveInnocents.Values;
    }

    private static void StripAllInnocentTargetMarkers()
    {
        foreach (var pc in PlayerControl.AllPlayerControls.ToArray())
        {
            if (pc == null)
            {
                continue;
            }

            var comp = pc.GetModifierComponent();
            if (comp == null)
            {
                continue;
            }

            foreach (var m in pc.GetModifiers<InnocentTargetModifier>().ToArray())
            {
                comp.RemoveModifier(m);
            }
        }
    }

    private static void GiveKillerTauntMarker(PlayerControl killer, byte innocentPlayerId)
    {
        var comp = killer.GetModifierComponent();
        if (comp == null)
        {
            return;
        }

        foreach (var existing in killer.GetModifiers<InnocentTargetModifier>().ToArray())
        {
            comp.RemoveModifier(existing);
        }

        killer.AddModifier<InnocentTargetModifier>(innocentPlayerId);
    }

    private static void RemoveInnocentTauntMarker(byte killerPlayerId, byte innocentPlayerId)
    {
        var killer = GameData.Instance?.GetPlayerById(killerPlayerId)?.Object;
        if (killer == null)
        {
            return;
        }

        var comp = killer.GetModifierComponent();
        if (comp == null)
        {
            return;
        }

        foreach (var m in killer.GetModifiers<InnocentTargetModifier>().ToArray())
        {
            if (m.InnocentPlayerId == innocentPlayerId)
            {
                comp.RemoveModifier(m);
            }
        }
    }
}
