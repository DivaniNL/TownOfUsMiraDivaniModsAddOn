using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using DivaniMods.Assets;
using DivaniMods.Networking.Impostor.ImpostorKilling;
using DivaniMods.Options;
using DivaniMods.Roles.Impostor.ImpostorKilling;
using TownOfUs.Buttons;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Utilities;
using UnityEngine;

namespace DivaniMods.Buttons.Impostor.ImpostorKilling;

public sealed class MosquitoStingButton : TownOfUsButton, IDiseaseableButton
{
    public override string Name => "Sting";
    public override float Cooldown => OptionGroupSingleton<MosquitoOptions>.Instance.StingCooldown.Value;
    public override float EffectDuration => 0f;
    public override int MaxUses => (int)OptionGroupSingleton<MosquitoOptions>.Instance.StingCharges.Value;
    public override LoadableAsset<Sprite> Sprite => DivaniAssets.MosquitoStingButton;
    public override ButtonLocation Location => ButtonLocation.BottomRight;
    public override Color TextOutlineColor => Palette.ImpostorRed;
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;

    public static MosquitoStingButton? Instance { get; private set; }
    public static int ChargesPerKill => (int)OptionGroupSingleton<MosquitoOptions>.Instance.ChargesPerKill.Value;

    private int _currentCharges = -1;

    public int CurrentCharges
    {
        get
        {
            if (_currentCharges < 0)
            {
                _currentCharges = MaxUses;
            }
            return _currentCharges;
        }
        set
        {
            _currentCharges = value;
            SetUses(value);
        }
    }

    public override bool Enabled(RoleBehaviour? role)
    {
        Instance = this;
        return role is MosquitoRole;
    }

    public void SetDiseasedTimer(float multiplier)
    {
        SetTimer(Cooldown * multiplier);
    }

    private static MosquitoTargetMode Mode =>
        (MosquitoTargetMode)OptionGroupSingleton<MosquitoOptions>.Instance.TargetMode.Value;

    private static bool IsValidTarget(PlayerControl? plr, PlayerControl me) =>
        plr != null && plr.Data != null && !plr.Data.Disconnected && !plr.HasDied()
        && plr.PlayerId != me.PlayerId && !plr.IsImpostorAligned();

    private static PlayerControl? GetFarthestTarget(PlayerControl player)
    {
        PlayerControl? best = null;
        var bestDistance = -1f;
        var from = player.GetTruePosition();

        foreach (var pc in PlayerControl.AllPlayerControls)
        {
            if (!IsValidTarget(pc, player)) continue;

            var distance = Vector2.Distance(from, pc.GetTruePosition());
            if (distance > bestDistance)
            {
                bestDistance = distance;
                best = pc;
            }
        }

        return best;
    }

    public override bool CanUse()
    {
        var player = PlayerControl.LocalPlayer;
        if (player == null || player.Data == null || player.Data.IsDead) return false;
        if (!base.CanUse()) return false;

        SetUses(CurrentCharges);

        var hasCharges = CurrentCharges > 0 || MaxUses == 0;
        return hasCharges && GetFarthestTarget(player) != null && Timer <= 0;
    }

    public override void ClickHandler()
    {
        if (!CanUse())
        {
            return;
        }

        var player = PlayerControl.LocalPlayer;
        if (player == null || player.HasModifier<GlitchHackedModifier>())
        {
            return;
        }

        if (Mode == MosquitoTargetMode.PlayerSelection)
        {
            if (Minigame.Instance != null)
            {
                return;
            }

            OpenTargetMenu(player);
            return;
        }

        OnClick();
        Timer = Cooldown;
    }

    protected override void OnClick()
    {
        // Furthest mode — player-selection fires from the menu callback.
        var player = PlayerControl.LocalPlayer;
        if (player == null) return;

        var target = GetFarthestTarget(player);
        if (target == null) return;

        FireAndConsume(player, target);
    }

    public void AddCharges(int amount)
    {
        if (amount > 0)
        {
            CurrentCharges += amount;
        }
    }

    public void ResetCharges()
    {
        _currentCharges = -1;
    }

    private void FireAndConsume(PlayerControl shooter, PlayerControl target)
    {
        Fire(shooter, target);

        if (MaxUses > 0)
        {
            CurrentCharges--;
        }
    }

    private static void Fire(PlayerControl shooter, PlayerControl target)
    {
        var aimbot = OptionGroupSingleton<MosquitoOptions>.Instance.AimbotMode.Value;
        var dest = target.GetTruePosition();
        MosquitoRpc.RpcSpawnMosquito(shooter, target.PlayerId, dest.x, dest.y, aimbot);

        // Sync with the kill button: launching a sting also puts the stab on cooldown.
        var killButton = CustomButtonSingleton<MosquitoKillButton>.Instance;
        if (killButton != null)
        {
            killButton.SetTimer(killButton.Cooldown);
        }
        shooter.SetKillTimer(killButton?.Cooldown ?? shooter.GetKillCooldown());
    }

    private void OpenTargetMenu(PlayerControl player)
    {
        var menu = CustomPlayerMenu.Create();
        menu.transform.FindChild("PhoneUI").GetChild(0).GetComponent<SpriteRenderer>().material =
            player.cosmetics.currentBodySprite.BodySprite.material;
        menu.transform.FindChild("PhoneUI").GetChild(1).GetComponent<SpriteRenderer>().material =
            player.cosmetics.currentBodySprite.BodySprite.material;

        menu.Begin(
            plr => IsValidTarget(plr, player),
            plr =>
            {
                menu.ForceClose();

                if (plr == null || !IsValidTarget(plr, player))
                {
                    return;
                }

                FireAndConsume(player, plr);
                Timer = Cooldown;
            });
    }
}
