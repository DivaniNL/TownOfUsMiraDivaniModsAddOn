using System;
using System.Text;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using DivaniMods.Assets;
using DivaniMods.Events.Impostor.ImpostorPower;
using TownOfUs;
using TownOfUs.Extensions;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modules;
using TownOfUs.Modules.Wiki;
using TownOfUs.Roles;
using TownOfUs.Utilities;
using UnityEngine;
using DivaniMods.Roles.Crewmate.CrewmateKilling;
using DivaniMods.Interfaces;

namespace DivaniMods.Roles.Impostor.ImpostorPower;

public sealed class SummonerRole(IntPtr cppPtr)
    : ImpostorRole(cppPtr), IDivaniRole, IWikiDiscoverable, IDoomable, ICrewVariant
{
    public string RoleName => MiraLocaleManager.Get("DivaniMods.Role.Summoner", "Summoner");
    public string LocaleKey => "Summoner";
    public string RoleDescription => MiraLocaleManager.Get("DivaniMods.Role.Summoner.Description");
    public string RoleMedDescription => MiraLocaleManager.Get("DivaniMods.Role.Summoner.MedDescription");
    public string RoleLongDescription => MiraLocaleManager.Get("DivaniMods.Role.Summoner.LongDescription");
    public Color RoleColor => Palette.ImpostorRed;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorPower;

    public DoomableType DoomHintType => DoomableType.Death;

    public RoleBehaviour CrewVariant =>
        RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<RetributionistRole>());

    public string GetAdvancedDescription() => RoleLongDescription + MiscUtils.AppendOptionsText(GetType());

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var sb = ITownOfUsRole.SetNewTabText(this);
        var req = SummonerState.Required;
        var kills = Math.Min(SummonerState.KillsSinceRevenant, req);
        sb.AppendLine(
            $"<b>{MiraLocaleManager.Get("DivaniMods.Role.Summoner.Tab.KillsRequired")
                .Replace("<kills>", kills.ToString(TownOfUsPlugin.Culture))
                .Replace("<required>", req.ToString(TownOfUsPlugin.Culture))}</b>");
        if (RevenantActive())
        {
            sb.AppendLine(TownOfUsPlugin.Culture,
                 $"<b>{TownOfUsColors.Impostor.ToTextColor()}{MiraLocaleManager.Get("DivaniMods.Role.Summoner.Tab.RevenantActive")}</color></b>");
            sb.AppendLine($"<b>{MiraLocaleManager.Get("DivaniMods.Role.Summoner.Tab.RecruitAfterDeath")}</b>");
        }
        else if (SummonerState.SummonReady)
        {
            sb.AppendLine(TownOfUsPlugin.Culture,
                $"<b>{TownOfUsColors.Impostor.ToTextColor()}{MiraLocaleManager.Get("DivaniMods.Role.Summoner.Tab.SummonActive")}</color></b>");
        }

        return sb;
    }

    [HideFromIl2Cpp] public List<CustomButtonWikiDescription> Abilities { get; } =
    [
       new(
            MiraLocaleManager.Get("DivaniMods.Role.Summoner.Ability.Summon"),
            MiraLocaleManager.Get("DivaniMods.Role.Summoner.Ability.Summon.Description"),
            DivaniAssets.SummonerMeetingActive
        )
    ];

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = MiraAPI.Utilities.Assets.TmpSpriteUtils.CreateSpriteAsset(DivaniAssets.SummonerIcon.LoadAsset(), "DivaniMod.Role.Impostor.Summoner", 1.45f),
        OptionsScreenshot = DivaniAssets.SummonerBanner,
        Icon = DivaniAssets.SummonerIcon,
        IntroSound = DivaniAssets.SummonerIntroSound,
        MaxRoleCount = 1,
    };

    public byte PendingRecruitTargetId { get; set; } = 255;

    private MeetingMenu? _meetingMenu;
    private byte _localSelectedId = 255;

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        PendingRecruitTargetId = 255;
        _localSelectedId = 255;

        if (Player.AmOwner)
        {
            _meetingMenu = new MeetingMenu(
                this,
                OnMeetingToggle,
                MeetingAbilityType.Toggle,
                DivaniAssets.SummonerMeetingActive,
                DivaniAssets.SummonerMeetingInactive,
                IsExempt,
                activeColor: Color.white,
                disabledColor: Color.white,
                hoverColor: Color.white)
            {
                Position = new Vector3(-0.40f, 0f, -3f),
            };
        }
    }

    public override void OnMeetingStart()
    {
        RoleBehaviourStubs.OnMeetingStart(this);

        PendingRecruitTargetId = 255;
        _localSelectedId = 255;

        if (!Player.AmOwner || _meetingMenu == null)
        {
            return;
        }

        var usable = !Player.HasDied() &&
                     !Player.HasModifier<JailedModifier>() &&
                     !RevenantActive() &&
                     SummonerState.SummonReady;
        var hud = MeetingHud.Instance;
        if (hud != null)
        {
            _meetingMenu.GenButtons(hud, usable);
        }
    }

    public override void OnVotingComplete()
    {
        RoleBehaviourStubs.OnVotingComplete(this);

        if (Player.AmOwner && _meetingMenu != null)
        {
            _meetingMenu.HideButtons();
        }
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);

        if (Player.AmOwner)
        {
            _meetingMenu?.Dispose();
            _meetingMenu = null;
        }
    }

    private static bool RevenantActive()
    {
        foreach (var pc in PlayerControl.AllPlayerControls)
        {
            if (pc != null && pc.Data?.Role is DivaniMods.Roles.Impostor.ImpostorAfterlife.RevenantRole { GhostActive: true })
            {
                return true;
            }
        }

        return false;
    }

    private bool IsExempt(PlayerVoteArea voteArea)
    {
        var target = GameData.Instance.GetPlayerById(voteArea.PlayerId)?.Object;
        return !IsValidRecruitTarget(target, Player);
    }

    private void OnMeetingToggle(PlayerVoteArea voteArea, MeetingHud hud)
    {
        if (hud.state == MeetingHud.MeetingStates.Discussion || IsExempt(voteArea))
        {
            return;
        }

        if (_meetingMenu == null)
        {
            return;
        }

        if (_localSelectedId == voteArea.PlayerId)
        {
            _meetingMenu.Actives[voteArea.PlayerId] = false;
            _localSelectedId = 255;
            RpcSetPendingTarget(Player, 255);
            return;
        }

        if (_localSelectedId != 255)
        {
            _meetingMenu.Actives[_localSelectedId] = false;
        }

        _localSelectedId = voteArea.PlayerId;
        _meetingMenu.Actives[voteArea.PlayerId] = true;
        RpcSetPendingTarget(Player, voteArea.PlayerId);
    }

    [MethodRpc((uint)DivaniRpcCalls.SummonerSetPendingTarget)]
    public static void RpcSetPendingTarget(PlayerControl summoner, byte targetPlayerId)
    {
        if (summoner?.Data?.Role is not SummonerRole role)
        {
            return;
        }

        role.PendingRecruitTargetId = targetPlayerId;
    }

    internal static bool IsValidRecruitTarget(PlayerControl? target, PlayerControl summoner)
    {
        if (target == null || summoner == null)
        {
            return false;
        }

        if (target.Data == null || target.Data.Disconnected)
        {
            return false;
        }

        if (!target.HasDied())
        {
            return false;
        }

        if (target.PlayerId == summoner.PlayerId)
        {
            return false;
        }

        if (target.Data.Role is DivaniMods.Roles.Impostor.ImpostorAfterlife.RevenantRole { GhostActive: true })
        {
            return false;
        }

        return true;
    }
}
