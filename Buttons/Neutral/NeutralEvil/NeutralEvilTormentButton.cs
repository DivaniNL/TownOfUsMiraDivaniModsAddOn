using MiraAPI.Hud;
using MiraAPI.Modifiers;
using DivaniMods.Interfaces;
using TownOfUs.Assets;
using TownOfUs.Buttons;
using TownOfUs.Modifiers;
using TownOfUs.Networking;
using TownOfUs.Utilities;
using UnityEngine;
using TownOfUs;
using MiraAPI.Utilities.Assets;

namespace DivaniMods.Buttons.Neutral;

public sealed class NeutralEvilTormentButton : TownOfUsButton
{
    public override string Name => "Kill";
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Neutral;
    public override float Cooldown => 0.01f;
    public override LoadableAsset<Sprite> Sprite => TouNeutAssets.ExeTormentSprite;
    public override ButtonLocation Location => ButtonLocation.BottomRight;
    public override bool ShouldPauseInVent => false;
    public override bool UsableInDeath => true;

    public bool Show { get; set; }
    public INeutralEvilWinOutcomeRole? Owner { get; set; }

    public override bool Enabled(RoleBehaviour? role)
    {
        return Show && Owner != null;
    }

    public override bool CanUse()
    {
        if (HudManager.Instance.Chat.IsOpenOrOpening || MeetingHud.Instance)
        {
            return false;
        }

        return Show && Owner != null;
    }

    protected override void OnClick()
    {
        if (Minigame.Instance || Owner == null)
        {
            return;
        }

        var playerMenu = CustomPlayerMenu.Create();
        playerMenu.transform.FindChild("PhoneUI").GetChild(0).GetComponent<SpriteRenderer>().material =
            PlayerControl.LocalPlayer.cosmetics.currentBodySprite.BodySprite.material;
        playerMenu.transform.FindChild("PhoneUI").GetChild(1).GetComponent<SpriteRenderer>().material =
            PlayerControl.LocalPlayer.cosmetics.currentBodySprite.BodySprite.material;

        playerMenu.Begin(
            plr => plr != null && !plr.HasDied() && !plr.AmOwner && !plr.HasModifier<InvulnerabilityModifier>(),
            plr =>
            {
                playerMenu.ForceClose();

                if (plr == null || Owner == null)
                {
                    return;
                }

                PlayerControl.LocalPlayer.RpcGhostRoleMurder(plr);
                Owner.HasKilled = true;
                Owner = null;
                Show = false;
            });
    }
}
