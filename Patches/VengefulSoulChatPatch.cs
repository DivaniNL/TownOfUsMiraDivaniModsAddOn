using HarmonyLib;
using DivaniMods.Roles.Crewmate.CrewmateAfterlife;
using UnityEngine;

namespace DivaniMods.Patches;

[HarmonyPatch]
internal static class VengefulSoulChatPatch
{
    private static bool _hidden;
    private static bool _muted;
    private static AudioClip? _savedSound;

    private static AudioClip? _silentClip;
    private static AudioClip SilentClip => _silentClip ??= AudioClip.Create("VengefulSoulSilent", 1, 1, 44100, false);

    private static bool Silenced =>
        PlayerControl.LocalPlayer?.Data?.Role is VengefulSoulRole soul && soul.GhostActive;

    [HarmonyPriority(Priority.First)]
    [HarmonyPrefix]
    [HarmonyPatch(typeof(ChatController), nameof(ChatController.AddChat))]
    [HarmonyPatch(typeof(ChatController), nameof(ChatController.AddChatNote))]
    [HarmonyPatch(typeof(ChatController), nameof(ChatController.SendChat))]
    public static bool BlockChat()
    {
        return !Silenced;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    public static void HudUpdatePostfix(HudManager __instance)
    {
        var chat = __instance.Chat;
        if (chat == null)
        {
            return;
        }

        if (Silenced)
        {
            if (!_muted && chat.messageSound != SilentClip)
            {
                _savedSound = chat.messageSound;
                chat.messageSound = SilentClip;
                _muted = true;
            }

            if (chat.IsOpenOrOpening)
            {
                chat.Close();
            }

            if (chat.gameObject.activeSelf)
            {
                chat.SetVisible(false);
                chat.gameObject.SetActive(false);
            }

            _hidden = true;
        }
        else
        {
            if (_muted)
            {
                if (_savedSound != null)
                {
                    chat.messageSound = _savedSound;
                }

                _savedSound = null;
                _muted = false;
            }

            if (_hidden)
            {
                if (PlayerControl.LocalPlayer?.Data?.IsDead == true && !MeetingHud.Instance)
                {
                    chat.gameObject.SetActive(true);
                    chat.SetVisible(true);
                }

                _hidden = false;
            }
        }
    }
}
