using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Mira;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Translation;
using MiraAPI.Hud;
using DivaniMods.Assets;
using DivaniMods.Buttons.Impostor.ImpostorKilling;
using DivaniMods.Modules.Mosquito;
using DivaniMods.Roles.Impostor.ImpostorKilling;
using TownOfUs.Utilities;
using UnityEngine;

namespace DivaniMods.Events.Impostor.ImpostorKilling;

[HarmonyPatch]
public static class MosquitoEvents
{
    [RegisterEvent]
    public static void OnStartMeeting(StartMeetingEvent _)
    {
        MosquitoObject.DestroyAll();
    }

    [RegisterEvent(2000)]
    public static void OnBeforeMurder(BeforeMurderEvent evt)
    {
        if (evt.IsCancelled && evt.Target != null && evt.Target.PlayerId == MosquitoObject.PendingStingTargetId)
        {
            MosquitoObject.PendingStingTargetId = byte.MaxValue;
        }
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
            // Deadlock-style: each kill grants extra sting charges.
            sting.AddCharges(MosquitoStingButton.ChargesPerKill);
        }

        if (evt.Target != null && evt.Target.PlayerId == MosquitoObject.PendingStingTargetId)
        {
            MosquitoObject.PendingStingTargetId = byte.MaxValue;

            var colorHex = ColorUtility.ToHtmlStringRGB(Palette.ImpostorRed);
            var notificationText = MiraLocaleManager
                .Get("DivaniMods.Role.Mosquito.Notification.Stung")
                .Replace("<player>", evt.Target.Data.PlayerName);

            MiraAPI.Utilities.Helpers.CreateAndShowNotification(
                $"<b><color=#{colorHex}>{notificationText}</color></b>",
                Color.white,
                new Vector3(0f, 1f, -20f),
                spr: DivaniAssets.MosquitoIcon.LoadAsset());
        }
    }
    [RegisterEvent(2000)]
    public static void OnKillButtonClick(MiraButtonClickEvent evt)
    {
        if (evt.Button is not MosquitoKillButton || !evt.IsCancelled)
        {
            return;
        }

        var sting = CustomButtonSingleton<MosquitoStingButton>.Instance;
        if (sting != null)
        {
            sting.SetTimer(sting.Cooldown);
        }
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
