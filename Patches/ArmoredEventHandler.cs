using MiraAPI.Events;
using MiraAPI.Events.Mira;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using System.Linq;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using DivaniMods.Assets;
using DivaniMods.Modifiers.Game.Impostor.ImpostorPassive;
using DivaniMods.Modifiers.Game.Universal;
using UnityEngine;
using TownOfUs;
using TownOfUs.Buttons;
using TownOfUs.Modifiers;
using TownOfUs.Options;
using TownOfUs.Utilities;

namespace DivaniMods.Patches;

public static class ArmoredEventHandler
{
    [RegisterEvent]
    public static void OnRoundStart(RoundStartEvent @event)
    {
        foreach (var mod in ModifierUtils.GetActiveModifiers<ArmoredModifier>().ToList())
        {
            if (mod.AttacksRemaining <= 0)
            {
                var player = mod.Player;

                if (player != null && player.HasModifier<ArmoredShieldModifier>())
                {
                    player.RemoveModifier<ArmoredShieldModifier>();
                }

                player?.RemoveModifier<ArmoredModifier>();

                if (player != null && player.AmOwner)
                {
                    var colorHex = ColorUtility.ToHtmlStringRGB(ArmoredModifier.ArmoredColor);
                    MiraAPI.Utilities.Helpers.CreateAndShowNotification(
                        $"<b><color=#{colorHex}>Your armor is broken</color></b>",
                        Color.white,
                        new Vector3(0f, 1f, -20f),
                        spr: DivaniAssets.ArmoredIcon.LoadAsset());
                }

                continue;
            }

            mod.RefreshDisplayedAttacks();
        }
    }

    [RegisterEvent(-900)]
    public static void BeforeMurderEventHandler(BeforeMurderEvent @event)
    {
        var source = @event.Source;

        if (CheckForArmoredShield(@event, source, @event.Target))
        {
            ResetAttackerCooldown(source);
        }
    }

    [RegisterEvent(-900)]
    public static void MiraButtonClickEventHandler(MiraButtonClickEvent @event)
    {
        var source = PlayerControl.LocalPlayer;
        var button = @event.Button as CustomActionButton<PlayerControl>;
        var target = button?.Target;

        if (target == null || button is not IKillButton || !button.CanClick())
        {
            return;
        }

        if (CheckForArmoredShield(@event, source, target))
        {
            ResetAttackerCooldown(source, button);
        }
    }

    private static bool HasOtherShield(PlayerControl target)
    {
        foreach (var modifier in target.GetModifiers<BaseModifier>())
        {
            if (modifier is ArmoredShieldModifier)
            {
                continue;
            }

            var type = modifier.GetType();
            while (type != null && type != typeof(object))
            {
                if (type.Name == "BaseShieldModifier")
                {
                    return true;
                }
                type = type.BaseType;
            }
        }

        return false;
    }

    private static void ResetAttackerCooldown(PlayerControl source, CustomActionButton<PlayerControl>? button = null)
    {
        if (source == null || !source.AmOwner)
        {
            return;
        }

        button?.ResetCooldownAndOrEffect();
        source.SetKillTimer(source.GetKillCooldown());
    }

    private static bool CheckForArmoredShield(MiraCancelableEvent @event, PlayerControl source, PlayerControl target)
    {
        if (MeetingHud.Instance || ExileController.Instance || source == null || target == null)
        {
            return false;
        }

        if (source.HasModifier<RuthlessModifier>())
        {
            return false;
        }

        if (!target.HasModifier<ArmoredShieldModifier>() ||
            target.PlayerId == source.PlayerId ||
            (source.TryGetModifier<IndirectAttackerModifier>(out var indirect) && indirect.IgnoreShield))
        {
            return false;
        }

        // Only consume armor if the holder has no other shield; let the other shield absorb the hit.
        if (HasOtherShield(target))
        {
            return false;
        }

        @event.Cancel();

        if (TutorialManager.InstanceExists || source.AmOwner)
        {
            RpcArmoredAttacked(source, target);
        }

        return true;
    }

    [MethodRpc((uint)DivaniRpcCalls.ArmoredAttacked)]
    public static void RpcArmoredAttacked(PlayerControl source, PlayerControl armored)
    {
        if (source == null || armored == null)
        {
            return;
        }

        if (armored.TryGetModifier<ArmoredModifier>(out var mod))
        {
            mod.AttacksRemaining = Math.Max(0, mod.AttacksRemaining - 1);

            if (mod.AttacksRemaining <= 0)
            {
                armored.RemoveModifier<ArmoredShieldModifier>();
            }
        }

        if (source.AmOwner)
        {
            Coroutines.Start(MiscUtils.CoFlash(
                OptionGroupSingleton<GameMechanicOptions>.Instance.AnonymousShields
                    ? TownOfUsColors.NeutralWiki
                    : ArmoredModifier.ArmoredColor));
        }
    }
}
