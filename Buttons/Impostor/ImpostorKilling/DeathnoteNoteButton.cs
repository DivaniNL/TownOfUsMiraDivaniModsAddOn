using DivaniMods.Assets;
using DivaniMods.Modifiers.Impostors;
using DivaniMods.Options;
using DivaniMods.Roles.Impostor.ImpostorKilling;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using TownOfUs.Assets;
using TownOfUs.Buttons;
using TownOfUs.Utilities;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class DeathnoteButton : TownOfUsRoleButton<DeathnoteRole, PlayerControl>
{
    public override float Distance => 1.5f;
    public override string Name => MiraLocaleManager.Get("DivaniMods.Role.Deathnote.Ability.Note", "Note");
    public override float Cooldown => OptionGroupSingleton<DeathnoteOptions>.Instance.NoteCooldown.Value;
    public override LoadableAsset<Sprite> Sprite => DivaniAssets.NoteButton;
    public override bool PauseTimerInVent => true;
    public override bool UsableFirstRound => OptionGroupSingleton<DeathnoteOptions>.Instance.CanNoteFirstRound;
    public override ButtonLocation Location => ButtonLocation.BottomRight;
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => Palette.ImpostorRed;
    public override bool ZeroIsInfinite { get; set; } = true;

    public override int MaxUses => (int)OptionGroupSingleton<DeathnoteOptions>.Instance.MaxNotesPerGame.Value;
    public int CurrentCharges => UsesLeft;
    public int ExtraUses { get; set; }

    private static int StartingCharges => (int)OptionGroupSingleton<DeathnoteOptions>.Instance.StartingCharges.Value;

    private void ApplyUses(int amount)
    {
        if (Button == null)
        {
            UsesLeft = Mathf.Max(0, amount);
            return;
        }

        SetUses(amount);
        Button.usesRemainingText.gameObject.SetActive(true);
        Button.usesRemainingSprite.gameObject.SetActive(true);
    }

    public void AddCharges(int amount)
    {
        if (amount == 0)
        {
            return;
        }

        var cap = MaxUses > 0 ? MaxUses : int.MaxValue;
        ApplyUses(Mathf.Min(cap, UsesLeft + amount));
    }

    public void ResetCharges()
    {
        ExtraUses = 0;
        var start = StartingCharges;
        var cap = MaxUses > 0 ? MaxUses : int.MaxValue;
        ApplyUses(Mathf.Min(cap, start));
    }

    public void AccrueKill()
    {
        var per = (int)OptionGroupSingleton<DeathnoteOptions>.Instance.KillsPerExtraCharge.Value;
        if (per <= 0)
        {
            return;
        }

        ExtraUses++;
        if (ExtraUses >= per)
        {
            ExtraUses = 0;
            AddCharges(1);
        }
    }

    protected override void OnClick()
    {
        if (Target != null && !Target.HasModifier<DeathnoteModifier>() && !Target.IsImpostorAligned())
        {
            Target?.RpcAddModifier<DeathnoteModifier>();
            CheckReset(false);
        }
    }

    public void CheckReset(bool resetSelf)
    {
        var sync = OptionGroupSingleton<DeathnoteOptions>.Instance.DeathnoteCooldownSync;
        if (sync == true)
        {
            if (resetSelf)
            {
                ResetCooldownAndOrEffect();
            }
            else
            {
                PlayerControl.LocalPlayer.SetKillTimer(PlayerControl.LocalPlayer.GetKillCooldown());
            }
        }
    }

    public override bool CanUse()
    {
        var player = PlayerControl.LocalPlayer;
        if (player == null || player.Data == null || player.Data.IsDead)
        {
            return false;
        }

        return base.CanUse();
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
        return target != null && !target.Data.Disconnected && !target.HasModifier<DeathnoteModifier>() && !target.IsImpostorAligned();
    }

    public override bool Enabled(RoleBehaviour? role)
    {
        return role is DeathnoteRole;
    }
}