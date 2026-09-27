using DivaniMods.Assets;
using DivaniMods.Modifiers.Impostors;
using DivaniMods.Options;
using DivaniMods.Roles.Impostor.ImpostorKilling;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using TownOfUs.Assets;
using TownOfUs.Buttons;
using TownOfUs.Utilities;
using UnityEngine;

public class DeathnoteButton : CustomActionButton<PlayerControl>
{
    public override string Name => "Note";
    public override float Cooldown => OptionGroupSingleton<DeathnoteOptions>.Instance.NoteCooldown.Value;
    public override LoadableAsset<Sprite> Sprite => DivaniAssets.NoteButton;
    public override bool PauseTimerInVent => true;
    public override ButtonLocation Location => ButtonLocation.BottomRight;
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => Palette.ImpostorRed;

    public override int MaxUses => (int)OptionGroupSingleton<DeathnoteOptions>.Instance.NotesPerGame.Value;

    protected override void OnClick()
    {
        if (Target != null && !Target.HasModifier<DeathnoteModifier>() && !Target.IsImpostorAligned())
        {
            Target?.RpcAddModifier<DeathnoteModifier>();
        }
    }

    public override PlayerControl? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetClosestPlayer(true, Distance);
    }
        public override void CreateButton(Transform parent)
    {
        base.CreateButton(parent);

        if (Button == null) return;

        Button.usesRemainingSprite.sprite = TouAssets.AbilityCounterPlayerSprite.LoadAsset();
        if (TextOutlineColor != Color.clear)
        {
            SetTextOutline(Palette.ImpostorRed);
            Button.usesRemainingSprite.color = TextOutlineColor;
        }
    }

    public override void SetOutline(bool active)
    {
        Target?.cosmetics.SetOutline(active, new Il2CppSystem.Nullable<Color>(Palette.ImpostorRed));
    }

    public override bool IsTargetValid(PlayerControl? target)
    {
        return true;
    }

    public override bool Enabled(RoleBehaviour? role)
    {
        return role is DeathnoteRole;
    }
}