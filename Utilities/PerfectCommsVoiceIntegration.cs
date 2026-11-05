using System.Runtime.CompilerServices;
using DivaniMods.Modifiers.Neutral.NeutralOutlier;
using DivaniMods.Modules.Monster;
using DivaniMods.Roles.Crewmate.CrewmateAfterlife;
using MiraAPI.Modifiers;
using PerfectComms.Api;

namespace DivaniMods.Utilities;

internal static class PerfectCommsVoiceIntegration
{
    private const string Mod = DivaniPlugin.Id;

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Register()
    {
        PerfectCommsApi.RegisterVoicePairRule(Mod, ctx =>
        {
            var speakerDueling = ctx.Speaker.TryGetModifier<DuelModifier>(out var speakerDuel);
            var listenerDueling = ctx.Listener.TryGetModifier<DuelModifier>(out var listenerDuel);

            if (speakerDuel != null && listenerDuel != null
                && speakerDuel.OpponentId == ctx.Listener.PlayerId
                && listenerDuel.OpponentId == ctx.Speaker.PlayerId)
            {
                return VoicePairResult.Route(VoicePairRouteShape.Radio, reason: "Duel");
            }

            if (!speakerDueling && !listenerDueling)
            {
                return VoicePairResult.Pass;
            }

            if (ctx.ListenerIsDead)
            {
                return speakerDueling
                    ? VoicePairResult.Route(VoicePairRouteShape.Radio, reason: "Duel")
                    : VoicePairResult.Pass;
            }

            return VoicePairResult.Mute("Duel");
        });

        PerfectCommsApi.RegisterVoicePairRule(Mod, ctx =>
            ctx.SpeakerIsDead && ctx.Listener.Data?.Role is VengefulSoulRole
                ? VoicePairResult.Mute("Vengeful Soul")
                : VoicePairResult.Pass);

        PerfectCommsApi.RegisterVoicePairRule(Mod, ctx =>
        {
            if (!MonsterState.IsEaten(ctx.Speaker.PlayerId)) return VoicePairResult.Pass;

            var monsterId = MonsterState.MonsterOf(ctx.Speaker.PlayerId);
            if (ctx.ListenerIsDead || ctx.Listener.PlayerId == monsterId) return VoicePairResult.Pass;

            return VoicePairResult.Mute("Inside the Monster's belly");
        });

        PerfectCommsApi.RegisterVoiceChannel(Mod, ctx =>
        {
            if (MonsterState.CountHeldBy(ctx.Player.PlayerId) > 0)
                return new VoiceChannelResult($"belly:{ctx.Player.PlayerId}", TwoWay: true, Shape: VoiceAudioShape.Radio);

            var owner = MonsterState.MonsterOf(ctx.Player.PlayerId);
            return owner != null
                ? new VoiceChannelResult($"belly:{owner}", TwoWay: true, Shape: VoiceAudioShape.Radio)
                : null;
        });
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Unregister() => PerfectCommsApi.Unregister(Mod);
}