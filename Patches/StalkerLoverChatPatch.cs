using System.Linq;
using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using Reactor.Utilities.Extensions;
using TMPro;
using DivaniMods.Options;
using DivaniMods.Roles;
using TownOfUs;
using TownOfUs.Assets;
using TownOfUs.Modifiers.Game.Alliance;
using TownOfUs.Patches.Options;
using TownOfUs.Utilities;
using UnityEngine;

namespace DivaniMods.Patches;

/// <summary>
/// Mirrors every Lover chat message into the Stalker's chat with the sender's
/// name, cosmetic, and attribution scrubbed so the Stalker knows that the
/// Lovers are talking but cannot tell who they are.
/// </summary>
[HarmonyPatch(typeof(TeamChatPatches), nameof(TeamChatPatches.RpcSendLoveChat))]
public static class StalkerLoverChatPatch
{
    [HarmonyPostfix]
    public static void Postfix(PlayerControl player, string text)
    {
        try
        {
            if (LobbyBehaviour.Instance || !HudManager.InstanceExists || HudManager.Instance?.Chat == null)
            {
                return;
            }

            var local = PlayerControl.LocalPlayer;
            if (local == null || local.Data == null)
            {
                return;
            }

            if (local.Data.Role is not StalkerRole)
            {
                return;
            }

            if (!OptionGroupSingleton<StalkerOptions>.Instance.CanReadLoverChat)
            {
                return;
            }

            // Don't echo a message the Stalker can't read anyway (dead lovers path
            // already handles death-chat via TheDeadKnow on the vanilla path).
            if (local.Data.IsDead)
            {
                return;
            }

            // Use the Stalker's own data as the bubble cosmetic so the Lover's
            // outfit/color never appears on-screen. Name is anonymized.
            var title = $"<color=#{TownOfUsColors.Lover.ToHtmlStringRGBA()}>??? (Lover)</color>";
            MiscUtils.AddTeamChat(
                local.Data,
                title,
                text,
                blackoutText: false,
                bubbleType: BubbleType.Lover,
                onLeft: true);
        }
        catch (System.Exception ex)
        {
            DivaniPlugin.Instance.Log.LogWarning($"StalkerLoverChatPatch failed: {ex.Message}");
        }
    }
}

/// <summary>
/// Blocks an alive Stalker from typing into the public/Lover chat during the
/// round. They get read-only access to the anonymised Lover chat mirror
/// (similar to how dead players can read but not send in their team chats),
/// so we swallow SendChat before Mira's LoverChatPatch/vanilla code runs.
/// Priority = Priority.First so we run before LoverChatPatches.SendChatPatch.
/// </summary>
[HarmonyPatch(typeof(ChatController), nameof(ChatController.SendChat))]
public static class StalkerBlockSendChatPatch
{
    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    public static bool Prefix(ChatController __instance)
    {
        try
        {
            if (MeetingHud.Instance || ExileController.Instance != null)
            {
                return true;
            }

            var local = PlayerControl.LocalPlayer;
            if (local == null || local.Data == null || local.Data.IsDead)
            {
                return true;
            }

            if (local.Data.Role is not StalkerRole)
            {
                return true;
            }

            // Silently drop the input - identical to LoverChatPatches' early-exit
            // flow for consistency, just without any send.
            __instance.freeChatField.Clear();
            __instance.quickChatMenu.Clear();
            __instance.quickChatField.Clear();
            __instance.UpdateChatMode();
            return false;
        }
        catch (System.Exception ex)
        {
            DivaniPlugin.Instance.Log.LogWarning($"StalkerBlockSendChatPatch failed: {ex.Message}");
            return true;
        }
    }
}

/// <summary>
/// Vanilla Among Us hides the chat button for alive players during the round.
/// The Lover modifier forcibly re-enables the chat GameObject and swaps the
/// chat button to the heart-themed sprites in its OnActivate / OnRoundStart
/// hooks. We mirror that flow for the Stalker so they can actually open the
/// chat window and see the mirrored (anonymized) Lover messages.
/// </summary>
public static class StalkerChatVisibility
{
    [RegisterEvent]
    public static void RoundStartHandler(RoundStartEvent @event)
    {
        ApplyLoverChatUi();
    }

    /// <summary>
    /// Enables the chat window during round and paints the chat button with
    /// the Lover heart sprites. No-ops if the local player isn't an alive
    /// Stalker with "Anonymous Lover Chat Access" enabled.
    /// </summary>
    public static void ApplyLoverChatUi()
    {
        try
        {
            if (!HudManager.InstanceExists || HudManager.Instance?.Chat == null)
            {
                return;
            }

            var local = PlayerControl.LocalPlayer;
            if (local == null || local.Data == null || local.Data.IsDead)
            {
                return;
            }

            if (local.Data.Role is not StalkerRole)
            {
                return;
            }

            if (!OptionGroupSingleton<StalkerOptions>.Instance.CanReadLoverChat)
            {
                return;
            }

            var chat = HudManager.Instance.Chat;
            chat.gameObject.SetActive(true);
            chat.SetVisible(true);

            var idle = TouChatAssets.LoveChatIdle.LoadAsset();
            var hover = TouChatAssets.LoveChatHover.LoadAsset();
            var selected = TouChatAssets.LoveChatOpen.LoadAsset();

            var inactive = chat.chatButton.transform.Find("Inactive")?.GetComponent<SpriteRenderer>();
            var active = chat.chatButton.transform.Find("Active")?.GetComponent<SpriteRenderer>();
            var sel = chat.chatButton.transform.Find("Selected")?.GetComponent<SpriteRenderer>();
            if (inactive != null) inactive.sprite = idle;
            if (active != null) active.sprite = hover;
            if (sel != null) sel.sprite = selected;
        }
        catch (System.Exception ex)
        {
            DivaniPlugin.Instance.Log.LogWarning($"StalkerChatVisibility.ApplyLoverChatUi failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Revert the chat button to the plain sprites (used on meeting start so
    /// the Stalker sees normal meeting chat UI just like everyone else).
    /// </summary>
    public static void ApplyNormalChatUi()
    {
        try
        {
            if (!HudManager.InstanceExists || HudManager.Instance?.Chat == null)
            {
                return;
            }

            var chat = HudManager.Instance.Chat;

            var idle = TouChatAssets.NormalChatIdle.LoadAsset();
            var hover = TouChatAssets.NormalChatHover.LoadAsset();
            var selected = TouChatAssets.NormalChatOpen.LoadAsset();

            var inactive = chat.chatButton.transform.Find("Inactive")?.GetComponent<SpriteRenderer>();
            var active = chat.chatButton.transform.Find("Active")?.GetComponent<SpriteRenderer>();
            var sel = chat.chatButton.transform.Find("Selected")?.GetComponent<SpriteRenderer>();
            if (inactive != null) inactive.sprite = idle;
            if (active != null) active.sprite = hover;
            if (sel != null) sel.sprite = selected;
        }
        catch (System.Exception ex)
        {
            DivaniPlugin.Instance.Log.LogWarning($"StalkerChatVisibility.ApplyNormalChatUi failed: {ex.Message}");
        }
    }
}
