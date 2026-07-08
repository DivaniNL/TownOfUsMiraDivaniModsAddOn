using HarmonyLib;
using MiraAPI.Modifiers;
using Reactor.Utilities.Extensions;
using DivaniMods.Modifiers.Crewmate.CrewmateSupport;
using DivaniMods.Roles.Crewmate.CrewmateSupport;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Utilities;

namespace DivaniMods.Patches;

[HarmonyPatch]
public static class TelecomChatPatch
{
    [HarmonyPatch(typeof(ChatController), nameof(ChatController.SendChat))]
    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    public static bool SendChatPatch(ChatController __instance)
    {
        if (MeetingHud.Instance || ExileController.Instance || PlayerControl.LocalPlayer.Data.IsDead)
        {
            return true;
        }

        var text = __instance.freeChatField.Text.WithoutRichText();

        if (text.Length < 1 || text.Length > 301)
        {
            return true;
        }

        var local = PlayerControl.LocalPlayer;
        if (!local.HasModifier<TelecomChatModifier>())
        {
            return true;
        }

        if (RoundChatManager.ShouldSendLover)
        {
            return true;
        }

        if (local.HasModifier<ParasiteInfectedModifier>() || local.HasModifier<PuppeteerControlModifier>())
        {
            MiscUtils.AddTeamChat(local.Data,
                $"<color=#{TelecomRole.TelecomColor.ToHtmlStringRGBA()}>{local.Data.PlayerName} (Telecom chat)</color>",
                "You are under control! Your message cannot be sent.", blackoutText: false,
                bubbleType: BubbleType.None, onLeft: false);
        }
        else
        {
            TelecomRole.RpcSendTelecomChat(local, text);
        }

        __instance.freeChatField.Clear();
        __instance.quickChatMenu.Clear();
        __instance.quickChatField.Clear();
        __instance.UpdateChatMode();

        return false;
    }
}
