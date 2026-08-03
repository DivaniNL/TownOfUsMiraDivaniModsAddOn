using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;
using MiraAPI.Roles;
using Reactor.Networking.Attributes;
using MiraAPI.Hud;
using DivaniMods.Buttons.Impostor.ImpostorPower;
using DivaniMods.Options;
using DivaniMods.Roles.Impostor.ImpostorPower;
using TownOfUs.Utilities;

namespace DivaniMods.Patches;

[HarmonyPatch]
public static class RecruiterPatch
{
    internal static bool RecruitingDisabled { get; private set; }

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CoStartGame))]
    [HarmonyPostfix]
    public static void ResetOnGameStart()
    {
        RecruitingDisabled = false;
    }

    [RegisterEvent]
    public static void OnRoundStartResolveRecruit(RoundStartEvent @event)
    {
        if (@event.TriggeredByIntro || RecruitingDisabled)
        {
            return;
        }

        var isHost = AmongUsClient.Instance && AmongUsClient.Instance.AmHost;
        var recruitedSomeone = false;

        foreach (var pc in PlayerControl.AllPlayerControls)
        {
            if (pc == null || pc.Data == null || pc.Data.Role is not RecruiterRole recruiter)
            {
                continue;
            }

            var id = recruiter.PendingRecruitTargetId;
            recruiter.PendingRecruitTargetId = byte.MaxValue;

            if (recruiter.Player.Data == null || recruiter.Player.Data.IsDead)
            {
                continue;
            }

            if (id == byte.MaxValue)
            {
                continue;
            }

            var target = GameData.Instance.GetPlayerById(id)?.Object;
            if (!RecruiterRole.IsValidRecruitTarget(target, recruiter.Player))
            {
                continue;
            }

            recruitedSomeone = true;

            if (isHost)
            {
                target!.RpcChangeRole(RoleId.Get<RecruitRole>());
                RpcSetRecruiterRecruited(recruiter.Player);
                if (PlayerControl.LocalPlayer != null)
                {
                    RpcRecruitImpostorFollowUp(PlayerControl.LocalPlayer, target!.PlayerId);
                }
            }
        }

        if (recruitedSomeone)
        {
            RecruitingDisabled = true;
        }
    }

    [MethodRpc((uint)DivaniRpcCalls.RecruiterSetRecruited)]
    public static void RpcSetRecruiterRecruited(PlayerControl recruiterPlayer)
    {
        if (recruiterPlayer?.Data?.Role is not RecruiterRole recruiter)
        {
            return;
        }

        recruiter.HasRecruited = true;

        if (recruiterPlayer.AmOwner)
        {
            ShowRecruiterChangeButton(recruiter);
        }
    }

    [RegisterEvent]
    public static void OnRoundStartShowRecruiterButton(RoundStartEvent _)
    {
        if (PlayerControl.LocalPlayer?.Data?.Role is RecruiterRole { HasRecruited: true } recruiter)
        {
            ShowRecruiterChangeButton(recruiter);
        }
    }

    private static void ShowRecruiterChangeButton(RecruiterRole recruiter)
    {
        CustomButtonSingleton<RecruiterChangeButton>.Instance.SetActive(true, recruiter);

        if (HudManager.InstanceExists)
        {
            HudManager.Instance.SetHudActive(false);
            HudManager.Instance.SetHudActive(true);
        }
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
