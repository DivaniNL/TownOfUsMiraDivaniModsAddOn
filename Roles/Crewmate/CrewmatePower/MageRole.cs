using Il2CppInterop.Runtime.Attributes;
using System;
using System.Collections.Generic;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using Reactor.Networking.Attributes;
using DivaniMods.Assets;
using DivaniMods.Buttons.Crewmate.CrewmatePower;
using DivaniMods.Modules;
using DivaniMods.Options;
using TownOfUs.Modules.Wiki;
using TownOfUs.Roles;
using TownOfUs.Utilities;
using UnityEngine;
using TownOfUs.Extensions;

namespace DivaniMods.Roles.Crewmate.CrewmatePower;

public enum MageSpell
{
    ShockShield,
    Energize,
    Illusion,
}

public sealed class MageRole(IntPtr cppPtr)
    : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public static readonly Color MageColor = new Color32(0x15, 0x86, 0xA2, 255);

    public string RoleName => "Mage";
    public string RoleDescription => "Cast spells to aid your team!";
    public string RoleLongDescription =>
        "Use your knowledge of magic to help the crew and weaken the impostors";
    public Color RoleColor => MageColor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmatePower;

    public override bool IsAffectedByComms => false;

    public DoomableType DoomHintType => DoomableType.Trickster;

    public string GetAdvancedDescription() => "The Mage is a Crewmate Power role that has three spell abilities tied to one cooldown, either to help the crew or weaken the impostors" + MiscUtils.AppendOptionsText(GetType());

    [HideFromIl2Cpp] public List<CustomButtonWikiDescription> Abilities { get; } =
    [
        new("Shock Shield", "Give a player a temporary shock shield. Any killer who attacks them dies instead.", DivaniAssets.MageShockShieldButton),
        new("Energize", "Add an ability use to crewmates, reduce one from anyone else.", DivaniAssets.MageEnergizeButton),
        new("Illusion", "Hide a player from killers for a short time.", DivaniAssets.MageIllusionButton),
    ];

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = DivaniAssets.MageIcon,
        IntroSound = DivaniAssets.MageIntroSound,
        OptionsScreenshot = DivaniAssets.MageBanner,
        MaxRoleCount = 1,
    };

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);

        if (!Player.AmOwner)
        {
            return;
        }

        var spells = CustomButtonSingleton<MageSpellButton>.Instance;
        var opts = OptionGroupSingleton<MageOptions>.Instance;
        spells.ShockShieldUsesLeft = (int)opts.MaxShockShieldUses.Value == 0 ? -1 : (int)opts.MaxShockShieldUses.Value;
        spells.EnergizeUsesLeft = (int)opts.MaxEnergizeUses.Value == 0 ? -1 : (int)opts.MaxEnergizeUses.Value;
        spells.IllusionUsesLeft = (int)opts.MaxIllusionUses.Value == 0 ? -1 : (int)opts.MaxIllusionUses.Value;

        spells.OverrideName(MageSpellButton.SpellNames[(int)spells.CurrentSpell]);
    }

    public void LobbyStart()
    {
        var spells = CustomButtonSingleton<MageSpellButton>.Instance;
        spells.CurrentSpell = MageSpell.ShockShield;
        spells.ShockShieldUsesLeft = -2;
        spells.EnergizeUsesLeft = -2;
        spells.IllusionUsesLeft = -2;
    }

    [MethodRpc((uint)DivaniRpcCalls.MageEnergize)]
    public static void RpcEnergize(PlayerControl mage, PlayerControl target)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(mage);
            return;
        }
        if (mage == null || target == null)
        {
            return;
        }
        if (mage.AmOwner)
        {
            MiraAPI.Utilities.Helpers.CreateAndShowNotification(
                $"<b><color=#1586a2>Energized {target.Data?.PlayerName}</color></b>",
                Color.white,
                new Vector3(0f, 1f, -20f),
                spr: DivaniAssets.MageIcon.LoadAsset());
        }

        if (target.AmOwner)
        {
            var opts = OptionGroupSingleton<MageOptions>.Instance;
            bool isBuff;
            if (target.IsCrewmate())
            {
                isBuff = true;
            }
            else if ((target.Data?.Role as ITownOfUsRole)?.RoleAlignment == RoleAlignment.NeutralBenign)
            {
                var mode = (EnergizeNeutralBenignMode)opts.EnergizeNeutralBenign.Value;
                if (mode == EnergizeNeutralBenignMode.None)
                {
                    return;
                }
                isBuff = mode == EnergizeNeutralBenignMode.Buff;
            }
            else
            {
                isBuff = false;
            }

            if ((EnergizeTiming)opts.EnergizeApplyTiming.Value == EnergizeTiming.AfterDelay)
            {
                MageEnergize.ApplyAfterDelay(target, isBuff, opts.EnergizeDelay.Value);
            }
            else
            {
                MageEnergize.QueuePending(isBuff);
            }
        }
    }

    [MethodRpc((uint)DivaniRpcCalls.MageShockShieldAttacked)]
    public static void RpcShockShieldAttacked(PlayerControl mage, PlayerControl source, PlayerControl shielded)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(mage);
            return;
        }

        if (mage != null && mage.AmOwner)
        {
            var targetName = shielded?.Data?.PlayerName ?? "your target";
            MiraAPI.Utilities.Helpers.CreateAndShowNotification(
                $"<b><color=#1586a2>Your Shock Shield on {targetName} struck an attacker!</color></b>",
                Color.white,
                new Vector3(0f, 1f, -20f),
                spr: DivaniAssets.MageIcon.LoadAsset());
        }
    }
}
