using AmongUs.GameOptions;
using DivaniMods.Assets;
using DivaniMods.Modules.Monster;
using DivaniMods.Options;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities.Assets;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using Reactor.Utilities.Extensions;
using System.Text;
using TMPro;
using TownOfUs;
using TownOfUs.Assets;
using TownOfUs.Interfaces;
using TownOfUs.Modules.Localization;
using TownOfUs.Modules.RainbowMod;
using TownOfUs.Modules.Wiki;
using TownOfUs.Roles;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;

namespace DivaniMods.Roles.Neutral.NeutralKilling;

public sealed class MonsterRole(IntPtr cppPtr) : NeutralRole(cppPtr), ITownOfUsRole, IProgressTally, IWikiDiscoverable, ICrewVariant
{
    public static readonly Color MonsterColor = new Color32(107, 179, 48, 255);
    public string RoleName => "Monster";
    public string RoleDescription => "Devour Everyone";
    public string RoleLongDescription =>
        "Eat nearby players to trap them. If you make it to the next meeting,\neveryone you've " +
        "eaten is killed for real. If you die first, they're released instead.";

    public Color RoleColor => MonsterColor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleAlignment RoleAlignment => RoleAlignment.NeutralKilling;
    public bool HasImpostorVision => true;
    public RoleBehaviour CrewVariant =>
        RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<TrapperRole>());

    private static TMP_SpriteAsset[] UxIcons =>
    [
        TmpSpriteUtils.CreateSpriteAsset(TouAssets.ChefProgressFedUncolored.LoadAsset(),
            "DivaniMod.Role.Neutral.Monster.Ui.PlayerUncolored", 1.45f),
        TmpSpriteUtils.CreateSpriteAsset(TouAssets.ChefProgressFedRainbow.LoadAsset(),
            "DivaniMod.Role.Neutral.Monster.Ui.PlayerRainbow", 1.45f),
    ];
    [HideFromIl2Cpp] public List<CustomButtonWikiDescription> Abilities { get; } =
    [
        new("Devour", "When you Devour a player, they are trapped. If you die first, they are released instead.", DivaniAssets.MonsterDevourButton),
    ];
    public string GetAdvancedDescription() => RoleLongDescription + MiscUtils.AppendOptionsText(GetType());
    
    private static string GetIcon(TMP_SpriteAsset asset) => $"<sprite name=\"{asset.name}\">";

    private static string GetIconColored(TMP_SpriteAsset asset, string color) =>
        $"<sprite name=\"{asset.name}\" color=#{color}>";

    public string GetBodyTally()
    {
        var held = MonsterState.GetHeld(Player.PlayerId);
        if (held.Count == 0)
        {
            return string.Empty;
        }

        var tally = new StringBuilder();
        foreach (var victimId in held)
        {
            var victim = MiscUtils.PlayerById(victimId);
            if (victim?.Data == null)
            {
                continue;
            }

            if (RainbowUtils.IsRainbow(victim.Data.DefaultOutfit.ColorId))
            {
                tally.Append(GetIcon(UxIcons[1]));
            }
            else
            {
                tally.Append(GetIconColored(UxIcons[0],
                    Palette.TextColors[victim.Data.DefaultOutfit.ColorId].ToHtmlStringRGBA()));
            }
        }

        return $"({tally})";
    }

    public bool ProgressOnName(bool localDead, bool inMeeting, bool amOwner, out string progress)
    {
        if (!inMeeting && (amOwner || localDead))
        {
            var tally = GetBodyTally();
            if (!string.IsNullOrEmpty(tally))
            {
                progress = tally;
                return true;
            }
        }

        progress = string.Empty;
        return false;
    }

    public string ProgressOnSummaryNormal => string.Empty;

    public string ProgressOnSummaryDetailed => string.Empty;

    public TallyLocation TallyPlacement(bool inMeeting) => TallyLocation.AboveName;
    public override void SpawnTaskHeader(PlayerControl playerControl)
{
    if (playerControl != PlayerControl.LocalPlayer)
    {
        return;
    }
    ImportantTextTask orCreateTask = PlayerTask.GetOrCreateTask<ImportantTextTask>(playerControl, 0);
    orCreateTask.Text =
        $"{TownOfUsColors.Neutral.ToTextColor()}{TouLocale.GetParsed("NeutralKillingTaskHeader")}</color>";
    orCreateTask.name = "NeutralRoleText";
}
    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = MiraAPI.Utilities.Assets.TmpSpriteUtils.CreateSpriteAsset(DivaniAssets.MonsterIcon.LoadAsset(), "DivaniMod.Role.Neutral.Monster", 1.45f),
        Icon = DivaniAssets.MonsterIcon,
        IntroSound = DivaniAssets.MonsterIntroSound,
        MaxRoleCount = 1,
        CanUseVent = OptionGroupSingleton<MonsterOptions>.Instance.CanVent.Value,
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>(),
    };

    public override bool CanUse(IUsable usable)
    {
        if (!GameManager.Instance.LogicUsables.CanUse(usable, Player))
        {
            return false;
        }

        var console = usable.TryCast<Console>()!;
        return console == null || console.AllowImpostor;
    }

    [HideFromIl2Cpp]
    public bool WinConditionMet()
    {
        if (Player.HasDied()) return false;

        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player == null || player.Data == null || player.HasDied()) continue;
            if (player.PlayerId == Player.PlayerId) continue;
            if (MonsterState.IsEaten(player.PlayerId)) continue;
            return false;
        }

        return true;
    }
    public override void Initialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Initialize(this, targetPlayer);

        if (Player.AmOwner && OptionGroupSingleton<MonsterOptions>.Instance.CanVent.Value)
        {
            HudManager.Instance.ImpostorVentButton.graphic.sprite = DivaniAssets.MonsterVentButton.LoadAsset();
            HudManager.Instance.ImpostorVentButton.buttonLabelText.SetOutlineColor(MonsterColor);
        }
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        MonsterState.ReleaseAll(targetPlayer.PlayerId, targetPlayer.GetTruePosition());
        MonsterState.ForgetMonster(targetPlayer.PlayerId);
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);
        TouRoleUtils.ClearTaskHeader(Player);

        if (Player.AmOwner && OptionGroupSingleton<MonsterOptions>.Instance.CanVent.Value)
        {
            HudManager.Instance.ImpostorVentButton.graphic.sprite = TouAssets.VentSprite.LoadAsset();
            HudManager.Instance.ImpostorVentButton.buttonLabelText.SetOutlineColor(TownOfUsColors.Impostor);
        }
    }

    public override void OnDeath(DeathReason reason)
    {
        if (Player != null)
            MonsterState.ReleaseAll(Player.PlayerId, Player.GetTruePosition());

        RoleBehaviourStubs.OnDeath(this, reason);
    }

    public override bool DidWin(GameOverReason gameOverReason) => WinConditionMet();

    [MethodRpc((uint)DivaniRpcCalls.MonsterDevour, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcEat(PlayerControl Monster, byte victimId)
    {
        if (Monster != null) MonsterState.Eat(Monster.PlayerId, victimId);
    }

    [MethodRpc((uint)DivaniRpcCalls.MonsterDigest, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcDigest(PlayerControl Monster)
    {
        if (Monster != null) MonsterState.DigestAll(Monster.PlayerId);
    }
}