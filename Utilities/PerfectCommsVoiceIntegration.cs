using System.Runtime.CompilerServices;
using DivaniMods.Modifiers.Neutral.NeutralOutlier;
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

            if (speakerDueling && listenerDueling
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
            ctx.Listener.Data?.Role is VengefulSoulRole
                ? VoicePairResult.Mute("Vengeful Soul")
                : VoicePairResult.Pass);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Unregister() => PerfectCommsApi.Unregister(Mod);
}