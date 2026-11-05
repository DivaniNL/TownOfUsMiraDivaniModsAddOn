using System.Linq;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Translation;
using MiraAPI.Utilities.Assets;
using DivaniMods.Assets;
using DivaniMods.Networking.Crewmate.CrewmateProtective;
using DivaniMods.Options;
using DivaniMods.Roles.Crewmate.CrewmateProtective;
using TownOfUs.Buttons;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Neutral;
using UnityEngine;

namespace DivaniMods.Buttons.Crewmate.CrewmateProtective;

public class PlaceDomeButton : TownOfUsButton
{
    private static string ActiveDomeLabel => MiraLocaleManager.Get("DivaniMods.Role.Domesmith.Button.DomeActive");
    public override string Name => MiraLocaleManager.Get("DivaniMods.Role.Domesmith.Ability.PlaceDome");
    public override float Cooldown => OptionGroupSingleton<DomesmithOptions>.Instance.PlaceDomeCooldown.Value;
    public override float EffectDuration => OptionGroupSingleton<DomesmithOptions>.Instance.PlaceDomeDuration.Value;
    public override int MaxUses => (int)OptionGroupSingleton<DomesmithOptions>.Instance.Charges.Value;
    public override LoadableAsset<Sprite> Sprite => DivaniAssets.DomesmithPlaceDomeButton;
    public override ButtonLocation Location { get; set; } = ButtonLocation.BottomRight;
    public override Color TextOutlineColor => DomesmithRole.DomesmithColor;
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;

    public static PlaceDomeButton? Instance { get; private set; }

    private bool _placingDome;
    private bool _domeTimerOnButton;
    private Vector3 _capturedPosition;

    public override bool Enabled(RoleBehaviour? role)
    {
        Instance = this;
        return role is DomesmithRole;
    }

    public void ResetPlacing()
    {
        _placingDome = false;
    }

    public override bool CanUse()
    {
        var player = PlayerControl.LocalPlayer;
        if (player == null || player.Data == null || player.Data.IsDead)
        {
            return false;
        }

        if (MeetingHud.Instance || ExileController.Instance)
        {
            return false;
        }

        if (PlayerTask.PlayerHasTaskOfType<IHudOverrideTask>(player))
        {
            return false;
        }

        if (EffectActive && !_domeTimerOnButton)
        {
            return false;
        }

        return base.CanUse();
    }

    public override void ClickHandler()
    {
        if (!CanClick() || PlayerControl.LocalPlayer.HasModifier<GlitchHackedModifier>() ||
            PlayerControl.LocalPlayer.GetModifiers<DisabledModifier>().Any(x => !x.CanUseAbilities))
        {
            return;
        }

        if (LimitedUses)
        {
            UsesLeft--;
            Button?.SetUsesRemaining(UsesLeft);
        }

        OnClick();
    }

    protected override void OnClick()
    {
        var player = PlayerControl.LocalPlayer;
        if (player == null || EffectActive)
        {
            return;
        }

        _capturedPosition = player.transform.position;
        _placingDome = true;

        var colorHex = ColorUtility.ToHtmlStringRGB(DomesmithRole.DomesmithColor);
        var delay = EffectDuration;
        var placingText = MiraLocaleManager
            .Get("DivaniMods.Role.Domesmith.Notification.PlacingDome")
            .Replace("<seconds>", delay.ToString("0.#"));

        MiraAPI.Utilities.Helpers.CreateAndShowNotification(
            $"<b><color=#{colorHex}>{placingText}</color></b>",
            Color.white,
            new Vector3(0f, 1f, -20f),
            spr: DivaniAssets.DomesmithIcon.LoadAsset());

        if (HasEffect)
        {
            EffectActive = true;
            Timer = EffectDuration;
        }
        else
        {
            OnEffectEnd();
        }
    }

    public override void OnEffectEnd()
    {
        if (_domeTimerOnButton)
        {
            return;
        }

        if (_placingDome)
        {
            _placingDome = false;

            var player = PlayerControl.LocalPlayer;
            if (player != null && player.Data != null && !player.Data.IsDead)
            {
                DomesmithRpc.RpcPlaceDome(
                    player,
                    _capturedPosition.x,
                    _capturedPosition.y,
                    _capturedPosition.z);

                var colorHex = ColorUtility.ToHtmlStringRGB(DomesmithRole.DomesmithColor);
                MiraAPI.Utilities.Helpers.CreateAndShowNotification(
                    $"<b><color=#{colorHex}>{MiraLocaleManager.Get("DivaniMods.Role.Domesmith.Notification.DomePlaced")}</color></b>",
                    Color.white,
                    new Vector3(0f, 1f, -20f),
                    spr: DivaniAssets.DomesmithIcon.LoadAsset());
            }
        }

        base.OnEffectEnd();
    }

    public static void SyncDomeTimerFromManager()
    {
        var instance = Instance;
        var local = PlayerControl.LocalPlayer;
        if (instance == null || local?.Data?.Role is not DomesmithRole)
        {
            return;
        }

        var remaining = DomeManager.GetLongestRemainingSeconds(local.PlayerId);
        if (remaining <= 0f)
        {
            instance.ClearDomeTimerDisplay();
            return;
        }

        instance._domeTimerOnButton = true;
        instance.OverrideName(ActiveDomeLabel);
        instance.TimerPaused = true;
        instance.EffectActive = true;
        instance.SetTimer(remaining);
    }

    private void ClearDomeTimerDisplay()
    {
        if (!_domeTimerOnButton)
        {
            return;
        }

        _domeTimerOnButton = false;
        TimerPaused = false;
        EffectActive = false;
        OverrideName(MiraLocaleManager.Get("DivaniMods.Role.Domesmith.Ability.PlaceDome"));
        SetTimer(Cooldown);
    }
}
