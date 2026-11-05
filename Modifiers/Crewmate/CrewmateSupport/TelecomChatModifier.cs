using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using DivaniMods.Assets;
using TownOfUs.Patches.Options;
using UnityEngine;

namespace DivaniMods.Modifiers.Crewmate.CrewmateSupport;

public sealed class TelecomChatModifier : BaseModifier
{
    public override string ModifierName => "Telecom Transmission";
    public override LoadableAsset<Sprite>? ModifierIcon => DivaniAssets.TelecomIcon;
    public override bool HideOnUi => true;

    [HideFromIl2Cpp] public PlayerControl? Partner { get; set; }

    public bool AmTelecom { get; set; }

    public override void OnActivate()
    {
        base.OnActivate();

        if (Player == null || !Player.AmOwner)
        {
            return;
        }

        ShowTelecomChat();
    }

    public override void OnDeactivate()
    {
        base.OnDeactivate();

        if (Player == null || !Player.AmOwner)
        {
            return;
        }

        if (!MeetingHud.Instance && HudManager.InstanceExists)
        {
            HudManager.Instance.Chat.SetVisible(false);
            HudManager.Instance.Chat.gameObject.SetActive(false);
        }

        TeamChatPatches.ForceNormalChat();
    }

    public static void ShowTelecomChat()
    {
        if (!HudManager.InstanceExists)
        {
            return;
        }

        HudManager.Instance.Chat.gameObject.SetActive(true);
        HudManager.Instance.Chat.SetVisible(true);

        if (!MeetingHud.Instance)
        {
            ApplyChatButtonSprites();
        }
    }

    public static void ApplyChatButtonSprites()
    {
        if (!HudManager.InstanceExists)
        {
            return;
        }

        var chatButton = HudManager.Instance.Chat.chatButton.transform;
        chatButton.Find("Inactive").GetComponent<SpriteRenderer>().sprite = DivaniAssets.TelecomChatIdle.LoadAsset();
        chatButton.Find("Active").GetComponent<SpriteRenderer>().sprite = DivaniAssets.TelecomChatHover.LoadAsset();
        chatButton.Find("Selected").GetComponent<SpriteRenderer>().sprite = DivaniAssets.TelecomChatOpen.LoadAsset();
    }
}
