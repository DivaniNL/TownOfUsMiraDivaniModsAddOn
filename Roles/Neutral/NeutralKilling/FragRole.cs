using Il2CppInterop.Runtime.Attributes;
using System;
using System.Linq;
using AmongUs.GameOptions;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using Reactor.Utilities;
using DivaniMods.Assets;
using DivaniMods.Buttons.Neutral.NeutralKilling;
using DivaniMods.Options;
using TownOfUs;
using TownOfUs.Assets;
using TownOfUs.Buttons;
using TownOfUs.Extensions;
using TownOfUs.Modules.Wiki;
using TownOfUs.Roles;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;
using DivaniMods.Interfaces;

namespace DivaniMods.Roles.Neutral.NeutralKilling;

public sealed class FragRole(IntPtr cppPtr)
    : NeutralRole(cppPtr), IDivaniRole, IWikiDiscoverable, IDoomable, ICrewVariant
{
    public static readonly Color FragColor = new Color32(232, 168, 124, 255);

    public string RoleName => MiraLocaleManager.Get("DivaniMods.Role.Frag", "Frag");
    public string RoleDescription => MiraLocaleManager.Get("DivaniMods.Role.Frag.Description");
    public string RoleMedDescription => MiraLocaleManager.Get("DivaniMods.Role.Frag.MedDescription");
    public string RoleLongDescription => MiraLocaleManager.Get("DivaniMods.Role.Frag.LongDescription");
    public Color RoleColor => FragColor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleAlignment RoleAlignment => RoleAlignment.NeutralKilling;

    public DoomableType DoomHintType => DoomableType.Fearmonger;

    public RoleBehaviour CrewVariant =>
        RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<TrapperRole>());

    public string GetAdvancedDescription() => RoleLongDescription + MiscUtils.AppendOptionsText(GetType());

    public bool HasImpostorVision => true;

    [HideFromIl2Cpp] public List<CustomButtonWikiDescription> Abilities { get; } =
    [
        new(
            MiraLocaleManager.Get("DivaniMods.Role.Frag.Ability.GiveFrag"),
            MiraLocaleManager.Get("DivaniMods.Role.Frag.Ability.GiveFrag.Description"),
            DivaniAssets.FragGiveButton
        ),
        new(
            MiraLocaleManager.Get("DivaniMods.Role.Frag.Ability.PassFrag"),
            MiraLocaleManager.Get("DivaniMods.Role.Frag.Ability.PassFrag.Description"),
            DivaniAssets.FragPassButton
        )
    ];

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = MiraAPI.Utilities.Assets.TmpSpriteUtils.CreateSpriteAsset(DivaniAssets.FragIcon.LoadAsset(), "DivaniMod.Role.Neutral.Frag", 1.45f),
        OptionsScreenshot = DivaniAssets.FragBanner,
        Icon = DivaniAssets.FragIcon,
        IntroSound = DivaniAssets.FragIntroSound,
        MaxRoleCount = 1,
        CanUseVent = OptionGroupSingleton<FragOptions>.Instance.CanVent.Value,
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>(),
    };

    public override void SpawnTaskHeader(PlayerControl playerControl)
    {
        if (playerControl != PlayerControl.LocalPlayer)
        {
            return;
        }
        ImportantTextTask orCreateTask = PlayerTask.GetOrCreateTask<ImportantTextTask>(playerControl, 0);
        orCreateTask.Text =
            $"{TownOfUsColors.Neutral.ToTextColor()}{MiraLocaleManager.Get("NeutralKillingTaskHeader")}</color>";
        orCreateTask.name = "NeutralRoleText";
    }

    public void OffsetButtons()
    {
        var beforeVent = !OptionGroupSingleton<FragOptions>.Instance.CanVent.Value;
        Coroutines.Start(MiscUtils.CoMoveButtonIndex(CustomButtonSingleton<FragGiveBombButton>.Instance, beforeVent));
        Coroutines.Start(MiscUtils.CoMoveButtonIndex(CustomButtonSingleton<FragBombButton>.Instance, beforeVent));
    }

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        if (Player.AmOwner)
        {
            OffsetButtons();
            HudManager.Instance.ImpostorVentButton.graphic.sprite = DivaniAssets.FragVentButton.LoadAsset();
            HudManager.Instance.ImpostorVentButton.buttonLabelText.SetOutlineColor(FragColor);
            CustomButtonSingleton<FakeVentButton>.Instance.Show = false;
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
            CustomButtonSingleton<FakeVentButton>.Instance.Show = true;
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

    public bool WinConditionMet()
    {
        var fragCount = CustomRoleUtils.GetActiveRolesOfType<FragRole>().Count(x => !x.Player.HasDied());

        if (MiscUtils.KillersAliveCount > fragCount)
        {
            return false;
        }

        return fragCount >= Helpers.GetAlivePlayers().Count - fragCount;
    }

    public override bool DidWin(GameOverReason gameOverReason)
    {
        return WinConditionMet();
    }
}
