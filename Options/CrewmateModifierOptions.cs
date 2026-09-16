using DivaniMods.Assets;
using DivaniMods.Modifiers.Game.Crewmate;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Options;
using UnityEngine;

namespace DivaniMods.Options;

public sealed class CrewmateModifierOptions : AbstractOptionGroup
{
    public override string GroupName => "Crewmate Modifiers";
    public override Func<bool> GroupVisible => () => RoleOptions.IsClassicRoleAssignment;
    public override Color GroupColor => Palette.CrewmateRoleHeaderBlue;
    public override MenuCategory ParentMenu => MenuCategory.Modifiers;
    public override uint GroupPriority => 2;

    public AmountChanceOption BearTrapAmount { get; } = new(
        "Bear Trap Amount", 0f, 0f, 5f, 1f,
        color: BearTrapModifier.BearTrapColor, asset: DivaniAssets.BearTrapIcon,
        assetName: "DivaniMod.Modifier.Crewmate.BearTrap", assetScale: 1.45f)
    {
        ChangedEvent = _bearTrapNotif,
    };

    public AmountChanceOption BearTrapChance { get; } =
        new("Bear Trap Chance", 20f, 0, 100f, 10f, "#", "#", MiraNumberSuffixes.Percent,
            color: BearTrapModifier.BearTrapColor, asset: DivaniAssets.BearTrapIcon,
            assetName: "DivaniMod.Modifier.Crewmate.BearTrap", assetScale: 1.45f)
        {
            ChangedEvent = _bearTrapNotif,
            Visible = () => OptionGroupSingleton<CrewmateModifierOptions>.Instance.BearTrapAmount.Value > 0
        };

    public AmountChanceOption BlindspotAmount { get; } = new(
        "Blindspot Amount", 1f, 0f, 5f, 1f,
        color: BlindspotModifier.BlindspotColor, asset: DivaniAssets.BlindspotIcon,
        assetName: "DivaniMod.Modifier.Crewmate.Blindspot", assetScale: 1.45f)
    {
        ChangedEvent = _blindspotNotif,
    };

    public AmountChanceOption BlindspotChance { get; } =
        new("Blindspot Chance", 50f, 0, 100f, 10f, "#", "#", MiraNumberSuffixes.Percent,
            color: BlindspotModifier.BlindspotColor, asset: DivaniAssets.BlindspotIcon,
            assetName: "DivaniMod.Modifier.Crewmate.Blindspot", assetScale: 1.45f)
        {
            ChangedEvent = _blindspotNotif,
            Visible = () => OptionGroupSingleton<CrewmateModifierOptions>.Instance.BlindspotAmount.Value > 0
        };

    public AmountChanceOption BloodyAmount { get; } = new(
        "Bloody Amount", 0f, 0f, 5f, 1f,
        color: BloodyModifier.ModifierUiColor, asset: DivaniAssets.BloodyIcon,
        assetName: "DivaniMod.Modifier.Crewmate.Bloody", assetScale: 1.45f)
    {
        ChangedEvent = _bloodyNotif,
    };

    public AmountChanceOption BloodyChance { get; } =
        new("Bloody Chance", 50f, 0, 100f, 10f, "#", "#", MiraNumberSuffixes.Percent,
            color: BloodyModifier.ModifierUiColor, asset: DivaniAssets.BloodyIcon,
            assetName: "DivaniMod.Modifier.Crewmate.Bloody", assetScale: 1.45f)
        {
            ChangedEvent = _bloodyNotif,
            Visible = () => OptionGroupSingleton<CrewmateModifierOptions>.Instance.BloodyAmount.Value > 0
        };

    public AmountChanceOption IncompetentAmount { get; } = new(
        "Incompetent Amount", 0f, 0f, 5f, 1f,
        color: IncompetentModifier.IncompetentColor, asset: DivaniAssets.IncompetentIcon,
        assetName: "DivaniMod.Modifier.Crewmate.Incompetent", assetScale: 1.45f)
    {
        ChangedEvent = _incompetentNotif,
    };

    public AmountChanceOption IncompetentChance { get; } =
        new("Incompetent Chance", 20f, 0, 100f, 10f, "#", "#", MiraNumberSuffixes.Percent,
            color: IncompetentModifier.IncompetentColor, asset: DivaniAssets.IncompetentIcon,
            assetName: "DivaniMod.Modifier.Crewmate.Incompetent", assetScale: 1.45f)
        {
            ChangedEvent = _incompetentNotif,
            Visible = () => OptionGroupSingleton<CrewmateModifierOptions>.Instance.IncompetentAmount.Value > 0
        };

    public AmountChanceOption SkilledAmount { get; } = new(
        "Skilled Amount", 0f, 0f, 5f, 1f,
        color: SkilledModifier.SkilledColor, asset: DivaniAssets.SkilledIcon,
        assetName: "DivaniMod.Modifier.Crewmate.Skilled", assetScale: 1.45f)
    {
        ChangedEvent = _skilledNotif,
    };

    public AmountChanceOption SkilledChance { get; } =
        new("Skilled Chance", 10f, 0, 100f, 10f, "#", "#", MiraNumberSuffixes.Percent,
            color: SkilledModifier.SkilledColor, asset: DivaniAssets.SkilledIcon,
            assetName: "DivaniMod.Modifier.Crewmate.Skilled", assetScale: 1.45f)
        {
            ChangedEvent = _skilledNotif,
            Visible = () => OptionGroupSingleton<CrewmateModifierOptions>.Instance.SkilledAmount.Value > 0
        };

    public AmountChanceOption SproutAmount { get; } = new(
        "Sprout Amount", 0f, 0f, 5f, 1f,
        color: SproutModifier.SproutColor, asset: DivaniAssets.SproutIcon,
        assetName: "DivaniMod.Modifier.Crewmate.Sprout", assetScale: 1.45f)
    {
        ChangedEvent = _sproutNotif,
    };

    public AmountChanceOption SproutChance { get; } =
        new("Sprout Chance", 50f, 0, 100f, 10f, "#", "#", MiraNumberSuffixes.Percent,
            color: SproutModifier.SproutColor, asset: DivaniAssets.SproutIcon,
            assetName: "DivaniMod.Modifier.Crewmate.Sprout", assetScale: 1.45f)
        {
            ChangedEvent = _sproutNotif,
            Visible = () => OptionGroupSingleton<CrewmateModifierOptions>.Instance.SproutAmount.Value > 0
        };

    public AmountChanceOption StrongAmount { get; } = new(
        "Strong Amount", 0f, 0f, 5f, 1f,
        color: StrongModifier.StrongColor, asset: DivaniAssets.StrongIcon,
        assetName: "DivaniMod.Modifier.Crewmate.Strong", assetScale: 1.45f)
    {
        ChangedEvent = _strongNotif,
    };

    public AmountChanceOption StrongChance { get; } =
        new("Strong Chance", 20f, 0, 100f, 10f, "#", "#", MiraNumberSuffixes.Percent,
            color: StrongModifier.StrongColor, asset: DivaniAssets.StrongIcon,
            assetName: "DivaniMod.Modifier.Crewmate.Strong", assetScale: 1.45f)
        {
            ChangedEvent = _strongNotif,
            Visible = () => OptionGroupSingleton<CrewmateModifierOptions>.Instance.StrongAmount.Value > 0
        };
    
    private static Action<float> _bearTrapNotif = x =>
    {
        var optAmount = OptionGroupSingleton<CrewmateModifierOptions>.Instance.BearTrapAmount;
        var opt = OptionGroupSingleton<CrewmateModifierOptions>.Instance.BearTrapChance;
        RunNotif(opt, optAmount, "Bear Trap");
    };
    private static Action<float> _blindspotNotif = x =>
    {
        var optAmount = OptionGroupSingleton<CrewmateModifierOptions>.Instance.BlindspotAmount;
        var opt = OptionGroupSingleton<CrewmateModifierOptions>.Instance.BlindspotChance;
        RunNotif(opt, optAmount, "Blindspot");
    };
    private static Action<float> _bloodyNotif = x =>
    {
        var optAmount = OptionGroupSingleton<CrewmateModifierOptions>.Instance.BloodyAmount;
        var opt = OptionGroupSingleton<CrewmateModifierOptions>.Instance.BloodyChance;
        RunNotif(opt, optAmount, "Bloody");
    };
    private static Action<float> _incompetentNotif = x =>
    {
        var optAmount = OptionGroupSingleton<CrewmateModifierOptions>.Instance.IncompetentAmount;
        var opt = OptionGroupSingleton<CrewmateModifierOptions>.Instance.IncompetentChance;
        RunNotif(opt, optAmount, "Incompetent");
    };
    private static Action<float> _skilledNotif = x =>
    {
        var optAmount = OptionGroupSingleton<CrewmateModifierOptions>.Instance.SkilledAmount;
        var opt = OptionGroupSingleton<CrewmateModifierOptions>.Instance.SkilledChance;
        RunNotif(opt, optAmount, "Skilled");
    };
    private static Action<float> _sproutNotif = x =>
    {
        var optAmount = OptionGroupSingleton<CrewmateModifierOptions>.Instance.SproutAmount;
        var opt = OptionGroupSingleton<CrewmateModifierOptions>.Instance.SproutChance;
        RunNotif(opt, optAmount, "Sprout");
    };
    private static Action<float> _strongNotif = x =>
    {
        var optAmount = OptionGroupSingleton<CrewmateModifierOptions>.Instance.StrongAmount;
        var opt = OptionGroupSingleton<CrewmateModifierOptions>.Instance.StrongChance;
        RunNotif(opt, optAmount, "Strong");
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
