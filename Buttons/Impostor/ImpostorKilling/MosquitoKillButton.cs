using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Keybinds;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using MiraAPI.Utilities.Assets;
using DivaniMods.Modifiers.Game.Alliance;
using DivaniMods.Roles.Impostor.ImpostorKilling;
using TownOfUs.Assets;
using TownOfUs.Buttons;
using TownOfUs.Modifiers;
using TownOfUs.Options;
using TownOfUs.Options.Maps;
using TownOfUs.Utilities;
using TownOfUs.Utilities.Appearances;
using UnityEngine;
using TownOfUs.Options.Modifiers.Alliance;

namespace DivaniMods.Buttons.Impostor.ImpostorKilling;

public sealed class MosquitoKillButton : TownOfUsKillRoleButton<MosquitoRole, PlayerControl>, IDiseaseableButton, IKillButton
{
    public override string Name => TranslationController.Instance.GetStringWithDefault(StringNames.KillLabel, "Kill");
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => Palette.ImpostorRed;
    public override float Cooldown => PlayerControl.LocalPlayer.GetKillCooldown();
    public override LoadableAsset<Sprite> Sprite => TouAssets.KillSprite;

    public override bool ZeroIsInfinite { get; set; } = true;

    public void SetDiseasedTimer(float multiplier)
    {
        SetTimer(Cooldown * multiplier);
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            return;
        }

        var player = PlayerControl.LocalPlayer;
        if (player == null)
        {
            return;
        }

        player.RpcCustomMurder(Target, MeetingCheck.OutsideMeeting);

        var sting = CustomButtonSingleton<MosquitoStingButton>.Instance;
        if (sting != null)
        {
            sting.SetTimer(sting.Cooldown);
        }
    }

    public override void ClickHandler()
    {
        if (!CanClick())
        {
            return;
        }

        OnClick();
        Button?.SetDisabled();
        Timer = Cooldown;
    }

    public override bool IsTargetValid(PlayerControl? target)
    {
        if (!base.IsTargetValid(target) || target == null)
        {
            return false;
        }

        var player = PlayerControl.LocalPlayer;
        if (player == null)
        {
            return false;
        }

        if (player.IsImpostorAligned() && target.IsImpostorAligned() &&
            !BetrayerRevealedModifier.AllowsImpostorTarget(player, target))
        {
            return false;
        }


        return true;
    }

    public override PlayerControl? GetTarget()
    {
        var genOpt = OptionGroupSingleton<GeneralOptions>.Instance;
        var saboOpt = OptionGroupSingleton<AdvancedSabotageOptions>.Instance;
        var closePlayer = PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance);

        var includePostors = genOpt.FFAImpostorMode ||
                             (PlayerControl.LocalPlayer.IsLover() &&
                              OptionGroupSingleton<LoversOptions>.Instance.LoverKillTeammates) ||
                             (saboOpt.KillDuringCamoComms &&
                              closePlayer?.GetAppearanceType() == TownOfUsAppearances.Camouflage);

        if (!OptionGroupSingleton<LoversOptions>.Instance.LoversKillEachOther && PlayerControl.LocalPlayer.IsLover())
        {
            return PlayerControl.LocalPlayer.GetClosestLivingPlayer(includePostors, Distance, false,
                x => !x.IsLover());
        }

        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(includePostors, Distance, false);
    }
}
