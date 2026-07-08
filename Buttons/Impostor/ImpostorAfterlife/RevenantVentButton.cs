using System.Linq;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using DivaniMods.Options;
using DivaniMods.Roles.Impostor.ImpostorAfterlife;
using TMPro;
using TownOfUs.Assets;
using TownOfUs.Buttons;
using TownOfUs.Modifiers;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace DivaniMods.Buttons.Impostor.ImpostorAfterlife;

public sealed class RevenantVentButton : TownOfUsRoleButton<RevenantRole, Vent>
{
    public override string Name => "Vent";
    public override BaseKeybind Keybind => Keybinds.VentAction;
    public override Color TextOutlineColor => RevenantRole.RevenantColor;
    public override float Cooldown => OptionGroupSingleton<SummonerOptions>.Instance.RevenantVentCooldown.Value;
    public override LoadableAsset<Sprite> Sprite => TouAssets.VentSprite;
    public override ButtonLocation Location { get; set; } = ButtonLocation.BottomRight;
    public override bool ShouldPauseInVent => false;
    public override bool UsableInDeath => true;

    public override bool Enabled(RoleBehaviour? role)
    {
        return role is RevenantRole { GhostActive: true };
    }

    private float _ventTimeLeft;
    private GameObject? _ventUI;
    private Image? _ventBar;
    private TextMeshProUGUI? _ventText;

    private static float MaxVentTime => OptionGroupSingleton<SummonerOptions>.Instance.RevenantMaxVentTime.Value;

    public override Vent? GetTarget()
    {
        var player = PlayerControl.LocalPlayer;
        if (player == null)
        {
            return null;
        }

        var filter = Helpers.CreateFilter(Constants.Usables);
        var vent = player.GetNearestObjectOfType<Vent>(player.MaxReportDistance, filter);
        if (vent == null || !(player.CanMove || player.inVent))
        {
            return null;
        }

        var center = new Vector2(player.Collider.bounds.center.x, player.Collider.bounds.center.y);
        var position = new Vector2(vent.transform.position.x, vent.transform.position.y);
        if (Vector2.Distance(center, position) > vent.UsableDistance ||
            PhysicsHelpers.AnythingBetween(player.Collider, center, position, Constants.ShipOnlyMask, false))
        {
            return null;
        }

        return vent;
    }

    public override bool CanUse()
    {
        var player = PlayerControl.LocalPlayer;
        if (player?.Data?.Role is not RevenantRole revenant || !revenant.GhostActive)
        {
            return false;
        }

        var newTarget = GetTarget();
        if (newTarget != Target)
        {
            Target?.SetOutline(false, false);
        }

        Target = IsTargetValid(newTarget) ? newTarget : null;
        SetOutline(true);

        if (HudManager.Instance.Chat.IsOpenOrOpening || MeetingHud.Instance)
        {
            return false;
        }

        if (player.GetModifiers<DisabledModifier>().Any(x => !x.CanUseAbilities))
        {
            return false;
        }

        return (Timer <= 0 && !player.inVent && Target != null) || player.inVent;
    }

    public override void ClickHandler()
    {
        if (!CanUse())
        {
            return;
        }

        OnClick();
        Button?.SetDisabled();
    }

    protected override void OnClick()
    {
        var player = PlayerControl.LocalPlayer;
        if (player == null)
        {
            return;
        }

        if (!player.inVent)
        {
            if (Target != null)
            {
                player.MyPhysics.RpcEnterVent(Target.Id);
                Target.SetButtons(true);
                _ventTimeLeft = MaxVentTime;
                Timer = 0.001f;
            }

            return;
        }

        ExitCurrentVent();
        Timer = Cooldown;
    }

    public override void FixedUpdateHandler(PlayerControl playerControl)
    {
        base.FixedUpdateHandler(playerControl);

        var player = PlayerControl.LocalPlayer;
        if (player == null || player.Data?.Role is not RevenantRole)
        {
            HideVentUI();
            return;
        }

        if (!player.inVent)
        {
            _ventTimeLeft = MaxVentTime;
            HideVentUI();
            return;
        }

        _ventTimeLeft -= Time.fixedDeltaTime;
        UpdateVentUI();

        if (_ventTimeLeft <= 0f)
        {
            ExitCurrentVent();
            Timer = Cooldown;
        }
    }

    private void ExitCurrentVent()
    {
        var player = PlayerControl.LocalPlayer;
        HideVentUI();
        if (player == null || !player.inVent || Vent.currentVent == null)
        {
            return;
        }

        Vent.currentVent.SetButtons(false);
        player.MyPhysics.RpcExitVent(Vent.currentVent.Id);
    }

    private void EnsureVentUI()
    {
        if (_ventUI != null)
        {
            return;
        }

        _ventUI = Object.Instantiate(TouAssets.ScatterUI.LoadAsset(), HudManager.Instance.transform);
        _ventUI.transform.localPosition = new Vector3(-3.22f, 2.26f, -10f);

        var canvas = _ventUI.transform.FindChild("ScatterCanvas");
        _ventText = canvas.FindChild("ScatterText").gameObject.GetComponent<TextMeshProUGUI>();
        _ventBar = canvas.FindChild("ScatterBar").gameObject.GetComponent<Image>();

        var icon = canvas.FindChild("ScatterIcon").gameObject.GetComponent<Image>();
        icon.sprite = DivaniMods.Assets.DivaniAssets.RevenantIcon.LoadAsset();
    }

    private void UpdateVentUI()
    {
        EnsureVentUI();
        if (_ventUI == null)
        {
            return;
        }

        var remaining = Mathf.Max(_ventTimeLeft, 0f);
        var rounded = (int)Mathf.Ceil(remaining);

        var color = rounded switch
        {
            > 6 => Color.green,
            > 3 => Color.yellow,
            _ => Color.red,
        };
        var hex = ColorUtility.ToHtmlStringRGB(color);

        if (_ventText != null)
        {
            _ventText.text = $"Vent: <color=#{hex}>{rounded}s</color>";
            _ventText.gameObject.SetActive(true);
        }

        if (_ventBar != null)
        {
            _ventBar.fillAmount = Mathf.Clamp(remaining / MaxVentTime, 0f, 1f);
            _ventBar.color = color;
        }

        _ventUI.SetActive(true);
    }

    private void HideVentUI()
    {
        if (_ventUI != null)
        {
            _ventUI.SetActive(false);
        }
    }
}
