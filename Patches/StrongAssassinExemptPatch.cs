using HarmonyLib;
using MiraAPI.Modifiers;
using DivaniMods.Modifiers.Game.Crewmate;
using TownOfUs.Modifiers.Game;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Roles.Neutral;

namespace DivaniMods.Patches;

public static class StrongGuessExempt
{
    public static bool TargetHasStrong(PlayerVoteArea? voteArea)
    {
        if (voteArea == null)
        {
            return false;
        }

        var player = GameData.Instance.GetPlayerById(voteArea.TargetPlayerId)?.Object;
        return player != null && player.HasModifier<StrongModifier>();
    }
}

[HarmonyPatch(typeof(AssassinModifier), nameof(AssassinModifier.IsExempt))]
public static class StrongAssassinExemptPatch
{
    public static void Postfix(PlayerVoteArea voteArea, ref bool __result)
    {
        if (!__result && StrongGuessExempt.TargetHasStrong(voteArea))
        {
            __result = true;
        }
    }
}

[HarmonyPatch(typeof(DoomsayerRole), nameof(DoomsayerRole.IsExempt))]
public static class StrongDoomsayerExemptPatch
{
    public static void Postfix(PlayerVoteArea voteArea, ref bool __result)
    {
        if (!__result && StrongGuessExempt.TargetHasStrong(voteArea))
        {
            __result = true;
        }
    }
}

[HarmonyPatch(typeof(VigilanteRole), nameof(VigilanteRole.IsExempt))]
public static class StrongVigilanteExemptPatch
{
    public static void Postfix(PlayerVoteArea voteArea, ref bool __result)
    {
        if (!__result && StrongGuessExempt.TargetHasStrong(voteArea))
        {
            __result = true;
        }
    }
}
