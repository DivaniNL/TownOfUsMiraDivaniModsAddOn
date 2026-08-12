using DivaniMods.Assets;
using DivaniMods.Modifiers.Game.Impostor.ImpostorPassive;
using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using TownOfUs.Options;
using UnityEngine;

namespace DivaniMods.Options;

public sealed class ImpostorModifierOptions : AbstractOptionGroup
{
    public override string GroupName => "Impostor Modifiers";
    public override Func<bool> GroupVisible => () => OptionGroupSingleton<RoleOptions>.Instance.IsClassicRoleAssignment;
    public override Color GroupColor => Palette.ImpostorRoleHeaderRed;
    public override MenuCategory ParentMenu => MenuCategory.Modifiers;
    public override uint GroupPriority => 3;

    public AmountChanceOption NullifiedAmount { get; } = new(
        "Nullified Amount", 0f, 0f, 5f, 1f,
        color: NullifiedModifier.NullifiedColor, asset: DivaniAssets.NullifiedIcon,
        assetName: "DivaniMod.Modifier.Impostor.Nullified", assetScale: 1.45f)
    {
        ChangedEvent = _nullifiedNotif,
    };

    public AmountChanceOption NullifiedChance { get; } =
        new("Nullified Chance", 50f, 0, 100f, 10f, "#", "#", MiraNumberSuffixes.Percent,
            color: NullifiedModifier.NullifiedColor, asset: DivaniAssets.NullifiedIcon,
            assetName: "DivaniMod.Modifier.Impostor.Nullified", assetScale: 1.45f)
        {
            ChangedEvent = _nullifiedNotif,
            Visible = () => OptionGroupSingleton<ImpostorModifierOptions>.Instance.NullifiedAmount.Value > 0
        };

    public AmountChanceOption RuthlessAmount { get; } = new(
        "Ruthless Amount", 0f, 0f, 5f, 1f,
        color: NullifiedModifier.NullifiedColor, asset: DivaniAssets.RuthlessIcon,
        assetName: "DivaniMod.Modifier.Impostor.Ruthless", assetScale: 1.45f)
    {
        ChangedEvent = _ruthlessNotif,
    };

    public AmountChanceOption RuthlessChance { get; } =
        new("Ruthless Chance", 50f, 0, 100f, 10f, "#", "#", MiraNumberSuffixes.Percent,
            color: NullifiedModifier.NullifiedColor, asset: DivaniAssets.RuthlessIcon,
            assetName: "DivaniMod.Modifier.Impostor.Ruthless", assetScale: 1.45f)
        {
            ChangedEvent = _ruthlessNotif,
            Visible = () => OptionGroupSingleton<ImpostorModifierOptions>.Instance.RuthlessAmount.Value > 0
        };
    private static Action<float> _nullifiedNotif = x =>
    {
        var optAmount = OptionGroupSingleton<ImpostorModifierOptions>.Instance.NullifiedAmount;
        var opt = OptionGroupSingleton<ImpostorModifierOptions>.Instance.NullifiedChance;
        RunNotif(opt, optAmount, "Nullified");
    };
    private static Action<float> _ruthlessNotif = x =>
    {
        var optAmount = OptionGroupSingleton<ImpostorModifierOptions>.Instance.RuthlessAmount;
        var opt = OptionGroupSingleton<ImpostorModifierOptions>.Instance.RuthlessChance;
        RunNotif(opt, optAmount, "Ruthless");
    };
    private static void RunNotif(AmountChanceOption opt, AmountChanceOption optAmount, string title)
    {
        opt.AddSettingsChangeMessage(HudManager.Instance.Notifier,
            opt.StringName,
            title,
            optAmount.Data.GetValueString(optAmount.Value),
            opt.Data.GetValueString(opt.Value));
    }
}
