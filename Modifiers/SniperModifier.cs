using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using DivaniMods.Assets;
using DivaniMods.Options;
using TownOfUs.Interfaces;
using TownOfUs.Modifiers.Game;
using TownOfUs.Modules.Wiki;
using TownOfUs.Roles;
using TownOfUs.Utilities;
using UnityEngine;

namespace DivaniMods.Modifiers;

public sealed class SniperModifier : TouGameModifier, IColoredModifier, IWikiDiscoverable
{
    public static readonly Color SniperColor = new Color32(155, 165, 160, 255);
    public const float MaxSniperDistance = 2.5f;

    public override string ModifierName => "Sniper";
    public override string LocaleKey => "Sniper";
    public override ModifierFaction FactionType => ModifierFaction.NeutralPassive;
    public override Color FreeplayFileColor => SniperColor;
    public Color ModifierColor => SniperColor;
    public override LoadableAsset<Sprite>? ModifierIcon => DivaniAssets.SniperIcon;

    public override string GetDescription()
    {
        var multiplier = OptionGroupSingleton<SniperOptions>.Instance.KillDistanceMultiplier.Value;
        return $"Kill without teleporting. Kill range is multiplied by {multiplier:0.0}x, up to long kill distance.";
    }

    public string GetAdvancedDescription() => GetDescription() + MiscUtils.AppendOptionsText(GetType());

    public override int GetAssignmentChance() =>
        (int)OptionGroupSingleton<NeutralModifierOptions>.Instance.SniperChance.Value;

    public override int GetAmountPerGame() =>
        (int)OptionGroupSingleton<NeutralModifierOptions>.Instance.SniperAmount;

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return base.IsModifierValidOn(role) &&
            role is ITownOfUsRole { RoleAlignment: RoleAlignment.NeutralKilling };
    }

    public static bool LocalPlayerHasSniper()
    {
        var localPlayer = PlayerControl.LocalPlayer;
        return localPlayer?.Data?.Role is ITownOfUsRole { RoleAlignment: RoleAlignment.NeutralKilling } &&
            localPlayer.HasModifier<SniperModifier>();
    }

    public static float ApplyRangeMultiplier(float baseDistance)
    {
        if (baseDistance <= 0f)
        {
            return baseDistance;
        }

        var multiplier = OptionGroupSingleton<SniperOptions>.Instance.KillDistanceMultiplier.Value;
        return Mathf.Min(baseDistance * multiplier, MaxSniperDistance);
    }
}
