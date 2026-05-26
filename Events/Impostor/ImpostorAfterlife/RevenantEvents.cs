using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Utilities;
using DivaniMods.Assets;
using DivaniMods.Events.Impostor.ImpostorPower;
using DivaniMods.Roles.Impostor.ImpostorAfterlife;
using DivaniMods.Roles.Impostor.ImpostorPower;
using UnityEngine;

namespace DivaniMods.Events.Impostor.ImpostorAfterlife;

public static class RevenantEvents
{
    [RegisterEvent]
    public static void OnEndMeeting(EndMeetingEvent _)
    {
        if (!SummonerState.FinalFourOrLess)
        {
            return;
        }

        var localWasRevenant = false;
        var deactivated = false;
        foreach (var pc in PlayerControl.AllPlayerControls)
        {
            if (pc?.Data?.Role is RevenantRole { GhostActive: true } rev)
            {
                rev.Caught = true;
                deactivated = true;
                if (pc.AmOwner)
                {
                    localWasRevenant = true;
                }
            }
        }

        if (!deactivated)
        {
            return;
        }

        SummonerState.ResetKills();

        var hex = ColorUtility.ToHtmlStringRGB(RevenantRole.RevenantColor);

        if (localWasRevenant)
        {
            Helpers.CreateAndShowNotification(
                $"<b><color=#{hex}>Final four reached - your Revenant returns to rest.</color></b>",
                Color.white,
                new Vector3(0f, 1f, -20f),
                spr: DivaniAssets.RevenantIcon.LoadAsset());
        }

        if (PlayerControl.LocalPlayer?.Data?.Role is SummonerRole)
        {
            Helpers.CreateAndShowNotification(
                $"<b><color=#{hex}>Final four reached - you can no longer summon a Revenant.</color></b>",
                Color.white,
                new Vector3(0f, 1f, -20f),
                spr: DivaniAssets.SummonerIcon.LoadAsset());
        }
    }
}
