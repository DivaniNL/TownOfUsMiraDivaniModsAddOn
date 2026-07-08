using System;
using HarmonyLib;
using MiraAPI.Modifiers;
using TMPro;
using DivaniMods.Assets;
using DivaniMods.Modifiers.Crewmate.CrewmateSupport;
using DivaniMods.Roles.Crewmate.CrewmateSupport;
using TownOfUs;
using TownOfUs.Assets;
using TownOfUs.Patches.Options;
using TownOfUs.Utilities;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace DivaniMods.Patches;

[HarmonyPatch]
public static class RoundChatManager
{
    public enum RoundChat
    {
        Telecom,
        Lover
    }

    public static RoundChat Selected { get; private set; } = RoundChat.Telecom;

    private static GameObject? _switchButton;
    private static TextMeshPro? _statusText;
    private static bool _bgTinted;

    private static bool TelecomActive =>
        PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.Data != null &&
        !PlayerControl.LocalPlayer.Data.IsDead &&
        PlayerControl.LocalPlayer.HasModifier<TelecomChatModifier>();

    private static bool LoverActive =>
        PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.Data != null &&
        !PlayerControl.LocalPlayer.Data.IsDead && PlayerControl.LocalPlayer.IsLover();

    public static bool ShouldSendLover => LoverActive && Selected == RoundChat.Lover;

    public static void HideSwitchButton()
    {
        if (_switchButton != null)
        {
            _switchButton.SetActive(false);
        }
    }

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    [HarmonyPostfix]
    public static void HudUpdatePostfix()
    {
        if (!HudManager.InstanceExists)
        {
            return;
        }

        SelfHealTelecomLink();

        var telecom = TelecomActive;
        var lover = LoverActive;

        if (MeetingHud.Instance)
        {
            HideSwitchButton();
            UpdateStatusText(false, false);
            return;
        }

        if (telecom && lover)
        {
            EnsureSwitchButton();
        }
        else
        {
            HideSwitchButton();
            if (!lover)
            {
                Selected = RoundChat.Telecom;
            }
        }

        if (telecom)
        {
            var chat = HudManager.Instance.Chat;
            if (chat != null && !chat.gameObject.activeSelf)
            {
                chat.gameObject.SetActive(true);
                chat.SetVisible(true);
            }

            ApplyMainChatSprite();

            if (chat != null && chat.IsOpenOrOpening && TeamChatPatches.PrivateChatDot != null)
            {
                TeamChatPatches.PrivateChatDot.enabled = false;
            }
        }

        UpdateStatusText(telecom, lover);
        UpdateChatBackground(telecom, lover);
    }

    private static Color ActiveChatColor(bool telecom, bool lover)
    {
        if (lover && (!telecom || Selected == RoundChat.Lover))
        {
            return TownOfUsColors.Lover;
        }

        return TelecomRole.TelecomColor;
    }

    private static void UpdateChatBackground(bool telecom, bool lover)
    {
        var container = GameObject.Find("ChatScreenContainer");
        var background = container?.transform.FindChild("Background");
        if (background == null)
        {
            return;
        }

        var sr = background.GetComponent<SpriteRenderer>();
        var chat = HudManager.Instance.Chat;
        var active = !MeetingHud.Instance && chat != null && chat.IsOpenOrOpening && (telecom || lover);

        if (active)
        {
            var c = ActiveChatColor(telecom, lover);
            sr.color = new Color(c.r * 0.35f, c.g * 0.35f, c.b * 0.35f, 0.85f);
            _bgTinted = true;
        }
        else if (_bgTinted)
        {
            sr.color = Color.white;
            _bgTinted = false;
        }
    }

    private static void SelfHealTelecomLink()
    {
        var lp = PlayerControl.LocalPlayer;
        if (lp == null || lp.Data == null || lp.Data.IsDead || MeetingHud.Instance)
        {
            return;
        }

        if (lp.Data.Role is not TelecomRole role || role.TargetId == byte.MaxValue || lp.HasModifier<TelecomChatModifier>())
        {
            return;
        }

        var target = GameData.Instance.GetPlayerById(role.TargetId)?.Object;
        if (target != null && target.Data != null && !target.Data.IsDead && !target.Data.Disconnected)
        {
            TelecomRole.RpcSetTransmission(lp, role.TargetId);
        }
    }

    private static void ApplyMainChatSprite()
    {
        var chatButton = HudManager.Instance.Chat.chatButton.transform;

        Sprite idle, hover, open;
        if (Selected == RoundChat.Lover)
        {
            idle = TouChatAssets.LoveChatIdle.LoadAsset();
            hover = TouChatAssets.LoveChatHover.LoadAsset();
            open = TouChatAssets.LoveChatOpen.LoadAsset();
        }
        else
        {
            idle = DivaniAssets.TelecomChatIdle.LoadAsset();
            hover = DivaniAssets.TelecomChatHover.LoadAsset();
            open = DivaniAssets.TelecomChatOpen.LoadAsset();
        }

        chatButton.Find("Inactive").GetComponent<SpriteRenderer>().sprite = idle;
        chatButton.Find("Active").GetComponent<SpriteRenderer>().sprite = hover;
        chatButton.Find("Selected").GetComponent<SpriteRenderer>().sprite = open;
    }

    private static void UpdateStatusText(bool telecom, bool lover)
    {
        var chat = HudManager.Instance.Chat;
        if (chat == null)
        {
            return;
        }

        if (_statusText == null)
        {
            _statusText = Object.Instantiate(chat.sendRateMessageText, chat.sendRateMessageText.transform.parent);
            _statusText.gameObject.SetActive(true);
        }

        if (MeetingHud.Instance || !chat.IsOpenOrOpening || (!telecom && !lover))
        {
            _statusText.text = string.Empty;
            return;
        }

        if (telecom && lover)
        {
            var loverSelected = Selected == RoundChat.Lover;
            _statusText.text = loverSelected
                ? "Lover Chat is Active. Press the switch button to cycle."
                : "Telecom Chat is Active. Press the switch button to cycle.";
            _statusText.color = loverSelected ? TownOfUsColors.Lover : TelecomRole.TelecomColor;
        }
        else if (telecom)
        {
            _statusText.text = "Telecom Chat is Active. Messages will be sent to this chat.";
            _statusText.color = TelecomRole.TelecomColor;
        }
        else
        {
            _statusText.text = "Lover Chat is Active. Messages will be sent to this chat.";
            _statusText.color = TownOfUsColors.Lover;
        }
    }

    private static void EnsureSwitchButton()
    {
        if (_switchButton != null)
        {
            _switchButton.SetActive(true);
            return;
        }

        var container = GameObject.Find("ChatScreenContainer");
        var banMenu = container?.transform.FindChild("BanMenuButton");
        if (banMenu == null)
        {
            return;
        }

        _switchButton = Object.Instantiate(banMenu.gameObject, banMenu.transform.parent);
        var passive = _switchButton.GetComponent<PassiveButton>();
        passive.OnClick = new Button.ButtonClickedEvent();
        passive.OnClick.AddListener(new Action(CycleChat));
        _switchButton.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = TouAssets.TeamChatSwitch.LoadAsset();
        _switchButton.name = "RoundChatSwitch";
        var pos = banMenu.transform.localPosition;
        _switchButton.transform.localPosition = new Vector3(pos.x, pos.y + 1.4f, pos.z);
    }

    private static void CycleChat()
    {
        Selected = Selected == RoundChat.Telecom ? RoundChat.Lover : RoundChat.Telecom;
        ApplyMainChatSprite();
    }
}
