using System.Linq;
using AmongUs.GameOptions;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using DivaniMods.Options;
using TownOfUs.Assets;
using TownOfUs.Options.Modifiers.Crewmate;
using TownOfUs.Roles;
using TownOfUs.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DivaniMods.Modifiers.Crewmate.CrewmateSupport;

public sealed class LocatorMarkModifier : BaseModifier
{
    public override string ModifierName => "Noisemaker";
    public override LoadableAsset<Sprite>? ModifierIcon => TouRoleIcons.Noisemaker;
    public override bool HideOnUi => true;

    public override void OnActivate()
    {
        base.OnActivate();

        if (Player != null && Player.AmOwner && OptionGroupSingleton<LocatorOptions>.Instance.TargetKnows)
        {
            Helpers.CreateAndShowNotification(
                "<b><color=#DDAB99>The Locator marked you as Noisemaker</color></b>",
                Color.white,
                new Vector3(0f, 1f, -20f),
                spr: TouRoleIcons.Noisemaker.LoadAsset());
        }
    }

    private void SoundDynamics(AudioSource source)
    {
        if (!PlayerControl.LocalPlayer)
        {
            source.volume = 0f;
            return;
        }

        source.volume = 1f;
        var truePosition = PlayerControl.LocalPlayer.GetTruePosition();
        source.volume = SoundManager.GetSoundVolume(Player.GetTruePosition(), truePosition, 7f, 50f, 0.5f);
    }

    public void NotifyOfDeath(PlayerControl player)
    {
        if (PlayerControl.LocalPlayer.IsImpostor() &&
            !OptionGroupSingleton<NoisemakerOptions>.Instance.ImpostorsAlerted)
        {
            return;
        }

        if (PlayerControl.LocalPlayer.Is(RoleAlignment.NeutralKilling) &&
            !OptionGroupSingleton<NoisemakerOptions>.Instance.NeutsAlerted)
        {
            return;
        }

        if (PlayerControl.LocalPlayer.AreCommsAffected() &&
            OptionGroupSingleton<NoisemakerOptions>.Instance.CommsAffected)
        {
            return;
        }

        if (Object.FindObjectsOfType<DeadBody>().FirstOrDefault(x => x.ParentId == player.PlayerId) == null &&
            OptionGroupSingleton<NoisemakerOptions>.Instance.BodyCheck)
        {
            return;
        }

        if (Constants.ShouldPlaySfx())
        {
            var audio = SoundManager.Instance.PlaySound(TouAudio.NoisemakerDeathSound.LoadAsset(), false, 1,
                SoundManager.Instance.SfxChannel);
            SoundDynamics(audio);
            VibrationManager.Vibrate(1f, PlayerControl.LocalPlayer.GetTruePosition(), 7f, 1.2f);
        }

        var noise = RoleManager.Instance.GetRole(RoleTypes.Noisemaker).Cast<NoisemakerRole>();
        var deathArrowPrefab =
            Object.Instantiate(noise.deathArrowPrefab, Player.transform.position, Quaternion.identity);

        var deathArrow = deathArrowPrefab.GetComponent<NoisemakerArrow>();
        deathArrow.SetDuration(OptionGroupSingleton<NoisemakerOptions>.Instance.AlertDuration);
        if (Player.AmOwner)
        {
            deathArrow.alwaysMaxSize = true;
        }

        deathArrow.gameObject.SetActive(true);
        deathArrow.target = Player.GetTruePosition();
    }
}
