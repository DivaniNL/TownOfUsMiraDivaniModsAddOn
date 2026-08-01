using System.Collections;
using System.Linq;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using DivaniMods.Assets;
using DivaniMods.Modifiers.Game.Alliance;
using DivaniMods.Options;
using TownOfUs.Events;
using TownOfUs.Events.TouEvents;
using TownOfUs.Modifiers;
using TownOfUs.Modules.Localization;
using TownOfUs.Utilities;
using UnityEngine;

namespace DivaniMods.Events.Modifiers;

public static class YinYangEvents
{
    [RegisterEvent]
    public static void OnRoundStart(RoundStartEvent evt)
    {
        if (evt.TriggeredByIntro)
        {
            return;
        }

        YinYangModifier.CheckAlliedPair();
        YinYangModifier.CheckForCompletion();
        ApplyShame();
    }

    [RegisterEvent]
    public static void OnEjection(EjectionEvent evt)
    {
        YinYangModifier.CheckForCompletion();
        ApplyShame();
    }

    [RegisterEvent]
    public static void OnAfterMurder(AfterMurderEvent evt)
    {
        YinYangModifier.CheckForCompletion();
    }

    [RegisterEvent]
    public static void OnChangeRole(ChangeRoleEvent evt)
    {
        YinYangModifier.CheckAlliedPair();
    }

    private static void ApplyShame()
    {
        if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost)
        {
            return;
        }

        var behavior = (LosingYinYangerBehavior)OptionGroupSingleton<YinYangOptions>.Instance.LosingYinYanger.Value;
        if (behavior != LosingYinYangerBehavior.LeavesInShame)
        {
            return;
        }

        foreach (var side in YinYangModifier.ActiveSides())
        {
            if (!side.HasLost || side.ShameApplied)
            {
                continue;
            }

            var player = side.Player;
            if (player == null || player.Data == null || player.HasDied() ||
                Helpers.GetAlivePlayers().Count <= 3)
            {
                continue;
            }

            RpcYinYangShame(player);
        }
    }

    [MethodRpc((uint)DivaniRpcCalls.YinYangShame)]
    public static void RpcYinYangShame(PlayerControl loser)
    {
        if (loser == null || loser.Data == null || loser.HasDied())
        {
            return;
        }

        var side = YinYangModifier.GetSideOf(loser);
        if (side == null || side.ShameApplied)
        {
            return;
        }

        side.ShameApplied = true;

        Coroutines.Start(CoShowShameNotification(loser, side.ModifierName, side.SideColor));

        loser.Exiled();

        DeathHandlerModifier.UpdateDeathHandlerImmediate(loser, TouLocale.Get("DiedToShame"),
            DeathEventHandlers.CurrentRound, DeathHandlerOverride.SetFalse,
            lockInfo: DeathHandlerOverride.SetTrue);
    }

    private static IEnumerator CoShowShameNotification(PlayerControl loser, string sideName, Color sideColor)
    {
        while (ExileController.Instance != null || MeetingHud.Instance != null)
        {
            yield return null;
        }

        if (loser == null || loser.Data == null)
        {
            yield break;
        }

        var hex = ColorUtility.ToHtmlStringRGB(sideColor);
        var text = loser.AmOwner
            ? $"<b><color=#{hex}>You lost the Yin-Yang as {sideName}, and leave in shame.</color></b>"
            : $"<b><color=#{hex}>{loser.Data.PlayerName} lost the Yin-Yang as {sideName}, and leaves in shame.</color></b>";

        var notif = Helpers.CreateAndShowNotification(
            text, Color.white, new Vector3(0f, 1f, -20f),
            spr: DivaniAssets.YinYangIcon.LoadAsset());

        notif.AdjustNotification();
    }
}
