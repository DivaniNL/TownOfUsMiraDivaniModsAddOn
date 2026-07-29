using AmongUs.GameOptions;
using DivaniMods.Assets;
using DivaniMods.Modules.Monster;
using DivaniMods.Options;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using TownOfUs;
using TownOfUs.Assets;
using TownOfUs.Roles;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;

namespace DivaniMods.Roles.Neutral.NeutralKilling;

public sealed class MonsterRole(IntPtr cppPtr) : NeutralRole(cppPtr), ITownOfUsRole
{
    public static readonly Color MonsterColor = new Color32(107, 179, 48, 255);
    public string RoleName => "Monster";
    public string RoleDescription => "Eat people. Survive to the next meeting and they're gone for good.";
    public string RoleLongDescription =>
        "Eat nearby players to trap them. If you make it to the next meeting, everyone you've " +
        "eaten is killed for real. If you die first, they're released instead.";

    public Color RoleColor => MonsterColor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleAlignment RoleAlignment => RoleAlignment.NeutralKilling;
    public bool HasImpostorVision => true;

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = MiraAPI.Utilities.Assets.TmpSpriteUtils.CreateSpriteAsset(DivaniAssets.MonsterIcon.LoadAsset(), "DivaniMod.Role.Neutral.Monster", 1.45f),
        Icon = DivaniAssets.MonsterIcon,
        IntroSound = TouAudio.TimeLordIntroSound,
        MaxRoleCount = 1,
        CanUseVent = OptionGroupSingleton<MonsterOptions>.Instance.CanVent.Value,
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>(),
    };

    [HideFromIl2Cpp]
    public bool WinConditionMet()
    {
        if (Player.HasDied()) return false;

        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player == null || player.Data == null || player.HasDied()) continue;
            if (player.PlayerId == Player.PlayerId) continue;
            if (MonsterState.IsEaten(player.PlayerId)) continue;
            return false;
        }

        return true;
    }
    public override void Initialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Initialize(this, targetPlayer);

        if (Player.AmOwner && OptionGroupSingleton<MonsterOptions>.Instance.CanVent.Value)
        {
            HudManager.Instance.ImpostorVentButton.graphic.sprite = DivaniAssets.MonsterVentButton.LoadAsset();
            HudManager.Instance.ImpostorVentButton.buttonLabelText.SetOutlineColor(MonsterColor);
        }
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        MonsterState.ReleaseAll(targetPlayer.PlayerId, targetPlayer.GetTruePosition());
        MonsterState.ForgetMonster(targetPlayer.PlayerId);
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);
        TouRoleUtils.ClearTaskHeader(Player);

        if (Player.AmOwner && OptionGroupSingleton<MonsterOptions>.Instance.CanVent.Value)
        {
            HudManager.Instance.ImpostorVentButton.graphic.sprite = TouAssets.VentSprite.LoadAsset();
            HudManager.Instance.ImpostorVentButton.buttonLabelText.SetOutlineColor(TownOfUsColors.Impostor);
        }
    }

    public override void OnDeath(DeathReason reason)
    {
        if (Player != null)
            MonsterState.ReleaseAll(Player.PlayerId, Player.GetTruePosition());

        RoleBehaviourStubs.OnDeath(this, reason);
    }

    public override bool DidWin(GameOverReason gameOverReason) => WinConditionMet();

    [MethodRpc((uint)DivaniRpcCalls.MonsterDevour, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcEat(PlayerControl Monster, byte victimId)
    {
        if (Monster != null) MonsterState.Eat(Monster.PlayerId, victimId);
    }

    [MethodRpc((uint)DivaniRpcCalls.MonsterDigest, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcDigest(PlayerControl Monster)
    {
        if (Monster != null) MonsterState.DigestAll(Monster.PlayerId);
    }
}