using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using DivaniMods.Assets;
using DivaniMods.Modifiers.Game.Universal;
using DivaniMods.Options;
using TownOfUs.Buttons;
using TownOfUs.Modules;
using UnityEngine;

namespace DivaniMods.Buttons.Modifiers;

public sealed class TacticalInsertionButton : TownOfUsButton
{
    public override string Name => "Tactical Insertion";
    public override Color TextOutlineColor => TacticalInsertionModifier.TacticalColor;
    public override BaseKeybind Keybind => Keybinds.ModifierAction;
    public override ButtonLocation Location => ButtonLocation.BottomLeft;

    public override float Cooldown =>
        Math.Clamp(OptionGroupSingleton<TacticalInsertionOptions>.Instance.Cooldown.Value + MapCooldown, 5f, 120f);

    public override int MaxUses => (int)OptionGroupSingleton<TacticalInsertionOptions>.Instance.Uses.Value;

    public override LoadableAsset<Sprite> Sprite => DivaniAssets.TacticalInsertionButton;

    public override bool ZeroIsInfinite { get; set; } = true;

    public override bool Enabled(RoleBehaviour? role)
    {
        var player = PlayerControl.LocalPlayer;
        if (player == null || player.Data == null || player.Data.IsDead)
        {
            return false;
        }

        return player.HasModifier<TacticalInsertionModifier>();
    }

    public override bool CanUse()
    {
        var player = PlayerControl.LocalPlayer;
        if (player == null || player.Data == null || player.Data.IsDead)
        {
            return false;
        }

        var modifier = player.GetModifier<TacticalInsertionModifier>();
        if (modifier == null)
        {
            SetUses(0);
            return false;
        }

        SetUses(modifier.UsesRemaining);

        if (modifier.UsesRemaining <= 0 || modifier.UsedThisRound || modifier.MarkedLocation.HasValue)
        {
            return false;
        }

        if (ModCompatibility.GetPlayerElevator(player).Item1)
        {
            return false;
        }

        return base.CanUse();
    }

    protected override void OnClick()
    {
        if (MeetingHud.Instance || ExileController.Instance)
        {
            return;
        }

        var player = PlayerControl.LocalPlayer;
        if (player == null)
        {
            return;
        }

        var modifier = player.GetModifier<TacticalInsertionModifier>();
        if (modifier == null || modifier.UsesRemaining <= 0 || modifier.UsedThisRound)
        {
            return;
        }

        modifier.UsesRemaining--;

        var pos = player.transform.position;
        TacticalInsertionModifier.RpcMark(player, pos.x, pos.y);

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound(DivaniAssets.TacInsertPlaceSound.LoadAsset(), false, 1f);
        }

        MiraAPI.Utilities.Helpers.CreateAndShowNotification(
            "<b><color=#00FF00>Position marked</color></b>",
            Color.white,
            new Vector3(0f, 1f, -20f),
            spr: DivaniAssets.TacticalInsertionIcon.LoadAsset());
    }
}
