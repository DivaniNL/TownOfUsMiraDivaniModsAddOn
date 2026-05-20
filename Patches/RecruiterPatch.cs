using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using AmongUs.GameOptions;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;
using MiraAPI.Roles;
using Reactor.Networking.Attributes;
using DivaniMods.Options;
using DivaniMods.Roles.Impostor.ImpostorSupport;

namespace DivaniMods.Patches;

[HarmonyPatch]
public static class RecruiterPatch
{
    internal static int MeetingsEnded { get; private set; }

    private static readonly HashSet<byte> PendingRecruitFollowUpIds = new();

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CoStartGame))]
    [HarmonyPostfix]
    public static void ResetOnGameStart()
    {
        MeetingsEnded = 0;
        PendingRecruitFollowUpIds.Clear();
    }

    [RegisterEvent]
    public static void OnEndMeeting(EndMeetingEvent _)
    {
        var wasFirstMeeting = MeetingsEnded == 0;
        MeetingsEnded++;

        if (!AmongUsClient.Instance || !AmongUsClient.Instance.AmHost || !wasFirstMeeting)
        {
            return;
        }

        foreach (var pc in PlayerControl.AllPlayerControls)
        {
            if (pc == null || pc.Data == null || pc.Data.Role is not RecruiterRole recruiter)
            {
                continue;
            }

            if (recruiter.Player.Data == null || recruiter.Player.Data.IsDead)
            {
                continue;
            }

            var id = recruiter.PendingRecruitTargetId;
            if (id == byte.MaxValue)
            {
                continue;
            }

            var target = GameData.Instance.GetPlayerById(id)?.Object;
            if (!RecruiterRole.IsValidRecruitTarget(target, recruiter.Player))
            {
                continue;
            }

            target!.RpcSetRole(RoleTypes.Impostor, true);
            PendingRecruitFollowUpIds.Add(target.PlayerId);
            if (PlayerControl.LocalPlayer != null)
            {
                RpcRecruitImpostorFollowUp(PlayerControl.LocalPlayer, target.PlayerId);
            }
        }
    }

    [RegisterEvent]
    public static void OnRoundStartRecruitFollowUp(RoundStartEvent _)
    {
        if (!AmongUsClient.Instance || !AmongUsClient.Instance.AmHost)
        {
            return;
        }

        if (PendingRecruitFollowUpIds.Count == 0)
        {
            return;
        }

        var local = PlayerControl.LocalPlayer;
        if (local == null)
        {
            return;
        }

        foreach (var playerId in PendingRecruitFollowUpIds.ToArray())
        {
            RpcRecruitImpostorFollowUp(local, playerId);
        }

        PendingRecruitFollowUpIds.Clear();
    }

    private static bool RecruitedShouldBecomeAssassin() =>
        OptionGroupSingleton<RecruiterOptions>.Instance.RecruitedBecomesAssassin;

    [MethodRpc((uint)DivaniRpcCalls.RecruitImpostorFollowUp)]
    public static void RpcRecruitImpostorFollowUp(PlayerControl _, byte targetPlayerId)
    {
        var target = GameData.Instance?.GetPlayerById(targetPlayerId)?.Object;
        if (target == null || target.Data == null || target.Data.IsDead)
        {
            return;
        }

        if (target.Data.Role is not ImpostorRole)
        {
            return;
        }

        if (RecruitedShouldBecomeAssassin())
        {
            TryAddImpostorAssassinModifier(target);
        }
        else
        {
            StripImpostorAssassinModifiers(target);
        }
    }

    private static void TryAddImpostorAssassinModifier(PlayerControl target)
    {
        var typeId = LookupImpostorAssassinModifierTypeId();
        if (typeId == 0)
        {
            return;
        }

        if (target.GetModifiers<BaseModifier>().Any(m => m.TypeId == typeId))
        {
            return;
        }

        target.AddModifier(typeId);
    }

    private static void StripImpostorAssassinModifiers(PlayerControl target)
    {
        var toRemove = new List<uint>();
        foreach (var m in target.GetModifiers<BaseModifier>())
        {
            for (var t = m.GetType(); t != null && t != typeof(object); t = t.BaseType)
            {
                if (t.Name != "ImpostorAssassinModifier")
                {
                    continue;
                }

                toRemove.Add(m.TypeId);
                break;
            }
        }

        foreach (var typeId in toRemove.Distinct())
        {
            target.RemoveModifier(typeId, null);
        }
    }

    private static uint LookupImpostorAssassinModifierTypeId()
    {
        try
        {
            var asm = Assembly.Load("TownOfUsMira");
            var t = asm.GetType("TownOfUs.Modifiers.Game.Impostor.ImpostorAssassinModifier");
            return t == null ? 0u : ModifierManager.GetModifierTypeId(t) ?? 0u;
        }
        catch
        {
            return 0u;
        }
    }
}
