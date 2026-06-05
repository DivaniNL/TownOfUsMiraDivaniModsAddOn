using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Hud;
using DivaniMods.Buttons.Impostor.ImpostorPower;
using DivaniMods.Modules.Mosquito;
using DivaniMods.Roles.Impostor.ImpostorPower;
using TownOfUs.Utilities;

namespace DivaniMods.Events.Impostor.ImpostorPower;

[HarmonyPatch]
public static class MosquitoEvents
{
    [RegisterEvent]
    public static void OnStartMeeting(StartMeetingEvent _)
    {
        MosquitoObject.DestroyAll();
    }

    [RegisterEvent]
    public static void OnAfterMurder(AfterMurderEvent evt)
    {
        if (evt.Source == null || !evt.Source.AmOwner || evt.Source.Data?.Role is not MosquitoRole)
        {
            return;
        }

        var sting = CustomButtonSingleton<MosquitoStingButton>.Instance;
        if (sting != null)
        {
            sting.Timer = sting.Cooldown;
            // Deadlock-style: each kill grants extra sting charges.
            sting.AddCharges(MosquitoStingButton.ChargesPerKill);
        }

        evt.Source.SetKillTimer(evt.Source.GetKillCooldown());
    }

    [RegisterEvent]
    public static void OnRoundStart(RoundStartEvent evt)
    {
        if (evt.TriggeredByIntro)
        {
            CustomButtonSingleton<MosquitoStingButton>.Instance?.ResetCharges();
        }
    }

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
    [HarmonyPostfix]
    public static void OnGameEnd()
    {
        MosquitoObject.DestroyAll();
    }
}
