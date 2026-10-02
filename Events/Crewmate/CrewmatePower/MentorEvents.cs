using System.Linq;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using DivaniMods.Modifiers.Crewmate.CrewmatePower;
using DivaniMods.Options;
using DivaniMods.Roles.Crewmate.CrewmatePower;
using TownOfUs.Extensions;
using TownOfUs.Utilities;
using MiraAPI.Utilities;
using TownOfUs.Options;
using MiraAPI.Events.Mira;
using TownOfUs.Buttons;
using MiraAPI.Hud;

namespace DivaniMods.Events.Crewmate.CrewmatePower;

public static class MentorEvents
{
    [RegisterEvent]
    public static void OnRoundStart(RoundStartEvent evt)
    {
        if (evt.TriggeredByIntro || !AmongUsClient.Instance.AmHost)
        {
            return;
        }

        var options = OptionGroupSingleton<MentorOptions>.Instance;

        foreach (var insomniac in ModifierUtils.GetPlayersWithModifier<MentorInsomniaModifier>().ToList())
        {
            var insomniaMod = insomniac.GetModifier<MentorInsomniaModifier>();
            if (insomniaMod == null)
            {
                continue;
            }

            insomniaMod.RoundsLeft--;
            if (insomniaMod.RoundsLeft <= 0)
            {
                insomniac.RpcRemoveModifier<MentorInsomniaModifier>();
            }
        }

        foreach (var student in ModifierUtils.GetPlayersWithModifier<MentorTaughtModifier>().ToList())
        {
            var taughtMod = student.GetModifier<MentorTaughtModifier>();

            if (taughtMod != null && (ushort)student.Data.Role.Role == taughtMod.LessonRoleId)
            {
                student.RpcChangeRole(taughtMod.OriginalRoleId);
            }

            student.RpcRemoveModifier<MentorTaughtModifier>();

            student.RpcAddModifier<MentorInsomniaModifier>((int)options.InsomniaRounds.Value);
        }

        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player == null || player.Data == null || player.Data.Role is not DreamerRole dreamer)
            {
                continue;
            }

            if (dreamer.Player == null || dreamer.Player.HasDied() || dreamer.DreamTargetId == byte.MaxValue)
            {
                continue;
            }

            var target = GameData.Instance.GetPlayerById(dreamer.DreamTargetId)?.Object;
            if (target == null)
            {
                continue;
            }

            var chosenRoleId = dreamer.DreamRoleId;

            DreamerRole.RpcReimagine(dreamer.Player, target, chosenRoleId);
        }
    }
}
