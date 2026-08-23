using System.Collections;
using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting.Voting;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.GameOptions;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using MiraAPI.Voting;
using Reactor.Utilities;
using DivaniMods.Options;
using DivaniMods.Roles.Impostor.ImpostorPower;
using TownOfUs.Events.Modifiers;
using TownOfUs.Utilities;
using UnityEngine;

namespace DivaniMods.Events.Impostor.ImpostorPower;

public static class ObfuscatorEvents
{
    [RegisterEvent(20)]
    public static void ProcessVotesEventHandler(ProcessVotesEvent @event)
    {
        foreach (var obf in CustomRoleUtils.GetActiveRolesOfType<ObfuscatorRole>())
        {
            SwapVotes(@event, obf);
        }
    }

    [RegisterEvent]
    public static void VotingCompleteEventHandler(VotingCompleteEvent @event)
    {
        if (!CustomRoleUtils.GetActiveRolesOfType<ObfuscatorRole>().HasAny())
        {
            return;
        }

        var swapperCount = CustomRoleUtils.GetActiveRolesOfType<TownOfUs.Roles.Crewmate.SwapperRole>().Count();
        var swapperDelay = swapperCount > 0 ? 4f / (swapperCount + 1) * swapperCount : 0f;

        Coroutines.Start(DelayedPerformSwaps(swapperDelay));
    }

    private static IEnumerator DelayedPerformSwaps(float delay)
    {
        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }

        var inner = PerformSwaps();
        while (inner.MoveNext())
        {
            yield return inner.Current;
        }
    }

    [RegisterEvent]
    public static void AfterMurderEventHandler(AfterMurderEvent @event)
    {
        if (@event.Source == null || @event.Source.Data?.Role is not ObfuscatorRole obf) return;

        var killsPer = (int)OptionGroupSingleton<ObfuscatorOptions>.Instance.KillsPerExtraCharge.Value;
        if (killsPer <= 0) return;

        obf.KillsSinceLastCharge++;
        if (obf.KillsSinceLastCharge >= killsPer)
        {
            obf.ChargesRemaining++;
            obf.KillsSinceLastCharge = 0;
        }
    }

    private static void SwapVotes(ProcessVotesEvent @event, ObfuscatorRole obf)
    {
        if (obf == null || obf.Player == null || obf.Player.HasDied()) return;
        if (obf.Swap1 == null || obf.Swap2 == null) return;
        if (obf.ChargesRemaining <= 0) return;

        var obfSwap1 = obf.Swap1!.PlayerId;
        var obfSwap2 = obf.Swap2!.PlayerId;

        var originalVoteList = @event.Votes.ToList();
        if (TiebreakerEvents.TiebreakingVote.HasValue)
        {
            originalVoteList.Add(TiebreakerEvents.TiebreakingVote.Value);
        }

        var swappers = CustomRoleUtils.GetActiveRolesOfType<TownOfUs.Roles.Crewmate.SwapperRole>()
            .Where(s => s != null && !s.Player.HasDied() && s.Swap1 != null && s.Swap2 != null)
            .ToList();

        var voteList = new List<CustomVote>();
        foreach (var vote in originalVoteList)
        {
            var suspect = vote.Suspect;

            foreach (var s in swappers)
            {
                var a = s.Swap1!.PlayerId;
                var b = s.Swap2!.PlayerId;
                if (suspect == a) suspect = b;
                else if (suspect == b) suspect = a;
            }

            if (suspect == obfSwap1) suspect = obfSwap2;
            else if (suspect == obfSwap2) suspect = obfSwap1;

            voteList.Add(new CustomVote(vote.Voter, suspect));
        }

        if (@event.ExiledPlayer != null)
        {
            @event.ExiledPlayer = VotingUtils.GetExiled(voteList, out _);
        }
        ObfuscatorRole.RpcConsumeCharge(obf.Player);
    }

    private static IEnumerator PerformSwaps()
    {
        var roles = CustomRoleUtils.GetActiveRolesOfType<ObfuscatorRole>().ToList();
        var duration = 4f / (roles.Count + 1);

        foreach (var role in roles)
        {
            if (role == null || role.Player.HasDied() || role.Swap1 == null || role.Swap2 == null)
            {
                yield break;
            }

            var p1 = role.Swap1.GetPlayer();
            var p2 = role.Swap2.GetPlayer();
            if (p1 == null || p2 == null || p1.HasDied() || p2.HasDied())
            {
                yield break;
            }

            var elements1 = GetUIElements(role.Swap1);
            var elements2 = GetUIElements(role.Swap2);

            var votes1 = GetVoteTransforms(role.Swap1);
            var votes2 = GetVoteTransforms(role.Swap2);

            votes2.ForEach(vote =>
                vote.GetComponent<SpriteRenderer>().material.SetInt(PlayerMaterial.MaskLayer, role.Swap1.MaskLayer));
            votes1.ForEach(vote =>
                vote.GetComponent<SpriteRenderer>().material.SetInt(PlayerMaterial.MaskLayer, role.Swap2.MaskLayer));

            for (var i = 0; i < elements1.Length; i++)
            {
                Coroutines.Start(Slide2D(elements1[i], elements1[i].position, elements2[i].position, duration));
                Coroutines.Start(Slide2D(elements2[i], elements2[i].position, elements1[i].position, duration));
            }

            yield return new WaitForSeconds(duration);
        }
    }

    private static Transform[] GetUIElements(PlayerVoteArea voteArea)
    {
        return
        [
            voteArea.PlayerIcon.transform,
            voteArea.NameText.transform,
            voteArea.Background.transform,
            voteArea.MaskArea.transform,
            voteArea.PlayerButton.transform,
            voteArea.LevelNumberText.transform,
            voteArea.ColorBlindName.transform,
            voteArea.Overlay.transform,
            voteArea.Megaphone.transform,
        ];
    }

    private static List<Transform> GetVoteTransforms(PlayerVoteArea voteArea)
    {
        var votes = new List<Transform>();
        for (var i = 0; i < voteArea.transform.childCount; i++)
        {
            var child = voteArea.transform.GetChild(i);
            if (child.name == "playerVote(Clone)")
            {
                votes.Add(child);
            }
        }
        return votes;
    }

    private static IEnumerator Slide2D(Transform target, Vector3 source, Vector3 dest, float duration)
    {
        yield return MiscUtils.PerformTimedAction(duration, p => target.position = Vector3.Lerp(source, dest, p));
    }
}
