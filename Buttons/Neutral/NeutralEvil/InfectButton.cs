using System.Linq;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using DivaniMods.Assets;
using DivaniMods.Options;
using DivaniMods.Roles.Neutral.NeutralEvil;
using TownOfUs.Buttons;
using UnityEngine;

namespace DivaniMods.Buttons.Neutral.NeutralEvil;

public class InfectButton : TownOfUsTargetButton<PlayerControl>
{
    public override string Name => "Infect";
    public override float Cooldown => OptionGroupSingleton<PlagueDoctorOptions>.Instance.InfectCooldown;
    public override float EffectDuration => 0f;
    public override int MaxUses => (int)OptionGroupSingleton<PlagueDoctorOptions>.Instance.MaxInfections;
    public override LoadableAsset<Sprite> Sprite => DivaniAssets.PlagueDoctorInfectButton;
    public override float Distance => 1.5f;
    public override ButtonLocation Location { get; set; } = ButtonLocation.BottomRight;
    public override Color TextOutlineColor => PlagueDoctorRole.PlagueDoctorColor;
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;

    public override bool Enabled(RoleBehaviour? role)
    {
        return role is PlagueDoctorRole;
    }

    public override PlayerControl? GetTarget()
    {
        var localPlayer = PlayerControl.LocalPlayer;
        if (localPlayer == null) return null;

        var plagueDoctor = localPlayer.Data?.Role as PlagueDoctorRole;
        if (plagueDoctor == null) return null;

        var closest = localPlayer.GetClosestPlayer(true, Distance, true);
        
        if (closest != null && PlagueDoctorRole.InfectedPlayers.ContainsKey(closest.PlayerId))
        {
            return null;
        }

        return closest;
    }

    public override void SetOutline(bool active)
    {
        if (Target == null) return;
        Target.cosmetics.SetOutline(active, new Il2CppSystem.Nullable<Color>(PlagueDoctorRole.PlagueDoctorColor));
    }

    public override bool IsTargetValid(PlayerControl? target)
    {
        if (target == null) return false;
        if (target.Data == null || target.Data.IsDead) return false;
        if (target == PlayerControl.LocalPlayer) return false;

        var plagueDoctor = PlayerControl.LocalPlayer?.Data?.Role as PlagueDoctorRole;
        if (plagueDoctor == null) return false;

        if (PlagueDoctorRole.InfectedPlayers.ContainsKey(target.PlayerId))
        {
            return false;
        }

        return true;
    }

    public override bool CanUse()
    {
        var player = PlayerControl.LocalPlayer;
        if (player == null || player.Data == null || player.Data.IsDead) return false;

        if (player.Data.Role is not PlagueDoctorRole) return false;

        SetUses(PlagueDoctorRole.NumInfectionsRemaining);

        if (PlagueDoctorRole.NumInfectionsRemaining <= 0) return false;

        return base.CanUse();
    }

    protected override void OnClick()
    {
        var player = PlayerControl.LocalPlayer;
        if (player == null || Target == null) return;

        if (player.Data?.Role is not PlagueDoctorRole) return;

        if (PlagueDoctorRole.NumInfectionsRemaining <= 0)
        {
            return;
        }

        if (PlagueDoctorRole.InfectedPlayers.ContainsKey(Target.PlayerId))
        {
            return;
        }

        PlagueDoctorRole.RpcSetInfected(player, Target.PlayerId);
        PlagueDoctorRole.NumInfectionsRemaining--;

        PlayInfectSound();
        ResetTarget();
    }

    private static void PlayInfectSound()
    {
        if (SoundManager.Instance == null) return;
        try
        {
            var clip = DivaniAssets.InfectSound.LoadAsset();
            if (clip == null) return;
            SoundManager.Instance.PlaySound(clip, false, 1f);
        }
        catch (System.Exception ex)
        {
            DivaniPlugin.Instance.Log.LogWarning($"PlagueDoctor: infect sfx failed: {ex.Message}");
        }
    }
}
