using System.Linq;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using DivaniMods.Assets;
using TownOfUs.Utilities;
using UnityEngine;

namespace DivaniMods.Modifiers.Game.Alliance;

public sealed class BetrayerRevealedModifier : BaseModifier
{
    public const string ColorTag = "#BA71FF";

    public override string ModifierName => "Betrayer Revealed";
    public override bool HideOnUi => true;

    public static bool AnyRevealed()
    {
        return ModifierUtils.GetActiveModifiers<BetrayerRevealedModifier>()
            .Any(x => x.Player != null && !x.Player.HasDied());
    }

    public override void OnActivate()
    {
        var local = PlayerControl.LocalPlayer;

        if (local == null || local.Data == null || Player == null || Player.Data == null)
        {
            return;
        }

        if (local.PlayerId == Player.PlayerId)
        {
            Notify("<b>The Impostors are aware of your whereabouts as the " +
                   $"<color={ColorTag}>Betrayer</color>. Stay on your toes!</b>");
            return;
        }

        if (local.IsImpostorAligned() && !local.HasModifier<BetrayerModifier>())
        {
            Notify($"<b>{Player.Data.PlayerName} has been betraying you all along, kill the " +
                   $"<color={ColorTag}>Betrayer</color> before your victory is stolen!</b>");
        }
    }

    private static void Notify(string message)
    {
        var notification = Helpers.CreateAndShowNotification(
            message,
            Color.white,
            new Vector3(0f, 1f, -20f),
            spr: DivaniAssets.BetrayerIcon.LoadAsset());

        notification.AdjustNotification();
    }
}
