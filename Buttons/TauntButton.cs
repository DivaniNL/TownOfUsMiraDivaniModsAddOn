using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Networking;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Reactor.Networking.Attributes;
using DivaniMods.Options;
using DivaniMods.Roles;
using TownOfUs.Assets;
using TownOfUs.Buttons;
using TownOfUs.Events;
using UnityEngine;

namespace DivaniMods.Buttons;

public sealed class TauntButton : CustomActionButton<PlayerControl>
{
    public override string Name => "Taunt";
    public override float Cooldown => OptionGroupSingleton<InnocentOptions>.Instance.TauntCooldown;
    public override float EffectDuration => 0f;
    public override int MaxUses => 0;
    public override LoadableAsset<Sprite>? Sprite => TouNeutAssets.JesterHauntSprite;
    public override float Distance => 1.5f;
    public override ButtonLocation Location { get; set; } = ButtonLocation.BottomRight;
    public override Color TextOutlineColor => InnocentRole.InnocentColor;
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;

    private static bool CanTauntThisRound =>
        OptionGroupSingleton<InnocentOptions>.Instance.CanTauntFirstRound ||
        TutorialManager.InstanceExists ||
        DeathEventHandlers.CurrentRound > 1;

    public override bool Enabled(RoleBehaviour? role)
    {
        return role is InnocentRole;
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

        Target.cosmetics.SetOutline(active, new Il2CppSystem.Nullable<Color>(InnocentRole.InnocentColor));
    }

    public override bool IsTargetValid(PlayerControl? target)
    {
        if (target == null || target.Data == null || target.Data.IsDead || target.Data.Disconnected)
        {
            return false;
        }

        return target != PlayerControl.LocalPlayer;
    }

    public override bool CanUse()
    {
        var player = PlayerControl.LocalPlayer;
        if (player == null || player.Data == null || player.Data.IsDead)
        {
            return false;
        }

        if (player.Data.Role is not InnocentRole || !CanTauntThisRound)
        {
            return false;
        }

        return base.CanUse();
    }

    protected override void OnClick()
    {
        var player = PlayerControl.LocalPlayer;
        if (player == null || Target == null || player.Data?.Role is not InnocentRole)
        {
            return;
        }

        RpcTaunt(player, Target.PlayerId);
        ResetTarget();
    }

    [MethodRpc((uint)DivaniRpcCalls.InnocentTaunt)]
    public static void RpcTaunt(PlayerControl innocent, byte killerId)
    {
        if (LobbyBehaviour.Instance || innocent.Data?.Role is not InnocentRole role)
        {
            return;
        }

        var killer = PlayerControl.AllPlayerControls.ToArray()
            .FirstOrDefault(player => player != null && player.PlayerId == killerId);
        if (killer == null || killer.Data == null || killer.Data.IsDead || innocent.Data.IsDead)
        {
            return;
        }

        role.PendingTauntKillerId = killerId;

        if (AmongUsClient.Instance.AmHost)
        {
            killer.RpcCustomMurder(
                innocent,
                MeetingCheck.OutsideMeeting,
                resetKillTimer: false);
        }
    }
}
