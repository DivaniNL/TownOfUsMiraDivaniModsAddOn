using System;
using System.Collections.Generic;
using System.Linq;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using DivaniMods.Assets;
using DivaniMods.Buttons;
using DivaniMods.Options;
using TownOfUs.Modules.Wiki;
using TownOfUs.Roles;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;

namespace DivaniMods.Roles;

public sealed class PlagueDoctorRole(IntPtr cppPtr) : NeutralRole(cppPtr), ITownOfUsRole, IWikiDiscoverable
{
    public static readonly Color PlagueDoctorColor = new Color32(255, 192, 0, 255);

    // Static data that persists across role changes (death)
    public static Dictionary<byte, bool> InfectedPlayers { get; } = new();
    public static Dictionary<byte, float> InfectionProgress { get; } = new();
    public static Dictionary<byte, bool> DeadPlayers { get; } = new();
    public static bool TriggerPlagueDoctorWin { get; set; }
    public static PlayerControl? PlagueDoctorPlayer { get; private set; }
    public static TMPro.TMP_Text? StatusText { get; set; }
    
    public static int NumInfectionsRemaining { get; set; }
    public static bool MeetingFlag { get; set; }
    public static float ImmunityTimer { get; set; }

    public string RoleName => "Plague Doctor";
    public string RoleDescription => "Infect everyone to win!";
    public string RoleLongDescription => "You are a Plague Doctor.\n" +
        "Use your ability to infect players directly.\n" +
        "Infected players will spread the disease\nto others who stand near them.\n" +
        "Win by infecting all living players!";
    public Color RoleColor => PlagueDoctorColor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleAlignment RoleAlignment => RoleAlignment.NeutralKilling;

    public string GetAdvancedDescription() => RoleLongDescription + MiscUtils.AppendOptionsText(GetType());

    public bool HasImpostorVision => true;

    private float _lastProgressUpdate;

    public CustomRoleConfiguration Configuration => new(this)
    {
        TasksCountForProgress = false,
        CanUseVent = OptionGroupSingleton<PlagueDoctorOptions>.Instance.CanVent,
        Icon = DivaniAssets.PlagueDoctorIcon,
        IntroSound = DivaniAssets.PlagueDoctorIntroSound,
    };

    public static bool CanWinWhileDead => OptionGroupSingleton<PlagueDoctorOptions>.Instance.CanWinDead;

    public override void Initialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Initialize(this, targetPlayer);
        
        // Set PlagueDoctorPlayer on ALL clients so everyone knows who the PD is
        if (PlagueDoctorPlayer == null)
        {
            PlagueDoctorPlayer = targetPlayer;
            DivaniPlugin.Instance.Log.LogInfo($"PlagueDoctor: Player {targetPlayer.PlayerId} initialized as Plague Doctor (AmOwner: {targetPlayer.AmOwner})");
        }
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);
        TouRoleUtils.ClearTaskHeader(Player);
    }

    public static void ClearAndReload()
    {
        InfectedPlayers.Clear();
        InfectionProgress.Clear();
        DeadPlayers.Clear();
        TriggerPlagueDoctorWin = false;
        PlagueDoctorPlayer = null;
        NumInfectionsRemaining = (int)OptionGroupSingleton<PlagueDoctorOptions>.Instance.MaxInfections;
        MeetingFlag = false;
        ImmunityTimer = 0f;

        if (StatusText != null)
        {
            UnityEngine.Object.Destroy(StatusText.gameObject);
            StatusText = null;
        }
        
        DivaniPlugin.Instance.Log.LogInfo("PlagueDoctor: Static data cleared");
    }

    public override void SpawnTaskHeader(PlayerControl playerControl)
    {
        if (playerControl != PlayerControl.LocalPlayer)
        {
            return;
        }

        ImportantTextTask orCreateTask = PlayerTask.GetOrCreateTask<ImportantTextTask>(playerControl, 0);
        orCreateTask.Text = $"{PlagueDoctorColor.ToTextColor()}Infect everyone to win!</color>";
        orCreateTask.name = "PlagueDoctorRoleText";
    }

    // Note: FixedUpdateHandler logic is now in PlagueDoctorPatch.HudManagerUpdate

    private static void UpdateImmunityTimer()
    {
        if (ImmunityTimer > 0)
        {
            ImmunityTimer -= Time.fixedDeltaTime;
            if (ImmunityTimer <= 0)
            {
                MeetingFlag = false;
            }
        }
    }

    private void UpdateInfectionSpread()
    {
        if (MeetingFlag || MeetingHud.Instance != null) return;
        
        var localPlayer = PlayerControl.LocalPlayer;
        if (localPlayer == null) return;
        if (!CanWinWhileDead && localPlayer.Data.IsDead) return;

        var infectDistance = OptionGroupSingleton<PlagueDoctorOptions>.Instance.InfectDistance;
        var infectDuration = OptionGroupSingleton<PlagueDoctorOptions>.Instance.InfectDuration;

        foreach (var target in PlayerControl.AllPlayerControls)
        {
            if (target == null || target == PlagueDoctorPlayer) continue;
            if (target.Data == null || target.Data.IsDead) continue;
            if (target.inVent) continue;
            if (InfectedPlayers.ContainsKey(target.PlayerId)) continue;

            if (!InfectionProgress.ContainsKey(target.PlayerId))
            {
                InfectionProgress[target.PlayerId] = 0f;
            }

            foreach (var infectedId in InfectedPlayers.Keys.ToList())
            {
                var source = GetPlayerById(infectedId);
                if (source == null || source.Data == null || source.Data.IsDead) continue;

                var distance = Vector3.Distance(source.transform.position, target.transform.position);
                var blocked = PhysicsHelpers.AnythingBetween(
                    source.GetTruePosition(),
                    target.GetTruePosition(),
                    Constants.ShipAndObjectsMask,
                    false);

                if (distance <= infectDistance && !blocked)
                {
                    InfectionProgress[target.PlayerId] += Time.fixedDeltaTime;

                    if (Time.time - _lastProgressUpdate > 0.5f)
                    {
                        RpcUpdateInfectionProgress(localPlayer, target.PlayerId, InfectionProgress[target.PlayerId]);
                        _lastProgressUpdate = Time.time;
                    }

                    break;
                }
            }

            if (InfectionProgress[target.PlayerId] >= infectDuration)
            {
                RpcSetInfected(localPlayer, target.PlayerId);
            }
        }
    }

    private static void UpdateStatusText()
    {
        if (MeetingHud.Instance != null)
        {
            if (StatusText != null)
            {
                StatusText.gameObject.SetActive(false);
            }
            return;
        }

        var localPlayer = PlayerControl.LocalPlayer;
        if (localPlayer == null) return;

        if (StatusText == null)
        {
            CreateStatusText();
        }

        if (StatusText == null) return;

        StatusText.gameObject.SetActive(true);

        var infectDuration = OptionGroupSingleton<PlagueDoctorOptions>.Instance.InfectDuration;
        var text = $"<color=#FFC000>[Infection Progress]</color>\n";

        foreach (var p in PlayerControl.AllPlayerControls)
        {
            if (p == null || p == PlagueDoctorPlayer) continue;
            if (DeadPlayers.ContainsKey(p.PlayerId) && DeadPlayers[p.PlayerId]) continue;
            if (p.Data == null || p.Data.IsDead) continue;

            text += $"{p.Data.PlayerName}: ";

            if (InfectedPlayers.ContainsKey(p.PlayerId))
            {
                text += "<color=#FF0000>INFECTED</color>";
            }
            else
            {
                var progress = InfectionProgress.GetValueOrDefault(p.PlayerId, 0f);
                var percent = Mathf.Clamp01(progress / infectDuration);
                var color = GetProgressColor(percent);
                text += $"<color={ColorToHex(color)}>{(percent * 100f):F1}%</color>";
            }

            text += "\n";
        }

        StatusText.text = text;
    }

    private static void CreateStatusText()
    {
        if (HudManager.Instance?.roomTracker == null) return;

        var gameObject = UnityEngine.Object.Instantiate(HudManager.Instance.roomTracker.gameObject);
        gameObject.transform.SetParent(HudManager.Instance.transform);
        gameObject.SetActive(true);

        var roomTracker = gameObject.GetComponent<RoomTracker>();
        if (roomTracker != null)
        {
            UnityEngine.Object.DestroyImmediate(roomTracker);
        }

        StatusText = gameObject.GetComponent<TMPro.TMP_Text>();

        var aliveCount = PlayerControl.AllPlayerControls.ToArray()
            .Count(x => x != null && x.Data != null && !x.Data.IsDead && x != PlagueDoctorPlayer);

        gameObject.transform.localPosition = new Vector3(-2.7f, -0.1f - aliveCount * 0.07f, gameObject.transform.localPosition.z);
        StatusText.transform.localScale = Vector3.one;
        StatusText.fontSize = 1.5f;
        StatusText.fontSizeMin = 1.5f;
        StatusText.fontSizeMax = 1.5f;
        StatusText.alignment = TMPro.TextAlignmentOptions.BottomLeft;
    }

    private static Color GetProgressColor(float percent)
    {
        if (percent < 0.5f)
        {
            return Color.Lerp(Color.green, Color.yellow, percent * 2f);
        }
        return Color.Lerp(Color.yellow, Color.red, (percent * 2f) - 1f);
    }

    private static string ColorToHex(Color color)
    {
        return $"#{ColorUtility.ToHtmlStringRGB(color)}";
    }

    public static bool WinConditionMet()
    {
        if (PlagueDoctorPlayer == null) return false;
        if (!CanWinWhileDead && PlagueDoctorPlayer.Data.IsDead) return false;

        var livingPlayers = PlayerControl.AllPlayerControls.ToArray()
            .Where(p => p != null && p.Data != null && !p.Data.IsDead && p != PlagueDoctorPlayer)
            .ToList();

        if (livingPlayers.Count == 0) return false;

        return livingPlayers.All(p => InfectedPlayers.ContainsKey(p.PlayerId));
    }

    // Note: CheckWinCondition logic is now in PlagueDoctorPatch

    public static void HandleMeetingStart()
    {
        MeetingFlag = true;
    }

    public static void OnMeetingEnd()
    {
        UpdateDeadPlayers();

        if (StatusText != null)
        {
            UnityEngine.Object.Destroy(StatusText.gameObject);
            StatusText = null;
        }

        DivaniPlugin.Instance.Log.LogInfo("PlagueDoctor: Meeting ended (ejection starting)");
    }

    /// <summary>
    /// Called when the round actually starts (players can move again, after any
    /// ejection animation). This is when the immunity grace period should begin
    /// so the timer isn't eaten up by the ejection sequence.
    /// </summary>
    public static void OnRoundStart()
    {
        var immunityTime = OptionGroupSingleton<PlagueDoctorOptions>.Instance.ImmunityTime;
        ImmunityTimer = immunityTime;
        MeetingFlag = false;

        DivaniPlugin.Instance.Log.LogInfo($"PlagueDoctor: Round started, ImmunityTimer={immunityTime}");
    }

    /// <summary>
    /// Ticks down the immunity timer. Called each frame from HudManagerUpdate
    /// while gameplay is active (meeting/ejection paused in the patch).
    /// </summary>
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
        if (StatusText != null)
        {
            UnityEngine.Object.Destroy(StatusText.gameObject);
            StatusText = null;
        }
        
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
        DivaniPlugin.Instance.Log.LogInfo($"PlagueDoctor.OnDeath - LocalPlayer: {localPlayer?.PlayerId}, PDPlayer: {PlagueDoctorPlayer?.PlayerId}, Killer: {killer?.PlayerId}");
        
        if (localPlayer == null) return;
        if (killer == null) return;
        if (PlagueDoctorPlayer == null || localPlayer != PlagueDoctorPlayer) return;

        var infectKiller = OptionGroupSingleton<PlagueDoctorOptions>.Instance.InfectKiller;
        DivaniPlugin.Instance.Log.LogInfo($"PlagueDoctor.OnDeath - InfectKiller option: {infectKiller}");
        
        if (infectKiller)
        {
            DivaniPlugin.Instance.Log.LogInfo($"PlagueDoctor.OnDeath - Calling RpcSetInfected for killer {killer.PlayerId}");
            RpcSetInfected(localPlayer, killer.PlayerId);
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

    [MethodRpc((uint)DivaniRpcCalls.PlagueDoctorSetInfected)]
    public static void RpcSetInfected(PlayerControl sender, byte targetId)
    {
        if (!InfectedPlayers.ContainsKey(targetId))
        {
            InfectedPlayers[targetId] = true;
            DivaniPlugin.Instance.Log.LogInfo($"PlagueDoctor: Player {targetId} infected");
        }
    }

    [MethodRpc((uint)DivaniRpcCalls.PlagueDoctorUpdateProgress)]
    public static void RpcUpdateInfectionProgress(PlayerControl sender, byte targetId, float progress)
    {
        InfectionProgress[targetId] = progress;
    }

    [MethodRpc((uint)DivaniRpcCalls.PlagueDoctorWin)]
    public static void RpcTriggerPlagueDoctorWin(PlayerControl sender)
    {
        DivaniPlugin.Instance.Log.LogInfo($"PlagueDoctor: Win RPC received! AmHost: {AmongUsClient.Instance.AmHost}");
        TriggerPlagueDoctorWin = true;
        
        // Only the host should trigger the actual game end
        if (AmongUsClient.Instance.AmHost && PlagueDoctorPlayer != null)
        {
            DivaniPlugin.Instance.Log.LogInfo("PlagueDoctor: Host triggering CustomGameOver and ending game");
            
            var winners = new NetworkedPlayerInfo[] { PlagueDoctorPlayer.Data };
            MiraAPI.GameEnd.CustomGameOver.Trigger<DivaniMods.GameOver.PlagueDoctorGameOver>(winners);
        }
    }

    public override bool DidWin(GameOverReason gameOverReason)
    {
        // Only return true if OUR CustomGameOver is active
        // This ensures PD doesn't appear in other team's wins
        if (MiraAPI.GameEnd.CustomGameOver.Instance is DivaniMods.GameOver.PlagueDoctorGameOver)
        {
            return true;
        }
        
        return false;
    }
}
