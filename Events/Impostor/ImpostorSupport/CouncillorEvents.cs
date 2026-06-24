using System.Collections.Generic;
using System.Linq;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting.Voting;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using DivaniMods.Assets;
using DivaniMods.Options;
using DivaniMods.Roles.Impostor.ImpostorSupport;
using TownOfUs.Modifiers;
using TownOfUs.Modules;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using UnityEngine;

namespace DivaniMods.Events.Impostor.ImpostorSupport;

public static class CouncillorEvents
{
    private static readonly Dictionary<byte, int> ExtraVotesByPlayer = new();

    public static int GetExtraVotes(byte playerId) =>
        ExtraVotesByPlayer.TryGetValue(playerId, out var votes) ? votes : 0;

    public static void ResetAll() => ExtraVotesByPlayer.Clear();

    [RegisterEvent]
    public static void OnAfterMurder(AfterMurderEvent evt)
    {
        if (evt.Source == null || evt.Source.Data?.Role is not CouncillorRole)
        {
            return;
        }

        var id = evt.Source.PlayerId;
        var gained = GetVotesGainedFromKill(evt.Target);
        ExtraVotesByPlayer[id] = GetExtraVotes(id) + gained;

        if (evt.Source.AmOwner)
        {
            ShowBonusVoteNotification(evt.Target, gained);
        }
    }

    private static void ShowBonusVoteNotification(PlayerControl? target, int votesGained)
    {
        if (target == null || !OptionGroupSingleton<CouncillorOptions>.Instance.GainsAllVotes)
        {
            return;
        }

        if (votesGained <= 1)
        {
            return;
        }

        var isMayor = target.GetRoleWhenAlive() is MayorRole mayor && mayor.Revealed;
        var isKnighted = target.HasModifier<KnightedModifier>();

        if (!isMayor && !isKnighted)
        {
            return;
        }

        var what = isMayor && isKnighted
            ? "Knighted Mayor"
            : isMayor ? "Mayor" : "Knighted player";
        var colorHex = ColorUtility.ToHtmlStringRGB(Palette.ImpostorRed);
        MiraAPI.Utilities.Helpers.CreateAndShowNotification(
            $"<b><color=#{colorHex}>You killed a {what} and gained {votesGained} extra vote{(votesGained == 1 ? "" : "s")} this round!</color></b>",
            Color.white,
            new Vector3(0f, 1f, -20f),
            spr: DivaniAssets.CouncillorIcon.LoadAsset());
    }

    private static int GetVotesGainedFromKill(PlayerControl? target)
    {
        if (target == null || !OptionGroupSingleton<CouncillorOptions>.Instance.GainsAllVotes)
        {
            return 1;
        }

        return GetVoteCount(target);
    }

    private static int GetVoteCount(PlayerControl player)
    {
        var votes = 1;

        if (player.GetRoleWhenAlive() is MayorRole mayor && mayor.Revealed)
        {
            votes += 2;
        }

        if (player.HasModifier<KnightedModifier>())
        {
            var knightCount = player.GetModifiers<KnightedModifier>()?.Count() ?? 0;
            if (knightCount <= 0)
            {
                knightCount = 1;
            }

            var votesPerKnight = (int)OptionGroupSingleton<MonarchOptions>.Instance.VotesPerKnight;
            votes += knightCount * votesPerKnight;
        }

        return votes;
    }

    [RegisterEvent]
    public static void OnRoundStart(RoundStartEvent _) => ResetAll();

    [RegisterEvent]
    public static void HandleVoteEvent(HandleVoteEvent evt)
    {
        var owner = evt.VoteData.Owner;
        if (owner == null || owner.Data?.Role is not CouncillorRole)
        {
            return;
        }

        var extra = GetExtraVotes(owner.PlayerId);
        if (extra <= 0)
        {
            return;
        }

        evt.VoteData.SetRemainingVotes(0);

        for (var i = 0; i < 1 + extra; i++)
        {
            evt.VoteData.VoteForPlayer(evt.TargetId);
        }

        evt.Cancel();
    }
}
