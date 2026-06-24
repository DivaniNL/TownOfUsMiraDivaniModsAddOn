using System;
using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using DivaniMods.Assets;
using DivaniMods.Options;
using TownOfUs;
using TownOfUs.Assets;
using TownOfUs.Extensions;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modules;
using TownOfUs.Modules.Wiki;
using TownOfUs.Roles;
using TownOfUs.Utilities;
using UnityEngine;

namespace DivaniMods.Roles.Impostor.ImpostorPower;

public sealed class ObfuscatorRole(IntPtr cppPtr)
    : ImpostorRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public string RoleName => "Obfuscator";
    public string LocaleKey => "Obfuscator";
    public string RoleDescription => "Transfer votes to rig crewmates!";
    public string RoleLongDescription =>
    "Transfer votes between Unsuspecting Crewmates. Always goes after regular Swapper";
    public Color RoleColor => Palette.ImpostorRed;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorPower;

    public DoomableType DoomHintType => DoomableType.Trickster;

    public string GetAdvancedDescription() => RoleLongDescription + MiscUtils.AppendOptionsText(GetType());

    [HideFromIl2Cpp] public List<CustomButtonWikiDescription> Abilities { get; } =
    [
        new("Transfer Votes", "Select two players in a meeting that will swap votes at the end.", DivaniAssets.ObfuscateActive)
    ];

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = DivaniAssets.ObfuscatorIcon,
        IntroSound = DivaniAssets.ObfuscatorIntro,
        MaxRoleCount = 1,
    };

    private MeetingMenu? _meetingMenu;

    [HideFromIl2Cpp] public PlayerVoteArea? Swap1 { get; set; }
    [HideFromIl2Cpp] public PlayerVoteArea? Swap2 { get; set; }

    public int ChargesRemaining { get; set; }
    public int KillsSinceLastCharge { get; set; }

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var sb = ITownOfUsRole.SetNewTabText(this);
        var killsPer = (int)OptionGroupSingleton<ObfuscatorOptions>.Instance.KillsPerExtraCharge.Value;
        sb.AppendLine(TownOfUsPlugin.Culture, $"<b>Charges: {ChargesRemaining}</b>");
        if (killsPer > 0)
        {
            var capped = Math.Min(KillsSinceLastCharge, killsPer);
            sb.AppendLine(TownOfUsPlugin.Culture, $"<b>Kills toward next charge: {capped}/{killsPer}</b>");
        }
        return sb;
    }

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);

        ChargesRemaining = (int)OptionGroupSingleton<ObfuscatorOptions>.Instance.InitialCharges.Value;
        KillsSinceLastCharge = 0;
        Swap1 = null;
        Swap2 = null;

        if (Player.AmOwner)
        {
            _meetingMenu = new MeetingMenu(this, SetActive, MeetingAbilityType.Toggle,
                DivaniAssets.ObfuscateActive, DivaniAssets.ObfuscateInactive, IsExempt,
                activeColor: Color.white, disabledColor: Color.white, hoverColor: Color.white)
            {
                Position = new Vector3(-0.40f, 0f, -3f),
            };
        }
    }

    public override void OnMeetingStart()
    {
        RoleBehaviourStubs.OnMeetingStart(this);

        Swap1 = null;
        Swap2 = null;

        if (!Player.AmOwner || _meetingMenu == null)
        {
            return;
        }

        var usable = !Player.HasDied()
                     && !Player.HasModifier<JailedModifier>()
                     && ChargesRemaining > 0;
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

    private static bool IsExempt(PlayerVoteArea voteArea)
    {
        var target = GameData.Instance.GetPlayerById(voteArea.TargetPlayerId)?.Object;
        if (target == null || target.Data == null) return true;
        return target.Data.Disconnected || target.Data.IsDead || target.HasModifier<JailedModifier>();
    }

    private void SetActive(PlayerVoteArea voteArea, MeetingHud meeting)
    {
        if (meeting.state == MeetingHud.VoteStates.Discussion || IsExempt(voteArea))
        {
            return;
        }

        if (_meetingMenu == null) return;

        if (!Swap1)
        {
            Swap1 = voteArea;
            _meetingMenu.Actives[voteArea.TargetPlayerId] = true;
        }
        else if (!Swap2)
        {
            Swap2 = voteArea;
            _meetingMenu.Actives[voteArea.TargetPlayerId] = true;
        }
        else if (Swap1 == voteArea)
        {
            _meetingMenu.Actives[Swap1!.TargetPlayerId] = false;
            Swap1 = null;
        }
        else if (Swap2 == voteArea)
        {
            _meetingMenu.Actives[Swap2!.TargetPlayerId] = false;
            Swap2 = null;
        }
        else
        {
            _meetingMenu.Actives[Swap1!.TargetPlayerId] = false;
            Swap1 = Swap2;
            Swap2 = voteArea;
            _meetingMenu.Actives[voteArea.TargetPlayerId] = !_meetingMenu.Actives[voteArea.TargetPlayerId];
        }

        RpcSyncSwaps(Player, Swap1?.TargetPlayerId ?? 255, Swap2?.TargetPlayerId ?? 255);
    }

    [MethodRpc((uint)DivaniRpcCalls.ObfuscatorSetSwaps)]
    public static void RpcSyncSwaps(PlayerControl obfuscator, byte swap1, byte swap2)
    {
        if (obfuscator?.Data?.Role is not ObfuscatorRole role) return;
        if (MeetingHud.Instance == null) return;

        var areas = MeetingHud.Instance.playerStates.ToList();
        role.Swap1 = areas.Find(x => x.TargetPlayerId == swap1);
        role.Swap2 = areas.Find(x => x.TargetPlayerId == swap2);
    }

    [MethodRpc((uint)DivaniRpcCalls.ObfuscatorConsumeCharge)]
    public static void RpcConsumeCharge(PlayerControl obfuscator)
    {
        if (obfuscator?.Data?.Role is not ObfuscatorRole role) return;
        if (role.ChargesRemaining > 0)
        {
            role.ChargesRemaining--;
        }
    }
}
