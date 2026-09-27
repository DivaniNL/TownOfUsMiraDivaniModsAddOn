using Il2CppInterop.Runtime.Attributes;
using System;
using AmongUs.GameOptions;
using MiraAPI.GameOptions;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Translation;
using DivaniMods.Assets;
using DivaniMods.Interfaces;
using DivaniMods.Options;
using TownOfUs;
using TownOfUs.Assets;
using TownOfUs.Extensions;
using TownOfUs.Interfaces;
using TownOfUs.Modules.Wiki;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Roles;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;
using MiraAPI.Utilities.Assets;

namespace DivaniMods.Roles.Neutral.NeutralEvil;

public sealed class InnocentRole(IntPtr cppPtr)
    : NeutralRole(cppPtr), IDivaniRole, IWikiDiscoverable, IDoomable, ICrewVariant, IGuessable, INeutralEvilWinOutcomeRole
{
    public static readonly Color InnocentColor = new Color32(255, 141, 168, 255);
    public static Dictionary<byte, InnocentRole> ActiveInnocents { get; } = new();

    public byte? PendingTauntKillerId { get; set; }
    public byte? TauntedKillerId { get; set; }
    public bool TargetVoted { get; set; }
    public bool TargetWasEvil { get; set; }
    public bool AboutToWin { get; set; }
    public bool AwaitingNextMeetingExile { get; set; }
    public bool WinWindowExpired { get; set; }

    public DoomableType DoomHintType => DoomableType.Trickster;
    public RoleBehaviour CrewVariant => RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<EngineerTouRole>());
    public bool CanBeGuessed => true;
    public string RoleName => MiraLocaleManager.Get("DivaniMods.Role.Innocent", "Innocent");
    public string RoleDescription => MiraLocaleManager.Get("DivaniMods.Role.Innocent.Description");
    public string RoleMedDescription => MiraLocaleManager.Get("DivaniMods.Role.Innocent.MedDescription");
    public string RoleLongDescription => MiraLocaleManager.Get("DivaniMods.Role.Innocent.LongDescription");
    public Color RoleColor => InnocentColor;

    public LoadableAsset<Sprite> WinIcon => DivaniAssets.InnocentIcon;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleAlignment RoleAlignment => RoleAlignment.NeutralEvil;
    public bool HasImpostorVision => false;

    public string GetAdvancedDescription() => RoleLongDescription + MiscUtils.AppendOptionsText(GetType());

    [HideFromIl2Cpp] public List<CustomButtonWikiDescription> Abilities { get; } =
    [
        new(
            MiraLocaleManager.Get("DivaniMods.Role.Innocent.Ability.Taunt"),
            MiraLocaleManager.Get("DivaniMods.Role.Innocent.Ability.Taunt.Description"),
            TouNeutAssets.JesterHauntSprite
        )
    ];

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = MiraAPI.Utilities.Assets.TmpSpriteUtils.CreateSpriteAsset(DivaniAssets.InnocentIcon.LoadAsset(), "DivaniMod.Role.Neutral.Innocent", 1.45f),
        OptionsScreenshot = DivaniAssets.InnocentBanner,
        Icon = DivaniAssets.InnocentIcon,
        IntroSound = DivaniAssets.InnocentIntroSound,
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>(),
        MaxRoleCount = 1,
    };

    public override void SpawnTaskHeader(PlayerControl playerControl)
    {
        if (playerControl != PlayerControl.LocalPlayer)
        {
            return;
        }

        var task = PlayerTask.GetOrCreateTask<ImportantTextTask>(playerControl, 0);
        task.Text = $"{TownOfUsColors.Neutral.ToTextColor()}{MiraLocaleManager.Get("NeutralEvilTaskHeader")}</color>";
        task.name = "NeutralRoleText";
    }

    public override void Initialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Initialize(this, targetPlayer);
        ActiveInnocents[targetPlayer.PlayerId] = this;
        PendingTauntKillerId = null;
        TauntedKillerId = null;
        TargetVoted = false;
        AboutToWin = false;
        AwaitingNextMeetingExile = false;
        WinWindowExpired = false;
        TargetWasEvil = false;
        AboutToTorment = false;
        HasKilled = false;
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

    public static bool TauntPiercesShields(PlayerControl? source, PlayerControl? target)
    {
        if (source == null || target == null ||
            !OptionGroupSingleton<InnocentOptions>.Instance.TauntBreaksShields)
        {
            return false;
        }

        return ActiveInnocents.TryGetValue(target.PlayerId, out var innocent) &&
               innocent.PendingTauntKillerId == source.PlayerId;
    }

    public bool ReachedWinCondition => TargetVoted;

    public NeutralEvilWinOutcome WinOutcome => OptionGroupSingleton<InnocentOptions>.Instance.WinOutcome;

    public NeutralEvilWinOutcome EffectiveWinOutcome => TargetWasEvil ? NeutralEvilWinOutcome.Nothing : WinOutcome;

    public bool AboutToTorment { get; set; }

    public bool HasKilled { get; set; }

    public bool WinConditionMet()
    {
        return WinOutcome is NeutralEvilWinOutcome.EndsGame && (TargetVoted || AboutToWin) && !TargetWasEvil;
    }

    public override bool DidWin(GameOverReason gameOverReason)
    {
        return TargetVoted;
    }

    public static void ClearAndReload()
    {
        ActiveInnocents.Clear();
    }
}
