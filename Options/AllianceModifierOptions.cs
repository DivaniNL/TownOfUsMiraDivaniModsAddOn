using DivaniMods.Assets;
using DivaniMods.Modifiers.Game.Alliance;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Options;
using UnityEngine;

namespace DivaniMods.Options;

public sealed class AllianceModifierOptions : AbstractOptionGroup
{
    public override string GroupName => "Alliance Modifiers";
    public override Func<bool> GroupVisible => () => OptionGroupSingleton<RoleOptions>.Instance.IsClassicRoleAssignment;
    public override Color GroupColor => Color.white;
    public override MenuCategory ParentMenu => MenuCategory.Modifiers;
    public override uint GroupPriority => 0;

    public AmountChanceOption BetrayerAmount { get; } = new(
        "Betrayer Amount", 1f, 0f, 3f, 1f,
        color: BetrayerModifier.BetrayerColor, asset: DivaniAssets.BetrayerIcon,
        assetName: "DivaniMod.Modifier.Alliance.Betrayer", assetScale: 1.45f)
    {
        ChangedEvent = _betrayerNotif,
    };

    public AmountChanceOption BetrayerChance { get; } =
        new("Betrayer Chance", 0f, 0f, 100f, 10f, "#", "#", MiraNumberSuffixes.Percent,
            color: BetrayerModifier.BetrayerColor, asset: DivaniAssets.BetrayerIcon,
            assetName: "DivaniMod.Modifier.Alliance.Betrayer", assetScale: 1.45f)
        {
            ChangedEvent = _betrayerNotif,
            Visible = () => OptionGroupSingleton<AllianceModifierOptions>.Instance.BetrayerAmount.Value > 0
        };
    
    public AmountChanceOption YinYangChance { get; } =
        new("Yin-Yang Chance", 0f, 0f, 100f, 10f, "#", "#", MiraNumberSuffixes.Percent,
            color: YinYangModifier.YinColor, asset: DivaniAssets.YinYangIcon,
            assetName: "DivaniMod.Modifier.Alliance.YinYang", assetScale: 1.45f)
        {
            ChangedEvent = x =>
            {
                var opt = OptionGroupSingleton<AllianceModifierOptions>.Instance.YinYangChance;
                RunNotif(opt, x > 0f ? "2" : "0", "Yin-Yang");
            },
        };

    private static Action<float> _betrayerNotif = x =>
    {
        var optAmount = OptionGroupSingleton<AllianceModifierOptions>.Instance.BetrayerAmount;
        var opt = OptionGroupSingleton<AllianceModifierOptions>.Instance.BetrayerChance;
        RunNotif(opt, optAmount, "Betrayer");
    };
    private static void RunNotif(AmountChanceOption opt, AmountChanceOption optAmount, string title)
    {
        opt.AddSettingsChangeMessage(HudManager.Instance.Notifier,
            opt.StringName,
            title,
            optAmount.Data.GetValueString(optAmount.Value),
            opt.Data.GetValueString(opt.Value));
    }

    private static void RunNotif(AmountChanceOption opt, string count, string title)
    {
        opt.AddSettingsChangeMessage(HudManager.Instance.Notifier,
            opt.StringName,
            title,
            count,
            opt.Data.GetValueString(opt.Value));
    }
}
