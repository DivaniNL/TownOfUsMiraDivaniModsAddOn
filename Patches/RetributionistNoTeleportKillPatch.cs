using HarmonyLib;
using MiraAPI.Networking;
using MiraAPI.Utilities;
using DivaniMods.Roles.Crewmate.CrewmateKilling;
using TownOfUs.Events;
using TownOfUs.Modifiers;
using TownOfUs.Modules;
using TownOfUs.Modules.Localization;
using TownOfUs.Roles;
using TownOfUs.Modules.Components;

namespace DivaniMods.Patches;

[HarmonyPatch(typeof(CustomMurderRpc), nameof(CustomMurderRpc.RpcConfirmCustomMurder),
    typeof(PlayerControl), typeof(PlayerControl), typeof(PlayerControl), typeof(MurderResultFlags),
    typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(bool))]
internal static class RetributionistNoTeleportKillPatch
{
    [HarmonyPriority(Priority.First)]
    [HarmonyPrefix]
    public static void Prefix(PlayerControl host, PlayerControl source, PlayerControl target,
        MurderResultFlags murderResultFlags, ref bool teleportMurderer)
    {
        if (target == null || target.GetRoleWhenAlive() is not RetributionistRole)
        {
            return;
        }

        teleportMurderer = false;

        if (LobbyBehaviour.Instance || host?.IsHost() != true ||
            source == null || source.Data == null || source.PlayerId == target.PlayerId ||
            target.Data == null || target.Data.IsDead || target.Data.Disconnected ||
            !murderResultFlags.HasFlag(MurderResultFlags.Succeeded) ||
            murderResultFlags.HasFlag(MurderResultFlags.FailedProtected) ||
            target.protectedByGuardianId > -1)
        {
            return;
        }

        var cod = source.GetRoleWhenAlive() is ITownOfUsRole touRole && touRole.LocaleKey != "KEY_MISS"
            ? touRole.LocaleKey
            : "Killer";

        GameHistory.UpdatePlayerDeathData(
            target,
            TouLocale.Get($"DiedTo{cod}"),
            roundOfDeath: HudManagerHelper.Instance.CurrentRound,
            diedThisRound: !MeetingHud.Instance && !ExileController.Instance
                ? DeathHandlerOverride.SetTrue
                : DeathHandlerOverride.SetFalse,
            killedBy: TouLocale.GetParsed("DiedByStringBasic").Replace("<player>", source.Data.PlayerName),
            lockInfo: DeathHandlerOverride.SetTrue);
    }
}
