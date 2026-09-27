using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using MiraAPI.Translation;
using MiraAPI.Utilities.Assets;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using DivaniMods.Assets;
using DivaniMods.Buttons.Crewmate.CrewmateSupport;
using DivaniMods.Modifiers.Crewmate.CrewmateKilling;
using DivaniMods.Options;
using DivaniMods.Interfaces;
using TownOfUs.Assets;
using TownOfUs.Extensions;
using TownOfUs.Modules.Anims;

using TownOfUs.Modules.Wiki;
using TownOfUs.Options;
using TownOfUs.Roles;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace DivaniMods.Roles.Crewmate.CrewmateSupport;

public sealed class MoleRole(IntPtr cppPtr)
    : CrewmateRole(cppPtr), IDivaniRole, IWikiDiscoverable, IDoomable
{
    public static readonly Color MoleColor = new Color32(150, 255, 171, 255);

    [HideFromIl2Cpp] public List<Vent> Vents { get; set; } = [];

    // Vents queued by the Mole during a round, placed after the next meeting (owner-side only).
    [HideFromIl2Cpp] public List<Vector3> PendingVents { get; set; } = [];

    // Mole vent id -> remaining rounds before it collapses (only tracked when duration > 0).
    [HideFromIl2Cpp] public static Dictionary<int, int> VentRounds { get; set; } = [];

    // Local-only: seconds left before the local player gets kicked out of the mole vent network.
    [HideFromIl2Cpp] public static float VentTimeLeft { get; set; }

    
    public string RoleName => MiraLocaleManager.Get("DivaniMods.Role.Mole", "Mole");
    public string RoleDescription => MiraLocaleManager.Get("DivaniMods.Role.Mole.Description");
    public string RoleMedDescription => MiraLocaleManager.Get("DivaniMods.Role.Mole.MedDescription");
    public string RoleLongDescription => MiraLocaleManager.Get("DivaniMods.Role.Mole.LongDescription");
    public Color RoleColor => MoleColor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateSupport;

    public DoomableType DoomHintType => DoomableType.Trickster;

    public string GetAdvancedDescription() => RoleLongDescription + MiscUtils.AppendOptionsText(GetType());

    [HideFromIl2Cpp] public List<CustomButtonWikiDescription> Abilities { get; } =
    [
        new(
            MiraLocaleManager.Get("DivaniMods.Role.Mole.Ability.Dig"),
            MiraLocaleManager.Get("DivaniMods.Role.Mole.Ability.Dig.Description"),
            DivaniAssets.MoleDigButton
    )
    ];

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = MiraAPI.Utilities.Assets.TmpSpriteUtils.CreateSpriteAsset(DivaniAssets.MoleIcon.LoadAsset(), "DivaniMod.Role.Crewmate.Mole", 1.45f),
        Icon = DivaniAssets.MoleIcon,
        MaxRoleCount = 1,
        IntroSound = TouAudio.MineSound
    };

    private static Sprite? _moleVentSprite;

    private static Sprite MoleVentSprite
    {
        get
        {
            if (_moleVentSprite != null)
            {
                return _moleVentSprite;
            }

            var prop = typeof(TouAssets).GetProperty("MinerVentSprite");
            _moleVentSprite = prop?.GetValue(null) is LoadableAsset<Sprite> loadable
                ? loadable.LoadAsset()
                : DivaniAssets.MinerVentSprite.LoadAsset();
            return _moleVentSprite;
        }
    }

    public static bool MoleVentsExist =>
        ShipStatus.Instance != null && ShipStatus.Instance.AllVents.Any(v =>
            v.name.StartsWith("MoleVent") && v.gameObject.activeSelf && v.myRend.enabled);

    public static bool VentsDisabledByPlayerCount()
    {
        var aliveCount = PlayerControl.AllPlayerControls.ToArray().Count(x => !x.HasDied());
        var minimum = (int)OptionGroupSingleton<GameMechanicOptions>.Instance.PlayerCountWhenVentsDisable.Value;
        return aliveCount <= minimum;
    }

    // Roles that can vent on their own (impostors, Engineer, custom venters) keep vanilla vent rules
    // inside mole vents: no mole vent button, no vent time limit.
    public static bool IsNativeVenter(RoleBehaviour? role)
    {
        if (role == null || role is MoleRole)
        {
            return false;
        }

        if (role.IsImpostor || role is EngineerTouRole)
        {
            return true;
        }

        if (role is ICustomRole customRole && customRole.Configuration.CanUseVent)
        {
            return true;
        }

        return role is not PlumberRole && role.CanVent;
    }

    public static bool IsBlockedByPlumber(Vent? vent)
    {
        if (vent == null)
        {
            return false;
        }

        var ventId = vent.Id;
        return PlumberRole.VentFlushSet.Contains(ventId) ||
               PlumberRole.VentsBlocked.Any(x => x.Key == ventId);
    }

    public static bool CanUseMoleVents(PlayerControl player)
    {
        if (player.HasModifier<CursedModifier>())
        {
            return false;
        }

        if (player.Data?.Role is MoleRole)
        {
            return true;
        }

        return OptionGroupSingleton<MoleOptions>.Instance.VentUsage switch
        {
            MoleVentUsage.Anyone => true,
            MoleVentUsage.Crewmates => player.IsCrewmate(),
            _ => false,
        };
    }

    [MethodRpc((uint)DivaniRpcCalls.MolePlaceVent)]
    public static void RpcPlaceVent(PlayerControl player, int ventId, Vector2 position, float zAxis, bool immediate)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(player);
            return;
        }

        if (player.Data.Role is not MoleRole mole)
        {
            return;
        }

        var ventPrefab = ShipStatus.Instance.AllVents[0];
        var vent = Instantiate(ventPrefab, ventPrefab.transform.parent);
        vent.EnterVentAnim = null!;
        vent.ExitVentAnim = null!;
        if (vent.myAnim)
        {
            vent.transform.localScale = new Vector3(0.9f, 0.9f, 1);
            var collider = vent.transform.GetComponent<BoxCollider2D>();
            collider.size = new Vector2(0.75f, 0.34f);
            collider.offset = new Vector2(-0.005f, 0);
            vent.Offset = new Vector3(0, 0.15f, 0);
            vent.myAnim.Stop();
            vent.myAnim.Destroy();
            vent.myAnim = null!;
        }

        vent.myRend.sprite = MoleVentSprite;
        vent.name = $"MoleVent-{player.PlayerId}-{ventId}";

        if (!player.AmOwner && !immediate)
        {
            vent.gameObject.SetActive(false);
        }

        vent.Id = ventId;
        vent.transform.position = new Vector3(position.x, position.y, zAxis + 0.003f);

        if (mole.Vents.Count > 0)
        {
            var leftVent = mole.Vents[^1];
            vent.Left = leftVent;
            leftVent.Right = vent;
        }
        else
        {
            vent.Left = null;
        }

        vent.Right = null;
        vent.Center = null;

        var allVents = ShipStatus.Instance.AllVents.ToList();
        allVents.Add(vent);
        ShipStatus.Instance.AllVents = allVents.ToArray();

        mole.Vents.Add(vent);

        var duration = (int)OptionGroupSingleton<MoleOptions>.Instance.VentRoundDuration;
        if (duration > 0)
        {
            VentRounds[ventId] = duration;
        }
    }

    // Called on every client at the start of each non-intro round: age vents, collapse expired ones.
    public static void ProcessRoundEnd()
    {
        if ((int)OptionGroupSingleton<MoleOptions>.Instance.VentRoundDuration <= 0)
        {
            return;
        }

        var expired = new List<int>();
        foreach (var ventId in VentRounds.Keys.ToArray())
        {
            var rounds = VentRounds[ventId];
            if (rounds <= 1)
            {
                expired.Add(ventId);
            }
            else
            {
                VentRounds[ventId] = rounds - 1;
            }
        }

        foreach (var ventId in expired)
        {
            RemoveVent(ventId);
        }
    }

    public static void RemoveVent(int ventId)
    {
        VentRounds.Remove(ventId);

        if (ShipStatus.Instance == null)
        {
            return;
        }

        var vent = ShipStatus.Instance.AllVents.FirstOrDefault(v =>
            v.name.StartsWith("MoleVent") && v.Id == ventId);
        if (vent == null)
        {
            return;
        }

        var left = vent.Left;
        var right = vent.Right;
        if (left)
        {
            left.Right = right;
        }

        if (right)
        {
            right.Left = left;
        }

        ShipStatus.Instance.AllVents = ShipStatus.Instance.AllVents
            .Where(v => v.Pointer != vent.Pointer).ToArray();

        foreach (var mole in CustomRoleUtils.GetActiveRolesOfType<MoleRole>())
        {
            mole.Vents.RemoveAll(v => v == null || v.Pointer == vent.Pointer);
        }

        vent.gameObject.Destroy();
    }

    // Owner-only: place vents that were dug during the previous round (After Next Meeting mode).
    [HideFromIl2Cpp]
    public void PlacePendingVents()
    {
        if (!Player || !Player.AmOwner || PendingVents.Count == 0)
        {
            return;
        }

        foreach (var pos in PendingVents.ToArray())
        {
            RpcPlaceVent(Player, MoleDigButton.GetNextVentId(), pos, pos.z, true);
        }

        PendingVents.Clear();
    }

    public static void ClearAll()
    {
        VentRounds.Clear();
        VentTimeLeft = OptionGroupSingleton<MoleOptions>.Instance.VentTimeLimit.Value;

        foreach (var mole in CustomRoleUtils.GetActiveRolesOfType<MoleRole>())
        {
            mole.PendingVents.Clear();
            mole.Vents.Clear();
        }
    }

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var stringB = ITownOfUsRole.SetNewTabText(this);
        var opt = OptionGroupSingleton<MoleOptions>.Instance;
        var duration = (int)opt.VentRoundDuration;

        var lifeText = duration == 0
            ? MiraLocaleManager.Get("DivaniMods.Role.Mole.Tab.VentDuration.Unlimited")
            : MiraLocaleManager.Get(
                duration == 1
                    ? "DivaniMods.Role.Mole.Tab.VentDuration.OneRound"
                    : "DivaniMods.Role.Mole.Tab.VentDuration.Rounds")
                .Replace("<rounds>", duration.ToString());
        stringB.Append(
            $"\n<b><size=60%>{MiraLocaleManager.Get("DivaniMods.Role.Mole.Tab.Note")
                .Replace("<text>", lifeText)}</size></b>");

        var visText = opt.VentVisibility switch
        {
            MoleVentVisibility.AfterUse =>
                MiraLocaleManager.Get("DivaniMods.Role.Mole.Tab.Visibility.AfterUse"),
            MoleVentVisibility.AfterNextMeeting =>
                MiraLocaleManager.Get("DivaniMods.Role.Mole.Tab.Visibility.AfterNextMeeting"),
            _ => string.Empty,
        };
        if (visText != string.Empty)
        {
            stringB.Append($"\n<b><size=60%>{visText}</size></b>");
        }

        var activeVents = ShipStatus.Instance == null
            ? []
            : ShipStatus.Instance.AllVents.ToArray()
                .Where(v => v != null && v.name.StartsWith("MoleVent")).ToList();

        if (activeVents.Count > 0 || PendingVents.Count > 0)
        {
            stringB.Append($"\n<b>{MiraLocaleManager.Get("TouRolePlumberVentListTabText")}:</b>");

            foreach (var vent in activeVents)
            {
                var ventLabel = MiraLocaleManager.Get("TouRolePlumberVentLabelTabText")
                .Replace("<roomName>", MiscUtils.GetRoomName(vent.transform.position));
                var roundsText = duration != 0 && VentRounds.TryGetValue(vent.Id, out var rounds)
                    ? $": {MiraLocaleManager.Get("TouRolePlumberVentRoundsTabText")
                        .Replace("<roundsRemaining>", rounds.ToString())}"
                    : string.Empty;
                stringB.Append($"\n{ventLabel}{roundsText}");
            }

            foreach (var pos in PendingVents)
            {
                var ventLabel = MiraLocaleManager.Get("TouRolePlumberVentLabelTabText")
                    .Replace("<roomName>", MiscUtils.GetRoomName(pos));

                var prepText = MiraLocaleManager.Get("TouRolePlumberUnbuiltBarricadeTabText");
                stringB.Append($"\n<color=#BFBFBF>{ventLabel}: {prepText}</color>");
            }
        }

        return stringB;
    }

    [MethodRpc((uint)DivaniRpcCalls.MoleShowVent)]
    public static void RpcShowVent(PlayerControl player, int ventId)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(player);
            return;
        }

        if (player.Data.Role is not MoleRole mole)
        {
            return;
        }

        var vent = mole.Vents.FirstOrDefault(x => x.Id == ventId);

        if (vent != null)
        {
            vent.gameObject.SetActive(true);
        }
    }
}
