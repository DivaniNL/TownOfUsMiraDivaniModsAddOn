using System;
using System.Linq;
using AmongUs.GameOptions;
using MiraAPI.GameOptions;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using DivaniMods.Assets;
using TownOfUs;
using TownOfUs.Extensions;
using TownOfUs.Modules.Localization;
using TownOfUs.Modules.Wiki;
using TownOfUs.Roles;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;

namespace DivaniMods.Roles.Neutral.NeutralKilling;

public sealed class FragRole(IntPtr cppPtr)
    : NeutralRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable, ICrewVariant
{
    public static readonly Color FragColor = new Color32(255, 110, 60, 255);

    public string RoleName => "Frag";
    public string RoleDescription => "Start a hot-potato time bomb!";
    public string RoleLongDescription =>
        "Give a time bomb to a player.\n" +
        "After a short random delay the timer starts.\n" +
        "The holder can pass it on, but not back\n" +
        "to the previous holder until it moves again.\n" +
        "Win by outlasting all other killers.";
    public Color RoleColor => FragColor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleAlignment RoleAlignment => RoleAlignment.NeutralKilling;

    public DoomableType DoomHintType => DoomableType.Relentless;

    public RoleBehaviour CrewVariant =>
        RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<TrapperRole>());

    public string GetAdvancedDescription() => RoleLongDescription + MiscUtils.AppendOptionsText(GetType());

    public bool HasImpostorVision => true;

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = DivaniAssets.FragIcon,
        IntroSound = DivaniAssets.FragIntroSound,
        MaxRoleCount = 1,
        // Required so that on death the Frag's role is swapped to NeutralGhostRole
        // (which remains in ModdedRoleTeams.Custom and whose WinConditionMet()
        // delegates to GetRoleWhenAlive()). Without this the dead Frag gets the
        // default CrewmateGhost role, which is filtered out by
        // NeutralRoleWinCondition.GetActiveRolesOfTeam(Custom) and the win
        // never triggers.
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>(),
    };

    public override void SpawnTaskHeader(PlayerControl playerControl)
    {
        if (playerControl != PlayerControl.LocalPlayer)
        {
            return;
        }
        ImportantTextTask orCreateTask = PlayerTask.GetOrCreateTask<ImportantTextTask>(playerControl, 0);
        orCreateTask.Text =
            $"{TownOfUsColors.Neutral.ToTextColor()}{TouLocale.GetParsed("NeutralKillingTaskHeader")}</color>";
        orCreateTask.name = "NeutralRoleText";
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);
        TouRoleUtils.ClearTaskHeader(Player);
    }

    public override bool CanUse(IUsable usable)
    {
        if (!GameManager.Instance.LogicUsables.CanUse(usable, Player))
        {
            return false;
        }

        var console = usable.TryCast<Console>()!;
        return console == null || console.AllowImpostor;
    }

    /// <summary>
    /// Glitch-style: Frag wins by being the last killer faction left. Requires every
    /// non-Frag killer dead AND Frag count to be at least equal to the non-Frag alive count.
    /// </summary>
    public bool WinConditionMet()
    {
        var fragCount = CustomRoleUtils.GetActiveRolesOfType<FragRole>().Count(x => !x.Player.HasDied());

        if (MiscUtils.KillersAliveCount > fragCount)
        {
            return false;
        }

        return fragCount >= Helpers.GetAlivePlayers().Count - fragCount;
    }

    public override bool DidWin(GameOverReason gameOverReason)
    {
        return WinConditionMet();
    }
}
