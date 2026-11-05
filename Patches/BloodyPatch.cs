using System.Linq;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Modifiers;
using DivaniMods.Modifiers.Crewmate;
using DivaniMods.Modifiers.Game.Crewmate;
using UnityEngine;

namespace DivaniMods.Patches;

public static class BloodyPatch
{
    [RegisterEvent]
    public static void OnAfterMurder(AfterMurderEvent evt)
    {
        if (MeetingHud.Instance || evt.Source == null || evt.Target == null)
        {
            return;
        }

        if (!evt.Target.HasModifier<BloodyModifier>())
        {
            return;
        }

        if (NullifiedPatch.ShouldNullify(evt.Source))
        {
            return;
        }

        var killer = evt.Source;
        var comp = killer.GetModifierComponent();
        if (comp == null)
        {
            return;
        }

        foreach (var existing in killer.GetModifiers<BloodyKillerFootstepsModifier>().ToArray())
        {
            comp.RemoveModifier(existing);
        }

        killer.AddModifier<BloodyKillerFootstepsModifier>();
    }

    [RegisterEvent]
    public static void OnMeetingStart(StartMeetingEvent _)
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

            foreach (var m in pc.GetModifiers<BloodyKillerFootstepsModifier>().ToArray())
            {
                comp.RemoveModifier(m);
            }
        }
    }
}
