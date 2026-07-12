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

public static class DreamerEvents
{
    [RegisterEvent]
    public static void OnRoundStart(RoundStartEvent evt)
    {
        if (evt.TriggeredByIntro || !AmongUsClient.Instance.AmHost)
        {
            return;
        }

        var options = OptionGroupSingleton<DreamerOptions>.Instance;

        foreach (var insomniac in ModifierUtils.GetPlayersWithModifier<DreamerInsomniaModifier>().ToList())
        {
            var insomniaMod = insomniac.GetModifier<DreamerInsomniaModifier>();
            if (insomniaMod == null)
            {
                continue;
            }

            insomniaMod.RoundsLeft--;
            if (insomniaMod.RoundsLeft <= 0)
            {
                insomniac.RpcRemoveModifier<DreamerInsomniaModifier>();
            }
        }

        foreach (var dreaming in ModifierUtils.GetPlayersWithModifier<DreamerTargetDreamingModifier>().ToList())
        {
            var dreamMod = dreaming.GetModifier<DreamerTargetDreamingModifier>();

            if (dreamMod != null && (ushort)dreaming.Data.Role.Role == dreamMod.DreamRoleId)
            {
                dreaming.RpcChangeRole(dreamMod.OriginalRoleId);
            }

            dreaming.RpcRemoveModifier<DreamerTargetDreamingModifier>();

            dreaming.RpcAddModifier<DreamerInsomniaModifier>((int)options.InsomniaRounds.Value);
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
            var chosenRole = RoleManager.Instance.GetRole((AmongUs.GameOptions.RoleTypes)dreamer.DreamRoleId);

            if (target == null)
            {
                continue;
            }

            if (!DreamerRole.IsValidDreamTarget(target, dreamer.Player))
            {
                continue;
            }

            if (chosenRole == target.Data.Role && options.FailDreamOnNoChange)
            {
                DreamerRole.RpcNotifyDreamFailed(dreamer.Player, target);
                continue;
            }

            if (!target!.IsCrewmate())
            {
                DreamerRole.RpcNotifyDreamFailed(dreamer.Player, target);
                continue;
            }

            if (target.HasModifier<DreamerTargetDreamingModifier>())
            {
                continue;
            }

            if (options.RespectMaxRoleCount.Value)
            {
                if (chosenRole != null && DreamerRole.IsBreakingMaxRoleCount(chosenRole, target))
                {
                    var onBreak = (DreamerOnDreamBreakMaxRoleCount)options.OnMaxRoleCountBroken.Value;

                    if (onBreak == DreamerOnDreamBreakMaxRoleCount.ApplyRandom)
                    {
                        var randomRole = DreamerRole.GetRandomValidRole(target);
                        if (randomRole == null)
                        {
                            DreamerRole.RpcNotifyDreamFailed(dreamer.Player, target);
                            continue;
                        }

                        dreamer.DreamRoleId = (ushort)randomRole.Role;
                        DreamerRole.RpcNotifyDreamRedirected(dreamer.Player, dreamer.DreamRoleId);
                    }
                    else
                    {
                        DreamerRole.RpcNotifyDreamFailed(dreamer.Player, target);
                        continue;
                    }
                }
            }

            var originalRole = (ushort)target.Data.Role.Role;
            target.RpcChangeRole(dreamer.DreamRoleId);
            target.RpcAddModifier<DreamerTargetDreamingModifier>(originalRole, dreamer.DreamRoleId);
        }
    }
}
