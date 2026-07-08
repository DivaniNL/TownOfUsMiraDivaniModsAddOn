using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using DivaniMods.Assets;
using DivaniMods.Modifiers.Crewmate.CrewmateSupport;
using DivaniMods.Options;
using DivaniMods.Roles.Crewmate.CrewmateSupport;
using TownOfUs.Buttons;
using UnityEngine;

namespace DivaniMods.Buttons.Crewmate.CrewmateSupport;

public sealed class LocatorMarkButton : TownOfUsTargetButton<PlayerControl>
{
    public override string Name => "Mark";
    public override float Cooldown => 25f;
    public override float EffectDuration => 0f;
    public override int MaxUses => (int)OptionGroupSingleton<LocatorOptions>.Instance.AbilityUses.Value;
    public override LoadableAsset<Sprite> Sprite => DivaniAssets.LocatorMarkButton;
    public override float Distance => 1.5f;
    public override ButtonLocation Location { get; set; } = ButtonLocation.BottomRight;
    public override Color TextOutlineColor => LocatorRole.LocatorColor;
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;

    public override bool Enabled(RoleBehaviour? role)
    {
        return role is LocatorRole;
    }

    public override PlayerControl? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetClosestPlayer(true, Distance, true);
    }

    public override void SetOutline(bool active)
    {
        if (Target == null)
        {
            return;
        }

        Target.cosmetics.SetOutline(active, new Il2CppSystem.Nullable<Color>(LocatorRole.LocatorColor));
    }

    public override bool IsTargetValid(PlayerControl? target)
    {
        if (target == null || target.Data == null || target.Data.IsDead || target.Data.Disconnected)
        {
            return false;
        }

        if (target == PlayerControl.LocalPlayer)
        {
            return false;
        }

        return !target.HasModifier<LocatorMarkModifier>();
    }

    public override bool CanUse()
    {
        var player = PlayerControl.LocalPlayer;
        if (player == null || player.Data == null || player.Data.IsDead)
        {
            return false;
        }

        if (player.Data.Role is not LocatorRole)
        {
            return false;
        }

        SetUses(LocatorRole.MarksRemaining);
        if (LocatorRole.MarksRemaining <= 0)
        {
            return false;
        }

        var perRound = (int)OptionGroupSingleton<LocatorOptions>.Instance.MarksPerRound.Value;
        if (LocatorRole.MarksThisRound >= perRound)
        {
            return false;
        }

        return base.CanUse();
    }

    protected override void OnClick()
    {
        var player = PlayerControl.LocalPlayer;
        if (player == null || Target == null || player.Data?.Role is not LocatorRole)
        {
            return;
        }

        if (LocatorRole.MarksRemaining <= 0 || Target.HasModifier<LocatorMarkModifier>())
        {
            return;
        }

        Target.RpcAddModifier<LocatorMarkModifier>();

        LocatorRole.MarksRemaining--;
        LocatorRole.MarksThisRound++;
        SetUses(LocatorRole.MarksRemaining);
        ResetTarget();
    }
}
