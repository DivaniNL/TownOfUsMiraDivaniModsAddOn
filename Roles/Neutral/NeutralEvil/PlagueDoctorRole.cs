using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using Reactor.Networking.Attributes;
using DivaniMods.Assets;
using DivaniMods.Modifiers.Neutral.NeutralEvil;
using DivaniMods.Options;
using DivaniMods.Patches;
using TownOfUs;
using TownOfUs.Assets;
using TownOfUs.Extensions;
using TownOfUs.Modules.Localization;
using TownOfUs.Modules.Wiki;
using TownOfUs.Roles;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;

namespace DivaniMods.Roles.Neutral.NeutralEvil;

public sealed class PlagueDoctorRole(IntPtr cppPtr)
    : NeutralRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable, ICrewVariant
{
    public DoomableType DoomHintType => DoomableType.Fearmonger;

    public RoleBehaviour CrewVariant =>
        RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<MedicRole>());

    public static readonly Color PlagueDoctorColor = new Color32(255, 192, 0, 255);

    public static Dictionary<byte, float> InfectionProgress { get; } = new();
    public static Dictionary<byte, bool> DeadPlayers { get; } = new();
    public static PlayerControl? PlagueDoctorPlayer { get; internal set; }

    private static readonly Dictionary<byte, float> LastAccrueFrame = new();
    private static float _lastProgressSync;

    public static int NumInfectionsRemaining { get; set; }
    public static bool MeetingFlag { get; set; }
    public static float ImmunityTimer { get; set; }
    public static bool InfectionWarningShown { get; set; }

    public string RoleName => "Plague Doctor";
    public string RoleDescription => "Cough, cough!";
    public string RoleLongDescription => "You are a Plague Doctor.\n" +
        "Use your ability to infect players directly.\n" +
        "Infected players will spread the disease\nto others who stand near them.\n" +
        "Win by infecting all living players!";
    public Color RoleColor => PlagueDoctorColor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleAlignment RoleAlignment => RoleAlignment.NeutralEvil;

    public string GetAdvancedDescription() => RoleLongDescription + MiscUtils.AppendOptionsText(GetType());

    public bool HasImpostorVision => true;

    [HideFromIl2Cpp] public List<CustomButtonWikiDescription> Abilities { get; } =
    [
        new("Infect", "Directly infect a player.", DivaniAssets.PlagueDoctorInfectButton)
    ];

    public CustomRoleConfiguration Configuration => new(this)
    {
        CanUseVent = OptionGroupSingleton<PlagueDoctorOptions>.Instance.CanVent,
        Icon = DivaniAssets.PlagueDoctorIcon,
        IntroSound = DivaniAssets.PlagueDoctorIntroSound,
        MaxRoleCount = 1,
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>(),
    };

    public static bool CanWinWhileDead => OptionGroupSingleton<PlagueDoctorOptions>.Instance.CanWinDead;

    public override void Initialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Initialize(this, targetPlayer);
        
        var previous = PlagueDoctorPlayer;
        PlagueDoctorPlayer = targetPlayer;
        if (previous == null || previous.PlayerId != targetPlayer.PlayerId)
        {
        }

        if (Player.AmOwner)
        {
            HudManager.Instance.ImpostorVentButton.graphic.sprite = DivaniAssets.PlagueDoctorVentButton.LoadAsset();
            HudManager.Instance.ImpostorVentButton.buttonLabelText.SetOutlineColor(PlagueDoctorColor);
        }
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);
        TouRoleUtils.ClearTaskHeader(Player);
        if (Player.AmOwner)
        {
            HudManager.Instance.ImpostorVentButton.graphic.sprite = TouAssets.VentSprite.LoadAsset();
            HudManager.Instance.ImpostorVentButton.buttonLabelText.SetOutlineColor(TownOfUsColors.Impostor);
        }
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

    public static void ClearAndReload()
    {
        InfectionProgress.Clear();
        LastAccrueFrame.Clear();
        DeadPlayers.Clear();
        PlagueDoctorPlayer = null;
        NumInfectionsRemaining = (int)OptionGroupSingleton<PlagueDoctorOptions>.Instance.MaxInfections.Value;
        MeetingFlag = false;
        ImmunityTimer = 0f;
        InfectionWarningShown = false;
        _lastProgressSync = 0f;
    }

    public override void SpawnTaskHeader(PlayerControl playerControl)
    {
        if (playerControl != PlayerControl.LocalPlayer)
        {
            return;
        }

        ImportantTextTask orCreateTask = PlayerTask.GetOrCreateTask<ImportantTextTask>(playerControl, 0);
        orCreateTask.Text =
            $"{TownOfUsColors.Neutral.ToTextColor()}{TouLocale.GetParsed("NeutralEvilTaskHeader")}</color>";
        orCreateTask.name = "NeutralRoleText";
    }

    public static bool IsInfected(PlayerControl? player)
    {
        return player != null && player.HasModifier<PlagueInfectedModifier>();
    }

    public static bool IsPlagueDoctor(PlayerControl? player)
    {
        if (player == null)
        {
            return false;
        }

        foreach (var role in CustomRoleUtils.GetActiveRolesOfType<PlagueDoctorRole>())
        {
            if (role.Player != null && role.Player.PlayerId == player.PlayerId)
            {
                return true;
            }
        }

        return false;
    }

    public static void InfectPlayer(PlayerControl? target)
    {
        if (target == null || target.Data == null || target.Data.IsDead)
        {
            return;
        }

        if (target == PlagueDoctorPlayer || IsPlagueDoctor(target) || IsInfected(target))
        {
            return;
        }

        target.RpcAddModifier<PlagueInfectedModifier>();
    }

    public static void OnPlayerInfected(PlayerControl? player)
    {
        if (player == null)
        {
            return;
        }

        var infectDuration = OptionGroupSingleton<PlagueDoctorOptions>.Instance.InfectDuration.Value;
        InfectionProgress[player.PlayerId] = infectDuration;
    }

    public static void OnPlayerCured(PlayerControl? player)
    {
        if (player == null)
        {
            return;
        }

        InfectionProgress[player.PlayerId] = 0f;
        LastAccrueFrame.Remove(player.PlayerId);
    }

    public static void SpreadInfectionFrom(PlayerControl source)
    {
        if (source == null || source.Data == null || source.Data.IsDead)
        {
            return;
        }

        var opts = OptionGroupSingleton<PlagueDoctorOptions>.Instance;
        var infectDistance = opts.InfectDistance.Value;
        var infectDuration = opts.InfectDuration.Value;

        foreach (var target in PlayerControl.AllPlayerControls)
        {
            if (target == null || target == PlagueDoctorPlayer) continue;
            if (target.Data == null || target.Data.IsDead) continue;
            if (IsPlagueDoctor(target)) continue;
            if (target.inVent) continue;
            if (IsInfected(target)) continue;

            var distance = Vector3.Distance(source.transform.position, target.transform.position);
            if (distance > infectDistance) continue;

            var blocked = PhysicsHelpers.AnythingBetween(
                source.GetTruePosition(),
                target.GetTruePosition(),
                Constants.ShipAndObjectsMask,
                false);
            if (blocked) continue;

            if (LastAccrueFrame.TryGetValue(target.PlayerId, out var stamp) &&
                Mathf.Approximately(stamp, Time.fixedTime))
            {
                continue;
            }

            LastAccrueFrame[target.PlayerId] = Time.fixedTime;

            var progress = InfectionProgress.GetValueOrDefault(target.PlayerId, 0f) + Time.fixedDeltaTime;
            InfectionProgress[target.PlayerId] = progress;

            if (Time.time - _lastProgressSync > 0.5f)
            {
                RpcUpdateInfectionProgress(PlayerControl.LocalPlayer, target.PlayerId, progress);
                _lastProgressSync = Time.time;
            }

            if (progress >= infectDuration)
            {
                InfectPlayer(target);
            }
        }
    }

    public bool WinConditionMet()
    {
        if (Player == null) return false;
        if (!CanWinWhileDead && Player.HasDied()) return false;
        return HasInfectedAllLivingPlayers(Player);
    }

    private static bool HasInfectedAllLivingPlayers(PlayerControl player)
    {
        var livingPlayers = PlayerControl.AllPlayerControls.ToArray()
            .Where(p => p != null && p.Data != null && !p.HasDied() && p != player &&
                        p.Data.Role is not PlagueDoctorRole)
            .ToList();

        return livingPlayers.Count > 0 && livingPlayers.All(IsInfected);
    }

    public static void HandleMeetingStart()
    {
        MeetingFlag = true;
        TryTurnIntoAmnesiacWhenCannotWin();
    }

    public static void OnMeetingEnd()
    {
        UpdateDeadPlayers();
    }

    private static void TryTurnIntoAmnesiacWhenCannotWin()
    {
        if (!OptionGroupSingleton<PlagueDoctorOptions>.Instance.TurnIntoAmne)
        {
            return;
        }

        var plagueDoctor = PlayerControl.LocalPlayer;
        if (plagueDoctor == null || plagueDoctor.Data == null || plagueDoctor.Data.IsDead)
        {
            return;
        }

        if (plagueDoctor.Data.Role is not PlagueDoctorRole || PlagueDoctorPlayer != plagueDoctor)
        {
            return;
        }

        if (NumInfectionsRemaining > 0 || HasLivingInfectedPlayer())
        {
            return;
        }

        RpcTurnIntoAmnesiacWhenCannotWin(plagueDoctor, plagueDoctor.PlayerId);
    }

    private static bool HasLivingInfectedPlayer()
    {
        foreach (var p in PlayerControl.AllPlayerControls)
        {
            if (p != null && !p.HasDied() && IsInfected(p))
            {
                return true;
            }
        }

        return false;
    }

    public static void OnRoundStart()
    {
        var immunityTime = OptionGroupSingleton<PlagueDoctorOptions>.Instance.ImmunityTime.Value;
        ImmunityTimer = immunityTime;
        MeetingFlag = false;

    }

    public static void TickImmunityTimer(float deltaTime)
    {
        if (ImmunityTimer > 0f)
        {
            ImmunityTimer -= deltaTime;
            if (ImmunityTimer < 0f) ImmunityTimer = 0f;
        }
    }

    public static void UpdateDeadPlayers()
    {
        foreach (var pc in PlayerControl.AllPlayerControls)
        {
            if (pc?.Data != null)
            {
                DeadPlayers[pc.PlayerId] = pc.Data.IsDead;
            }
        }
    }

    public static void OnPlagueDoctorDeath(PlayerControl? killer)
    {
        var localPlayer = PlayerControl.LocalPlayer;
        
        if (localPlayer == null) return;
        if (killer == null) return;
        if (PlagueDoctorPlayer == null || localPlayer != PlagueDoctorPlayer) return;

        var infectKiller = OptionGroupSingleton<PlagueDoctorOptions>.Instance.InfectKiller;
        
        if (infectKiller)
        {
            InfectPlayer(killer);
        }
    }

    private static PlayerControl? GetPlayerById(byte id)
    {
        foreach (var p in PlayerControl.AllPlayerControls)
        {
            if (p != null && p.PlayerId == id)
            {
                return p;
            }
        }
        return null;
    }

    [MethodRpc((uint)DivaniRpcCalls.PlagueDoctorUpdateProgress)]
    public static void RpcUpdateInfectionProgress(PlayerControl sender, byte targetId, float progress)
    {
        InfectionProgress[targetId] = progress;
    }

    [MethodRpc((uint)DivaniRpcCalls.PlagueDoctorTurnIntoAmnesiac)]
    public static void RpcTurnIntoAmnesiacWhenCannotWin(PlayerControl sender, byte plagueDoctorId)
    {
        var plagueDoctor = GetPlayerById(plagueDoctorId);
        if (plagueDoctor == null || plagueDoctor.Data == null || plagueDoctor.Data.IsDead)
        {
            return;
        }

        if (plagueDoctor.Data.Role is not PlagueDoctorRole)
        {
            return;
        }


        ClearAndReload();
        plagueDoctor.ChangeRole(RoleId.Get<AmnesiacRole>());

        if (!plagueDoctor.AmOwner)
        {
            return;
        }

        Coroutines.Start(MiscUtils.CoFlash(TownOfUsColors.Amnesiac));
        var notification = MiraAPI.Utilities.Helpers.CreateAndShowNotification(
            $"There are no longer any living infected players, and you have no infections left. You have become an {TownOfUsColors.Amnesiac.ToTextColor()}Amnesiac</color>.",
            Color.white,
            new Vector3(0f, 1f, -20f),
            spr: TouRoleIcons.Amnesiac.LoadAsset());
        notification.AdjustNotification();
    }

    public override bool DidWin(GameOverReason gameOverReason)
    {
        return WinConditionMet() && PlagueDoctorPlayer != null && Player == PlagueDoctorPlayer;
    }

    public static void TryShowInfectionWarning()
    {
        var options = OptionGroupSingleton<PlagueDoctorOptions>.Instance;
        if (!options.NotifyPlayersWhenInfectionClose) return;
        if (PlagueDoctorPlayer == null) return;

        var uninfectedLeft = PlayerControl.AllPlayerControls.ToArray()
            .Count(p => p != null &&
                        p.Data != null &&
                        !p.HasDied() &&
                        !IsPlagueDoctor(p) &&
                        !IsInfected(p));

        if (uninfectedLeft <= 0 || uninfectedLeft > options.NotifyWhenUninfectedLeft.Value)
        {

            InfectionWarningShown = false;
            return;
        }

        if (InfectionWarningShown) return;

        InfectionWarningShown = true;
        MiraAPI.Utilities.Helpers.CreateAndShowNotification(
            $"<b><color=#FFC000>A plague is spreading. {uninfectedLeft} player(s) remain uninfected.</color></b>",
            Color.white,
            new Vector3(0f, 1f, -20f),
            spr: DivaniAssets.PlagueDoctorIcon.LoadAsset());
    }
}
