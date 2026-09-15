using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using DivaniMods.Assets;
using DivaniMods.Events.Neutral.NeutralBenign;
using DivaniMods.Modifiers.Neutral.NeutralBenign;
using DivaniMods.Options;
using TownOfUs;
using TownOfUs.Assets;
using TownOfUs.Interfaces;
using TownOfUs.Modules.Wiki;
using TownOfUs.Modifiers.Game.Alliance;
using TownOfUs.Roles;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;
using TownOfUs.Extensions;
using DivaniMods.Interfaces;

namespace DivaniMods.Roles.Neutral.NeutralBenign;

public sealed class CupidRole(IntPtr cppPtr)
    : NeutralRole(cppPtr), IDivaniRole, IWikiDiscoverable, IDoomable, ICrewVariant, IUnlovable
{
    public static readonly Color CupidColor = new Color32(222, 125, 127, 255);

    [HideFromIl2Cpp] public List<byte> ProvisionalTargets { get; } = new();
    public bool Finalized { get; set; }
    public PlayerControl? LoverOne { get; set; }
    public PlayerControl? LoverTwo { get; set; }

    private string _lastKnownCoupleKey = string.Empty;

    public string LocaleKey => "Cupid";
    public string RoleName => MiraLocaleManager.Get("DivaniMods.Role.Cupid", "Cupid");
    public string RoleDescription => MiraLocaleManager.Get("DivaniMods.Role.Cupid.Description");
    public string RoleMedDescription => MiraLocaleManager.Get("DivaniMods.Role.Cupid.MedDescription");
    public string RoleLongDescription => MiraLocaleManager.Get("DivaniMods.Role.Cupid.LongDescription");

    public Color RoleColor => CupidColor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleAlignment RoleAlignment => RoleAlignment.NeutralBenign;

    public DoomableType DoomHintType => DoomableType.Protective;
    public bool IsUnlovable => true;

    public RoleBehaviour CrewVariant => RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<MedicRole>());

    public string GetAdvancedDescription() => RoleLongDescription + MiscUtils.AppendOptionsText(GetType());

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = MiraAPI.Utilities.Assets.TmpSpriteUtils.CreateSpriteAsset(DivaniAssets.CupidIcon.LoadAsset(), "DivaniMod.Role.Neutral.Cupid", 1.45f),
        OptionsScreenshot = DivaniAssets.CupidBanner,
        Icon = DivaniAssets.CupidIcon,
        IntroSound = DivaniAssets.CupidIntroSound,
        MaxRoleCount = 1,
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>()
    };
    [HideFromIl2Cpp] public List<CustomButtonWikiDescription> Abilities { get; } =
    [
       new(
        MiraLocaleManager.Get("DivaniMods.Role.Cupid.Ability.Matchmake"),
        MiraLocaleManager.Get("DivaniMods.Role.Cupid.Ability.Matchmake.Description"),
        DivaniAssets.CupidMatchmakeButton
    ),
    new(
        MiraLocaleManager.Get("DivaniMods.Role.Cupid.Ability.Bestow"),
        MiraLocaleManager.Get("DivaniMods.Role.Cupid.Ability.Bestow.Description"),
        DivaniAssets.CupidProtectButton
        )
    ];

    public override void SpawnTaskHeader(PlayerControl playerControl)
    {
        if (playerControl != PlayerControl.LocalPlayer)
        {
            return;
        }
        var task = PlayerTask.GetOrCreateTask<ImportantTextTask>(playerControl, 0);
        task.Text = $"{TownOfUsColors.Neutral.ToTextColor()}{MiraLocaleManager.Get("NeutralBenignTaskHeader")}</color>";
        task.name = "NeutralRoleText";
    }

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        ProvisionalTargets.Clear();
        Finalized = false;
        LoverOne = null;
        LoverTwo = null;
        _lastKnownCoupleKey = string.Empty;
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);
        TouRoleUtils.ClearTaskHeader(Player);

        foreach (var plr in PlayerControl.AllPlayerControls.ToArray())
        {
            if (plr != null && plr.HasModifier<CupidLoverRevealModifier>())
            {
                plr.RemoveModifier<CupidLoverRevealModifier>();
            }
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

    public void FixedUpdate()
    {
        if (!Player || Player.Data.Role is not CupidRole || !Player.AmOwner || !Finalized)
        {
            return;
        }

        var couple = GetCurrentCouple();
        ManageLoverReveals(couple);

        var key = CoupleKey(couple);
        if (key == _lastKnownCoupleKey || couple.Count != 2)
        {
            return;
        }

        LoverOne = couple[0];
        LoverTwo = couple[1];
        _lastKnownCoupleKey = key;

        var message = MiraLocaleManager.Get("DivaniMods.Role.Cupid.Notification.NowInLove")
            .Replace("<player1>", couple[0].Data.PlayerName)
            .Replace("<player2>", couple[1].Data.PlayerName);

        var notif = Helpers.CreateAndShowNotification(
            $"<b>{CupidColor.ToTextColor()}{message}</color></b>",
            Color.white, new Vector3(0f, 1f, -20f), spr: DivaniAssets.CupidIcon.LoadAsset());
        notif.AdjustNotification();
    }

    [HideFromIl2Cpp] public List<PlayerControl> GetCurrentCouple()
    {
        PlayerControl? anchor = null;
        if (LoverOne != null && LoverOne.HasModifier<LoverModifier>())
        {
            anchor = LoverOne;
        }
        else if (LoverTwo != null && LoverTwo.HasModifier<LoverModifier>())
        {
            anchor = LoverTwo;
        }

        if (anchor == null)
        {
            var fallback = new List<PlayerControl>();
            if (LoverOne != null) fallback.Add(LoverOne);
            if (LoverTwo != null) fallback.Add(LoverTwo);
            return fallback;
        }

        var other = anchor.GetModifier<LoverModifier>()?.OtherLover;
        var couple = new List<PlayerControl> { anchor };
        if (other != null)
        {
            couple.Add(other);
        }
        return couple;
    }

    private static string CoupleKey(List<PlayerControl> couple)
    {
        return string.Join(",", couple.Where(x => x != null).Select(x => x.PlayerId).OrderBy(x => x));
    }

    [HideFromIl2Cpp]
    private static void ManageLoverReveals(List<PlayerControl> couple)
    {
        foreach (var lover in couple)
        {
            if (lover != null && !lover.HasModifier<CupidLoverRevealModifier>())
            {
                lover.AddModifier<CupidLoverRevealModifier>();
            }
        }

        foreach (var plr in PlayerControl.AllPlayerControls.ToArray())
        {
            if (plr != null && plr.HasModifier<CupidLoverRevealModifier>() &&
                !couple.Any(c => c != null && c.PlayerId == plr.PlayerId))
            {
                plr.RemoveModifier<CupidLoverRevealModifier>();
            }
        }
    }

    [HideFromIl2Cpp]
    public void RestoreFinalizedCouple(PlayerControl? loverOne, PlayerControl? loverTwo)
    {
        LoverOne = loverOne;
        LoverTwo = loverTwo;
        Finalized = true;
        ProvisionalTargets.Clear();

        var couple = new List<PlayerControl>();
        if (loverOne != null) couple.Add(loverOne);
        if (loverTwo != null) couple.Add(loverTwo);
        _lastKnownCoupleKey = CoupleKey(couple);
    }

    public bool IsLover(PlayerControl player)
    {
        if (player == null)
        {
            return false;
        }
        return GetCurrentCouple().Any(x => x != null && x.PlayerId == player.PlayerId);
    }

    bool ICustomRole.CanLocalPlayerSeeRole(PlayerControl player)
    {
        return OptionGroupSingleton<CupidOptions>.Instance.LoversKnowCupid &&
               IsLover(PlayerControl.LocalPlayer);
    }

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var stringB = ITownOfUsRole.SetNewTabText(this);

        if (!Finalized)
        {
            stringB.Append($"\n<b>{CupidColor.ToTextColor()}{MiraLocaleManager.Get("DivaniMods.Role.Cupid.Tab.ProvisionalLovers")}</color></b>");
            foreach (var id in ProvisionalTargets)
            {
                var plr = MiscUtils.PlayerById(id);
                if (plr != null)
                {
                    stringB.Append(TownOfUsPlugin.Culture, $"\n{Color.white.ToTextColor()}{plr.Data.PlayerName}</color>");
                }
            }
            return stringB;
        }

        var cupidKnowsRoles = OptionGroupSingleton<CupidOptions>.Instance.CupidKnowsLoverRoles;
        stringB.Append($"\n<b>{CupidColor.ToTextColor()}{MiraLocaleManager.Get("DivaniMods.Role.Cupid.Tab.Lovers")}</color></b>");
        foreach (var lover in GetCurrentCouple())
        {
            if (lover == null)
            {
                continue;
            }

            var line = $"{Color.white.ToTextColor()}{lover.Data.PlayerName}</color>";
            if (cupidKnowsRoles && lover.Data.Role != null)
            {
                var roleColor = lover.Data.Role.TeamColor;
                line += $" ({roleColor.ToTextColor()}{lover.Data.Role.GetRoleName()}</color>)";
            }
            stringB.Append(TownOfUsPlugin.Culture, $"\n{line}");
        }
        return stringB;
    }

    public bool WinConditionMet()
    {
        return false;
    }

    public override bool DidWin(GameOverReason gameOverReason)
    {
        if (!Finalized)
        {
            return false;
        }

        var couple = GetCurrentCouple();
        if (couple.Count != 2 || couple.Any(p => p == null || !p.HasModifier<LoverModifier>()))
        {
            return false;
        }

        return couple.Any(p => p.GetModifiers<GameModifier>().Any(m => m.DidWin(gameOverReason) == true));
    }

    [MethodRpc((uint)DivaniRpcCalls.CupidSetMatchTarget)]
    public static void RpcSetMatchTarget(PlayerControl cupid, byte targetId)
    {
        if (cupid.Data.Role is not CupidRole role || role.Finalized)
        {
            return;
        }

        var target = MiscUtils.PlayerById(targetId);
        if (target == null || role.ProvisionalTargets.Contains(targetId))
        {
            return;
        }

        if (role.ProvisionalTargets.Count >= 2)
        {
            var oldestId = role.ProvisionalTargets[0];
            role.ProvisionalTargets.RemoveAt(0);
            var oldest = MiscUtils.PlayerById(oldestId);
            oldest?.RemoveModifier<CupidToBeLoversModifier>();
        }

        target.AddModifier<CupidToBeLoversModifier>(cupid.PlayerId);
        role.ProvisionalTargets.Add(targetId);
    }

    [MethodRpc((uint)DivaniRpcCalls.CupidFinalizeLovers)]
    public static void RpcFinalizeLovers(PlayerControl cupid, byte loverOneId, byte loverTwoId)
    {
        if (cupid.Data.Role is not CupidRole role || role.Finalized)
        {
            return;
        }

        var loverOne = MiscUtils.PlayerById(loverOneId);
        var loverTwo = MiscUtils.PlayerById(loverTwoId);
        if (loverOne == null || loverTwo == null)
        {
            DivaniPlugin.Instance.Log.LogError("Cupid RpcFinalizeLovers - lover missing");
            return;
        }

        foreach (var plr in PlayerControl.AllPlayerControls.ToArray()
                     .Where(x => x != null && x.HasModifier<CupidToBeLoversModifier>()).ToList())
        {
            plr.RemoveModifier<CupidToBeLoversModifier>();
        }

        var oneMod = loverOne.AddModifier<LoverModifier>();
        var twoMod = loverTwo.AddModifier<LoverModifier>();
        if (oneMod != null)
        {
            oneMod.OtherLover = loverTwo;
        }
        if (twoMod != null)
        {
            twoMod.OtherLover = loverOne;
        }
        if (!loverOne.IsCrewmate() || !loverTwo.IsCrewmate())
        {
            if (oneMod != null) oneMod.ForceDisableTasks = true;
            if (twoMod != null) twoMod.ForceDisableTasks = true;
        }

        role.LoverOne = loverOne;
        role.LoverTwo = loverTwo;
        role.Finalized = true;
        role.ProvisionalTargets.Clear();
        role._lastKnownCoupleKey = CoupleKey([loverOne, loverTwo]);
        CupidLoverReviveEvents.FinalizedCouples[cupid.PlayerId] = (loverOneId, loverTwoId);

        if (cupid.AmOwner)
        {
            var message = MiraLocaleManager.Get("DivaniMods.Role.Cupid.Notification.FellInLove")
                .Replace("<player1>", loverOne.Data.PlayerName)
                .Replace("<player2>", loverTwo.Data.PlayerName);

            var notif = Helpers.CreateAndShowNotification(
                $"<b>{CupidColor.ToTextColor()}{message}</color></b>",
                Color.white,
                new Vector3(0f, 1f, -20f),
                spr: DivaniAssets.CupidIcon.LoadAsset());
            notif.AdjustNotification();
        }
        else if (PlayerControl.LocalPlayer == loverOne || PlayerControl.LocalPlayer == loverTwo)
        {
            var partner = PlayerControl.LocalPlayer == loverOne ? loverTwo : loverOne;
            var message = OptionGroupSingleton<CupidOptions>.Instance.LoversKnowCupid
                ? MiraLocaleManager.Get("DivaniMods.Role.Cupid.Notification.CupidMatchedYou")
                    .Replace("<player>", partner.Data.PlayerName)
                : MiraLocaleManager.Get("DivaniMods.Role.Cupid.Notification.YouAreInLove")
                    .Replace("<player>", partner.Data.PlayerName);

            var notif = Helpers.CreateAndShowNotification(
                $"<b>{TownOfUsColors.Lover.ToTextColor()}{message}</color></b>",
                TownOfUsColors.Lover,
                new Vector3(0f, 1f, -20f),
                spr: TouModifierIcons.Lover.LoadAsset());
            notif.AdjustNotification();
        }
    }
}
