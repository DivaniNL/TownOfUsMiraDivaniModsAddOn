using System.Collections.Generic;
using System.Linq;
using System.Text;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Reactor.Networking.Attributes;
using DivaniMods.Assets;
using DivaniMods.Buttons.Modifiers;
using DivaniMods.Options;
using DivaniMods.Roles.Neutral.NeutralEvil;
using TownOfUs.Extensions;
using TownOfUs.Interfaces;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Game;
using TownOfUs.Modifiers.Game.Alliance;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Modules.Wiki;
using TownOfUs.Options;
using TownOfUs.Roles.Other;
using TownOfUs.Utilities;
using UnityEngine;
using Random = System.Random;

namespace DivaniMods.Modifiers.Game.Alliance;

public enum YinYangSide : byte
{
    Unassigned,
    Yin,
    Yang,
}

public sealed class YinYangModifier(byte side)
    : AllianceGameModifier, IWikiDiscoverable, IAssignableTargets, IUnguessableBasic
{
    public bool IsGuessable => !AnyResolved();

    public static readonly Color YinColor = new Color32(83, 80, 100, 255);
    public static readonly Color YangColor = new Color32(255, 255, 255, 255);

    public const string MarkSymbol = "▣";

    public YinYangSide Side { get; } = (YinYangSide)side;

    public bool IsYin => Side == YinYangSide.Yin;

    public Color SideColor => Side == YinYangSide.Yang ? YangColor : YinColor;
    public Color RivalColor => Side == YinYangSide.Yang ? YinColor : YangColor;
    public string RivalSideName => IsYin ? "Yang" : "Yin";

    public bool HasWon { get; set; }
    public bool HasLost { get; set; }
    public bool ShameApplied { get; set; }

    public override ModifierUiConfiguration Configuration => new(
        SideColor,
        TmpSpriteUtils.CreateSpriteAsset(DivaniAssets.YinYangIcon.LoadAsset(),
            "DivaniMod.Modifier.Alliance.YinYang", 1.45f));

    public override string ModifierName => Side switch
    {
        YinYangSide.Yin => "Yin",
        YinYangSide.Yang => "Yang",
        _ => "Yin-Yang",
    };

    public override string LocaleKey => "YinYang";
    public string ShortName => ModifierName;
    public override string Symbol => MarkSymbol;
    public override bool HideOnUi => HasWon || HasLost;
    public override float IntroSize => 3f;
    public override string IntroInfo => $"Beat {RivalSideName}!";
    public override Color FreeplayFileColor => SideColor;
    public override LoadableAsset<Sprite>? ModifierIcon => DivaniAssets.YinYangIcon;

    public new int Priority { get; set; } = 4;

    public List<CustomButtonWikiDescription> Abilities =>
    [
        new("Mark", "Mark a player with your symbol. Marking someone your rival already marked steals them.",
            IsYin ? DivaniAssets.YinMarkButton : DivaniAssets.YangMarkButton),
    ];

    public override int GetAmountPerGame() => 0;

    public override int GetAssignmentChance() => 0;

    public override int CustomAmount =>
        (int)OptionGroupSingleton<AllianceModifierOptions>.Instance.YinYangChance.Value != 0 ? 2 : 0;

    public override int CustomChance =>
        (int)OptionGroupSingleton<AllianceModifierOptions>.Instance.YinYangChance.Value;

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return base.IsModifierValidOn(role) && IsEligibleRole(role);
    }

    public static bool IsEligibleRole(RoleBehaviour? role)
    {
        return role is not null and not PlagueDoctorRole and not SpectatorRole;
    }

    public override string GetDescription()
    {
        var stringB = new StringBuilder();
        stringB.Append($"Mark every player before {RivalColor.ToTextColor()}{RivalSideName}</color> does.");

        if (Player == null || !Player.AmOwner || !GameStarted)
        {
            return stringB.ToString();
        }

        var unmarked = GetMarkTargets(true).Count(x => !IsMarkedByMe(x));
        stringB.Append($"\n<b>Players Unmarked: {SideColor.ToTextColor()}{unmarked}</color></b>");

        return stringB.ToString();
    }

    public string GetAdvancedDescription()
    {
        return "Yin and Yang are two halves of one alliance modifier, always spawned on opposing factions and " +
               "never on the same team. Both get a Mark button, and marking someone who already carries the " +
               "rival's mark steals them. A mark is only visible to the half that placed it and to fully dead " +
               "players, so marking your rival is safe cover even though they never count towards your total. " +
               "The first half holding a mark on every other living player wins alongside whichever faction " +
               "wins the game. The rival simply loses the race and plays on as their normal role, unless the " +
               "host has set the losing half to leave in shame." +
               MiscUtils.AppendOptionsText(GetType());
    }

    private static bool GameStarted => PlayerControl.LocalPlayer != null && ShipStatus.Instance != null;

    public PlayerControl? Rival => GetSide(IsYin ? YinYangSide.Yang : YinYangSide.Yin)?.Player;

    private static ModifierComponent? ComponentOf(PlayerControl? player)
    {
        if (player == null)
        {
            return null;
        }

        var component = player.GetComponent<ModifierComponent>();

        return component == null ? null : component;
    }

    private static void RemoveIfPresent<T>(ModifierComponent? component) where T : BaseModifier
    {
        if (component != null && component.HasModifier<T>())
        {
            component.RemoveModifier<T>();
        }
    }

    public static List<YinYangModifier> AllSides()
    {
        var sides = new List<YinYangModifier>();

        foreach (var player in PlayerControl.AllPlayerControls.ToArray())
        {
            var side = ComponentOf(player)?.GetModifier<YinYangModifier>();
            if (side != null)
            {
                sides.Add(side);
            }
        }

        return sides;
    }

    public static YinYangModifier? GetSide(YinYangSide side)
    {
        return AllSides().FirstOrDefault(x => x.Side == side);
    }

    public static YinYangModifier? GetSideOf(PlayerControl? player)
    {
        return ComponentOf(player)?.GetModifier<YinYangModifier>();
    }

    public static bool IsMarkedBy(PlayerControl? player, bool yin)
    {
        var component = ComponentOf(player);
        if (component == null)
        {
            return false;
        }

        return yin ? component.HasModifier<YinMarkedModifier>() : component.HasModifier<YangMarkedModifier>();
    }

    public bool IsMarkedByMe(PlayerControl? player) => IsMarkedBy(player, IsYin);

    public List<PlayerControl> GetMarkTargets(bool includeRival = false)
    {
        var rival = includeRival ? null : Rival;

        return Helpers.GetAlivePlayers()
            .Where(x => x != null && x.Data != null && !x.Data.Disconnected &&
                        x.PlayerId != Player.PlayerId &&
                        (rival == null || x.PlayerId != rival.PlayerId) &&
                        x.Data.Role is not SpectatorRole)
            .ToList();
    }

    public bool HasMarkedEveryone()
    {
        var targets = GetMarkTargets();
        return targets.Count > 0 && targets.All(IsMarkedByMe);
    }

    public static void CheckForCompletion()
    {
        if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost || AnyResolved())
        {
            return;
        }

        foreach (var side in ActiveSides())
        {
            if (side.Player == null || side.Player.HasDied() || !side.HasMarkedEveryone())
            {
                continue;
            }

            RpcYinYangWin(side.Player);
            return;
        }
    }

    public static bool AnyResolved()
    {
        return ActiveSides().Any(x => x.HasWon || x.HasLost);
    }

    public static List<YinYangModifier> ActiveSides()
    {
        return AllSides()
            .Where(x => x.Side != YinYangSide.Unassigned)
            .ToList();
    }

    public override bool? DidWin(GameOverReason reason)
    {
        return HasWon ? true : null;
    }

    public void AssignTargets()
    {
        if (!OptionGroupSingleton<RoleOptions>.Instance.IsClassicRoleAssignment)
        {
            return;
        }

        var chance = (int)OptionGroupSingleton<AllianceModifierOptions>.Instance.YinYangChance.Value;
        if (chance <= 0 || new Random().Next(1, 101) > chance)
        {
            return;
        }

        foreach (var holder in PlayerControl.AllPlayerControls.ToArray()
                     .Where(x => ComponentOf(x)?.HasModifier<YinYangModifier>() == true))
        {
            holder.RpcRemoveModifier<YinYangModifier>();
        }

        var candidates = PlayerControl.AllPlayerControls.ToArray()
            .Where(x => x != null && x.Data != null && !x.HasDied() && !x.Data.Disconnected &&
                        ComponentOf(x) is { } component &&
                        !component.HasModifier<ExecutionerTargetModifier>() &&
                        !component.HasModifier<AllianceGameModifier>() &&
                        !SpectatorRole.TrackedSpectators.Contains(x.Data.PlayerName) &&
                        IsEligibleRole(x.Data.Role))
            .ToList();

        candidates.Shuffle();

        var yin = candidates.FirstOrDefault();
        if (yin == null)
        {
            return;
        }

        var yang = candidates.FirstOrDefault(x => x.PlayerId != yin.PlayerId && FactionOf(x) != FactionOf(yin));
        if (yang == null)
        {
            DivaniPlugin.Instance.Log.LogWarning("Yin-Yang: no opposing faction available, pair not assigned");
            return;
        }

        RpcAssignYinYang(yin, yang);
    }

    private static int FactionOf(PlayerControl player)
    {
        if (player.IsImpostorAligned())
        {
            return 2;
        }

        return player.IsNeutral() ? 1 : 0;
    }

    public static void CheckAlliedPair()
    {
        if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost)
        {
            return;
        }

        if (!OptionGroupSingleton<YinYangOptions>.Instance.RemoveIfAllied.Value)
        {
            return;
        }

        var yin = GetSide(YinYangSide.Yin);
        var yang = GetSide(YinYangSide.Yang);

        if (yin?.Player == null || yang?.Player == null || !AreAllied(yin.Player, yang.Player))
        {
            return;
        }

        RpcDissolveYinYang();
    }

    public static bool AreAllied(PlayerControl? a, PlayerControl? b)
    {
        if (a == null || b == null || a.Data == null || b.Data == null || a.Data.Role == null ||
            b.Data.Role == null)
        {
            return false;
        }

        if (a.PlayerId == b.PlayerId)
        {
            return true;
        }

        if (a.HasDied() || b.HasDied())
        {
            return false;
        }

        var lover = ComponentOf(a)?.GetModifier<LoverModifier>();
        if (lover?.OtherLover != null && lover.OtherLover.PlayerId == b.PlayerId)
        {
            return true;
        }

        if (a.IsImpostorAligned() && b.IsImpostorAligned())
        {
            return true;
        }

        if (a.IsCrewmate() && b.IsCrewmate())
        {
            return true;
        }

        if (a.IsNeutral() && b.IsNeutral())
        {
            return a.Data.Role.GetType() == b.Data.Role.GetType();
        }

        return false;
    }

    [MethodRpc((uint)DivaniRpcCalls.YinYangDissolve)]
    public static void RpcDissolveYinYang()
    {
        var local = PlayerControl.LocalPlayer;
        var wasYinYanger = ComponentOf(local)?.HasModifier<YinYangModifier>() == true;

        foreach (var player in PlayerControl.AllPlayerControls.ToArray())
        {
            var component = ComponentOf(player);
            if (component == null)
            {
                continue;
            }

            RemoveIfPresent<YinYangModifier>(component);
            RemoveIfPresent<YinMarkedModifier>(component);
            RemoveIfPresent<YangMarkedModifier>(component);
        }

        HideMarkButtons();

        if (wasYinYanger)
        {
            ShowResultNotification(
                "<b>You and your rival ended up on the same side, the Yin-Yang has been broken.</b>");
        }
    }

    [MethodRpc((uint)DivaniRpcCalls.YinYangAssign)]
    public static void RpcAssignYinYang(PlayerControl yin, PlayerControl yang)
    {
        if (yin == null || yang == null || yin.PlayerId == yang.PlayerId)
        {
            return;
        }

        foreach (var player in PlayerControl.AllPlayerControls.ToArray())
        {
            RemoveIfPresent<YinYangModifier>(ComponentOf(player));
        }

        yin.AddModifier<YinYangModifier>((byte)YinYangSide.Yin);
        yang.AddModifier<YinYangModifier>((byte)YinYangSide.Yang);
    }

    [MethodRpc((uint)DivaniRpcCalls.YinYangMark)]
    public static void RpcYinYangMark(PlayerControl marker, PlayerControl target)
    {
        if (marker == null || target == null)
        {
            return;
        }

        var side = GetSideOf(marker);
        if (side == null || side.Side == YinYangSide.Unassigned)
        {
            return;
        }

        var targetComponent = ComponentOf(target);
        if (targetComponent == null)
        {
            return;
        }

        RemoveIfPresent<YinMarkedModifier>(targetComponent);
        RemoveIfPresent<YangMarkedModifier>(targetComponent);

        if (side.IsYin)
        {
            targetComponent.AddModifier<YinMarkedModifier>();
        }
        else
        {
            targetComponent.AddModifier<YangMarkedModifier>();
        }
    }

    [MethodRpc((uint)DivaniRpcCalls.YinYangWin)]
    public static void RpcYinYangWin(PlayerControl winner)
    {
        if (winner == null)
        {
            return;
        }

        var winnerSide = GetSideOf(winner);
        if (winnerSide == null || winnerSide.Side == YinYangSide.Unassigned || winnerSide.HasWon ||
            winnerSide.HasLost)
        {
            return;
        }

        winnerSide.HasWon = true;

        var loserSide = GetSide(winnerSide.IsYin ? YinYangSide.Yang : YinYangSide.Yin);
        if (loserSide != null)
        {
            loserSide.HasLost = true;
        }

        HideMarkButtons();

        var local = PlayerControl.LocalPlayer;
        if (local == null)
        {
            return;
        }

        if (local.PlayerId == winner.PlayerId)
        {
            ShowResultNotification(
                $"<b>You have marked everyone as {winnerSide.SideColor.ToTextColor()}{winnerSide.ModifierName}</color>, " +
                "you win no matter who takes the game!</b>");
            return;
        }

        if (loserSide?.Player != null && local.PlayerId == loserSide.Player.PlayerId)
        {
            ShowResultNotification(
                $"<b>{winnerSide.SideColor.ToTextColor()}{winnerSide.ModifierName}</color> marked everyone first, " +
                $"you have lost as {loserSide.SideColor.ToTextColor()}{loserSide.ModifierName}</color>.</b>");
        }
    }

    public static void HideMarkButtons()
    {
        var role = PlayerControl.LocalPlayer?.Data?.Role;
        if (role == null)
        {
            return;
        }

        CustomButtonSingleton<YinYangMarkButton>.Instance.SetActive(false, role);
    }

    public static void ShowResultNotification(string message)
    {
        var notification = Helpers.CreateAndShowNotification(
            message,
            Color.white,
            new Vector3(0f, 1f, -20f),
            spr: DivaniAssets.YinYangIcon.LoadAsset());

        notification.AdjustNotification();
    }
}
