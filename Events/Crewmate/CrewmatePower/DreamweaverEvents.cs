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

public static class DreamweaverEvents
{
    [RegisterEvent]
    public static void OnRoundStart(RoundStartEvent evt)
    {
        if (evt.TriggeredByIntro || !AmongUsClient.Instance.AmHost)
        {
            return;
        }

        var options = OptionGroupSingleton<DreamweaverOptions>.Instance;

        foreach (var insomniac in ModifierUtils.GetPlayersWithModifier<DreamweaverInsomniaModifier>().ToList())
        {
            var insomniaMod = insomniac.GetModifier<DreamweaverInsomniaModifier>();
            if (insomniaMod == null)
            {
                continue;
            }

            insomniaMod.RoundsLeft--;
            if (insomniaMod.RoundsLeft <= 0)
            {
                insomniac.RpcRemoveModifier<DreamweaverInsomniaModifier>();
            }
        }

        foreach (var dreaming in ModifierUtils.GetPlayersWithModifier<DreamweaverTargetDreamingModifier>().ToList())
        {
            var dreamMod = dreaming.GetModifier<DreamweaverTargetDreamingModifier>();

            if (dreamMod != null && (ushort)dreaming.Data.Role.Role == dreamMod.DreamRoleId)
            {
                dreaming.RpcChangeRole(dreamMod.OriginalRoleId);
            }

            dreaming.RpcRemoveModifier<DreamweaverTargetDreamingModifier>();

            dreaming.RpcAddModifier<DreamweaverInsomniaModifier>((int)options.InsomniaRounds.Value);
        }

        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player == null || player.Data == null || player.Data.Role is not DreamweaverRole Dreamweaver)
            {
                continue;
            }

            if (Dreamweaver.Player == null || Dreamweaver.Player.HasDied() || Dreamweaver.DreamTargetId == byte.MaxValue)
            {
                continue;
            }

            var target = GameData.Instance.GetPlayerById(Dreamweaver.DreamTargetId)?.Object;
            var chosenRole = RoleManager.Instance.GetRole((AmongUs.GameOptions.RoleTypes)Dreamweaver.DreamRoleId);

            if (target == null)
            {
                continue;
            }

            if (!DreamweaverRole.IsValidDreamTarget(target, Dreamweaver.Player))
            {
                continue;
            }

            if (chosenRole == target.Data.Role && options.FailDreamOnNoChange)
            {
                DreamweaverRole.RpcNotifyDreamFailed(Dreamweaver.Player, target);
                continue;
            }

            if (target.HasModifier<DreamweaverTargetDreamingModifier>())
            {
                continue;
            }

            // Max role count is always respected: if the dream would exceed it, the dream fails.
            if (chosenRole != null && DreamweaverRole.IsBreakingMaxRoleCount(chosenRole, target))
            {
                DreamweaverRole.RpcNotifyDreamFailed(Dreamweaver.Player, target);
                continue;
            }

            if (!target.IsCrewmate())
            {
                DreamweaverRole.RpcNotifyDreamFailed(Dreamweaver.Player, target);
                continue;
            }

            var originalRole = (ushort)target.Data.Role.Role;
            target.RpcChangeRole(Dreamweaver.DreamRoleId);
            target.RpcAddModifier<DreamweaverTargetDreamingModifier>(originalRole, Dreamweaver.DreamRoleId);
        }
    }
}
