using DivaniMods.Assets;
using DivaniMods.Modifiers.Game.Neutral.NeutralPassive;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs;
using TownOfUs.Options;
using UnityEngine;

namespace DivaniMods.Options;

public sealed class NeutralModifierOptions : AbstractOptionGroup
{
    public override string GroupName => "Neutral Modifiers";
    public override Func<bool> GroupVisible => () => RoleOptions.IsClassicRoleAssignment;
    public override Color GroupColor => TownOfUsColors.Neutral;
    public override MenuCategory ParentMenu => MenuCategory.Modifiers;
    public override uint GroupPriority => 4;

    public AmountChanceOption SniperAmount { get; } = new(
        "Sniper Amount", 0f, 0f, 5f, 1f,
        color: SniperModifier.SniperColor, asset: DivaniAssets.SniperIcon,
        assetName: "DivaniMod.Modifier.Neutral.Sniper", assetScale: 1.45f)
    {
        ChangedEvent = _sniperNotif,
    };

    public AmountChanceOption SniperChance { get; } =
        new("Sniper Chance", 50f, 0, 100f, 10f, "#", "#", MiraNumberSuffixes.Percent,
            color: SniperModifier.SniperColor, asset: DivaniAssets.SniperIcon,
            assetName: "DivaniMod.Modifier.Neutral.Sniper", assetScale: 1.45f)
        {
            ChangedEvent = _sniperNotif,
            Visible = () => OptionGroupSingleton<NeutralModifierOptions>.Instance.SniperAmount.Value > 0
        };
    private static Action<float> _sniperNotif = x =>
    {
        var optAmount = OptionGroupSingleton<NeutralModifierOptions>.Instance.SniperAmount;
        var opt = OptionGroupSingleton<NeutralModifierOptions>.Instance.SniperChance;
        RunNotif(opt, optAmount, "Sniper");
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
