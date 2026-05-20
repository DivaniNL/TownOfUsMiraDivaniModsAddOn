using System.Collections;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using DivaniMods.Assets;
using DivaniMods.Options;
using DivaniMods.Patches;
using DivaniMods.Roles.Neutral.NeutralEvil;
using TownOfUs.Buttons;
using UnityEngine;

namespace DivaniMods.Buttons.Neutral.NeutralEvil;

public class DemolitionistDefuseButton : TownOfUsButton
{
    public override string Name => "Defuse";
    public override float Cooldown => 1f;
    public override float EffectDuration => OptionGroupSingleton<DemolitionistOptions>.Instance.IsTimedSabotageStyle
        ? OptionGroupSingleton<DemolitionistOptions>.Instance.DefuseTime.Value
        : 0f;
    public override int MaxUses => 0;
    // TownOfUsButton defaults ZeroIsInfinite to false; without this MaxUses=0 reads as "0 uses left"
    // and CanUse is always false (button permanently greyed). True = unlimited uses.
    public override bool ZeroIsInfinite { get; set; } = true;
    public override LoadableAsset<Sprite> Sprite => DivaniAssets.DemolitionistSabotageButton;
    public override ButtonLocation Location { get; set; } = ButtonLocation.BottomLeft;
    public override Color TextOutlineColor => DemolitionistRole.DemolitionistColor;
    public override BaseKeybind Keybind => Keybinds.TertiaryAction;

    public static DemolitionistDefuseButton? Instance { get; set; }

    private bool _isDefusing;

    public static bool IsLocalDefusing =>
        (Instance != null && Instance._isDefusing)
        || DemolitionistNumpad.Controller.DefuseInProgress;

    public override bool Enabled(RoleBehaviour? role)
    {
        Instance = this;
        return role != null;
    }

    public override bool CanUse()
    {
        var player = PlayerControl.LocalPlayer;
        if (player == null || player.Data == null || player.Data.IsDead) return false;
        if (MeetingHud.Instance || ExileController.Instance) return false;
        if (!DemolitionistSabotageState.IsActive) return false;
        if (DemolitionistNumpad.Controller.InProgress) return false;
        if (_isDefusing) return false;
        if (!DemolitionistSabotageState.IsLocalPlayerAtPlantedConsole()) return false;

        return base.CanUse();
    }

    public override void ClickHandler()
    {
        if (!CanClick())
        {
            return;
        }

        OnClick();
        Button?.SetDisabled();
    }

    protected override void OnClick()
    {
        var player = PlayerControl.LocalPlayer;
        if (player == null) return;
        if (!DemolitionistSabotageState.IsActive) return;
        if (!DemolitionistSabotageState.IsLocalPlayerAtPlantedConsole()) return;
        if (_isDefusing) return;

        if (!OptionGroupSingleton<DemolitionistOptions>.Instance.IsTimedSabotageStyle)
        {
            Coroutines.Start(DefuseNumpadCoroutine(player));
            return;
        }

        Coroutines.Start(DefuseTimedCoroutine(player));
    }

    private IEnumerator DefuseTimedCoroutine(PlayerControl player)
    {
        _isDefusing = true;
        var defuseTime = EffectDuration;
        var colorHex = ColorUtility.ToHtmlStringRGB(DemolitionistRole.DemolitionistColor);

        EffectActive = true;
        Timer = defuseTime;
        Button?.OverrideText("DEFUSING");

        MiraAPI.Utilities.Helpers.CreateAndShowNotification(
            $"<b><color=#{colorHex}>Defusing...</color></b>",
            Color.white,
            new Vector3(0f, 1f, -20f),
            spr: DivaniAssets.DemolitionistSabotageButton.LoadAsset());

        var elapsed = 0f;
        while (elapsed < defuseTime)
        {
            if (player == null || player.Data == null || player.Data.IsDead)
            {
                AbortDefuse();
                yield break;
            }

            if (!DemolitionistSabotageState.IsActive)
            {
                AbortDefuse();
                yield break;
            }

            if (!DemolitionistSabotageState.IsLocalPlayerAtPlantedConsole())
            {
                MiraAPI.Utilities.Helpers.CreateAndShowNotification(
                    $"<b><color=#{colorHex}>Defuse aborted — too far from sabotage!</color></b>",
                    Color.white,
                    new Vector3(0f, 1f, -20f),
                    spr: DivaniAssets.DemolitionistSabotageButton.LoadAsset());
                AbortDefuse();
                yield break;
            }

            elapsed += Time.deltaTime;
            Timer = defuseTime - elapsed;
            if (Button != null)
            {
                Button.SetFillUp(Timer, defuseTime);
            }

            yield return null;
        }

        if (player == null || player.Data == null || player.Data.IsDead)
        {
            AbortDefuse();
            yield break;
        }

        if (!DemolitionistSabotageState.IsActive)
        {
            AbortDefuse();
            yield break;
        }

        if (!DemolitionistSabotageState.IsLocalPlayerAtPlantedConsole())
        {
            MiraAPI.Utilities.Helpers.CreateAndShowNotification(
                $"<b><color=#{colorHex}>Defuse aborted — too far from sabotage!</color></b>",
                Color.white,
                new Vector3(0f, 1f, -20f),
                spr: DivaniAssets.DemolitionistSabotageButton.LoadAsset());
            AbortDefuse();
            yield break;
        }

        DemolitionistSabotageState.RpcDefuseSabotage(player, player.PlayerId);
        EffectActive = false;
        Timer = Cooldown;
        _isDefusing = false;
        Button?.OverrideText(Name.ToUpperInvariant());
    }

    private IEnumerator DefuseNumpadCoroutine(PlayerControl player)
    {
        // No EffectActive during the keypad: TownOfUsButton would draw the clamped Timer (-1) as text.
        // _isDefusing + the numpad InProgress check keep the button disabled.
        _isDefusing = true;

        if (!DemolitionistNumpad.Controller.OpenDefuse(player))
        {
            AbortDefuse();
            yield break;
        }

        // Same as plant: IsLocalPlayerAtPlantedConsole uses CouldUse paths that fail while KeypadGame is open.
        while (DemolitionistNumpad.Controller.InProgress)
        {
            if (player == null || player.Data == null || player.Data.IsDead
                || !DemolitionistSabotageState.IsActive)
            {
                AbortDefuse();
                yield break;
            }

            yield return null;
        }

        EffectActive = false;
        Timer = Cooldown;
        _isDefusing = false;
    }

    // TownOfUsButton.FixedUpdate force-activates the button every frame (based on Use/Pet button),
    // which would override UpdateDefuseButton's proximity hiding and leave the defuse button
    // occupying a HUD slot when it should be gone. Empty override = visibility is solely the patch's.
    protected override void FixedUpdate(PlayerControl playerControl)
    {
    }

    private void AbortDefuse()
    {
        if (DemolitionistNumpad.Controller.InProgress)
        {
            DemolitionistNumpad.Controller.CancelActive();
        }

        EffectActive = false;
        Timer = Cooldown;
        _isDefusing = false;
        Button?.OverrideText(Name.ToUpperInvariant());
    }
}
