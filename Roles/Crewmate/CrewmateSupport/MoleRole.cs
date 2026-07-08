using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Roles;
using MiraAPI.Utilities.Assets;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using DivaniMods.Assets;
using DivaniMods.Buttons.Crewmate.CrewmateSupport;
using DivaniMods.Options;
using TownOfUs.Assets;
using TownOfUs.Extensions;
using TownOfUs.Modules.Anims;
using TownOfUs.Modules.Localization;
using TownOfUs.Modules.Wiki;
using TownOfUs.Options;
using TownOfUs.Roles;
using TownOfUs.Utilities;
using UnityEngine;

namespace DivaniMods.Roles.Crewmate.CrewmateSupport;

public sealed class MoleRole(IntPtr cppPtr)
    : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public static readonly Color MoleColor = new Color32(150, 255, 171, 255);

    [HideFromIl2Cpp] public List<Vent> Vents { get; set; } = [];

    // Vents queued by the Mole during a round, placed after the next meeting (owner-side only).
    [HideFromIl2Cpp] public List<Vector3> PendingVents { get; set; } = [];

    // Mole vent id -> remaining rounds before it collapses (only tracked when duration > 0).
    [HideFromIl2Cpp] public static Dictionary<int, int> VentRounds { get; set; } = [];

    public string RoleName => "Mole";
    public string RoleDescription => "Dig your own tunnel network!";
    public string RoleLongDescription => "Dig vents around the map to connect a tunnel network.";
    public Color RoleColor => MoleColor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateSupport;

    public DoomableType DoomHintType => DoomableType.Trickster;

    public string GetAdvancedDescription() => RoleLongDescription + MiscUtils.AppendOptionsText(GetType());

    [HideFromIl2Cpp] public List<CustomButtonWikiDescription> Abilities { get; } =
    [
        new("Dig", "Dig a vent at your current position. All Mole vents connect to each other.", DivaniAssets.MoleDigButton)
    ];

    public CustomRoleConfiguration Configuration => new(this)
    {
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

    public static bool CanUseMoleVents(PlayerControl player)
    {
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

        vent.numFramesUntilPlayerDisappearsOnEnter = 0;
        vent.numFramesUntilPlayerReappearsOnExit = 0;
        vent.myRend.sprite = MoleVentSprite;
        vent.name = $"MoleVent-{player.PlayerId}-{ventId}";

        if (!player.AmOwner && !immediate)
        {
            vent.gameObject.SetActive(false);
        }

        vent.Id = ventId;
        vent.transform.position = new Vector3(position.x, position.y, zAxis + 0.001f);

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
            ? "Dug vents last the whole game."
            : $"Dug vents collapse after {duration} round{(duration == 1 ? string.Empty : "s")}.";
        stringB.Append($"\n<b><size=60%>Note: {lifeText}</size></b>");

        var visText = opt.VentVisibility switch
        {
            MoleVentVisibility.AfterUse => "Vents stay hidden until first used.",
            MoleVentVisibility.AfterNextMeeting => "Dug vents only appear after the next meeting.",
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
            stringB.Append($"\n<b>{TouLocale.GetParsed("TouRolePlumberVentListTabText")}:</b>");

            foreach (var vent in activeVents)
            {
                var ventLabel = TouLocale.GetParsed("TouRolePlumberVentLabelTabText")
                    .Replace("<roomName>", MiscUtils.GetRoomName(vent.transform.position));
                var roundsText = duration != 0 && VentRounds.TryGetValue(vent.Id, out var rounds)
                    ? $": {TouLocale.GetParsed("TouRolePlumberVentRoundsTabText").Replace("<roundsRemaining>", rounds.ToString())}"
                    : string.Empty;
                stringB.Append($"\n{ventLabel}{roundsText}");
            }

            foreach (var pos in PendingVents)
            {
                var ventLabel = TouLocale.GetParsed("TouRolePlumberVentLabelTabText")
                    .Replace("<roomName>", MiscUtils.GetRoomName(pos));
                var prepText = TouLocale.GetParsed("TouRolePlumberUnbuiltBarricadeTabText");
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
