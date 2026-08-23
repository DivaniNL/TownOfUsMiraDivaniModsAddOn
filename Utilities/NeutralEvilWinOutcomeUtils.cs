using MiraAPI.Hud;
using MiraAPI.Utilities;
using DivaniMods.Buttons.Neutral;
using DivaniMods.Interfaces;
using DivaniMods.Options;
using TownOfUs.Events;
using TownOfUs.Modifiers;
using TownOfUs.Modules.Localization;
using TownOfUs.Utilities;
using UnityEngine;
using TownOfUs.Modules;

namespace DivaniMods.Utilities;

public static class NeutralEvilWinOutcomeUtils
{
    public static void TryResolveQuietWin(RoleBehaviour roleBehaviour, INeutralEvilWinOutcomeRole role)
    {
        if (role.AboutToTorment || role.EffectiveWinOutcome == NeutralEvilWinOutcome.EndsGame || !role.ReachedWinCondition)
        {
            return;
        }

        role.AboutToTorment = true;

        var player = roleBehaviour.Player;
        if (player == null)
        {
            return;
        }

        var roleTag = $"{role.RoleColor.ToTextColor()}{roleBehaviour.GetRoleName}</color>";

        if (player.AmOwner)
        {
            if (!player.HasDied())
            {
                player.DelayExile();
                GameHistory.UpdatePlayerDeathData(player,
                    "DiedToWinning", TownOfUs.Modules.Components.HudManagerHelper.Instance.CurrentRound, diedThisRound : DeathHandlerOverride.SetTrue,
                    lockInfo: DeathHandlerOverride.SetTrue);
            }

            var selfNotif = Helpers.CreateAndShowNotification(
                $"<b>{TouLocale.GetParsed("DivaniNeutralEvilWonSelf").Replace("<role>", roleTag)}</b>",
                Color.white, new Vector3(0f, 1f, -20f), spr: role.WinIcon.LoadAsset());
            selfNotif.AdjustNotification();

            if (role.EffectiveWinOutcome == NeutralEvilWinOutcome.KillOnePlayer)
            {
                var tormentNotif = Helpers.CreateAndShowNotification(
                    $"<b>{TouLocale.GetParsed("DivaniNeutralEvilTormentFeedback")}</b>",
                    Color.white, new Vector3(0f, 0.85f, -20f));
                tormentNotif.AdjustNotification();

                var button = CustomButtonSingleton<NeutralEvilTormentButton>.Instance;
                button.Owner = role;
                button.Show = true;
                button.SetActive(true, roleBehaviour);
            }
        }
        else
        {
            var otherNotif = Helpers.CreateAndShowNotification(
                $"<b>{TouLocale.GetParsed("DivaniNeutralEvilWonOther").Replace("<role>", roleTag).Replace("<player>", player.Data.PlayerName)}</b>",
                Color.white, new Vector3(0f, 1f, -20f), spr: role.WinIcon.LoadAsset());
            otherNotif.AdjustNotification();
        }
    }
}
