using DivaniMods.Assets;
using DivaniMods.Modifiers.Game.Universal;
using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using TownOfUs.Options;

namespace DivaniMods.Options;

public sealed class UniversalModifierOptions : AbstractOptionGroup
{
    public override string GroupName => "Universal Modifiers";
    public override Func<bool> GroupVisible => () => OptionGroupSingleton<RoleOptions>.Instance.IsClassicRoleAssignment;
    public override MenuCategory ParentMenu => MenuCategory.Modifiers;
    public override uint GroupPriority => 1;

    public AmountChanceOption ArmoredAmount { get; } = new(
        "Armored Amount", 0f, 0f, 5f, 1f,
        color: ArmoredModifier.ArmoredColor, asset: DivaniAssets.ArmoredIcon,
        assetName: "DivaniMod.Modifier.Universal.Armored", assetScale: 1.45f)
    {
        ChangedEvent = _armoredNotif,
    };

    public AmountChanceOption ArmoredChance { get; } =
        new("Armored Chance", 50f, 0, 100f, 10f, "#", "#", MiraNumberSuffixes.Percent,
            color: ArmoredModifier.ArmoredColor, asset: DivaniAssets.ArmoredIcon,
            assetName: "DivaniMod.Modifier.Universal.Armored", assetScale: 1.45f)
        {
            ChangedEvent = _armoredNotif,
            Visible = () => OptionGroupSingleton<UniversalModifierOptions>.Instance.ArmoredAmount.Value > 0
        };

    public AmountChanceOption FragileAmount { get; } = new(
        "Fragile Amount", 0f, 0f, 5f, 1f,
        color: FragileModifier.FragileColor, asset: DivaniAssets.FragileIcon,
        assetName: "DivaniMod.Modifier.Universal.Fragile", assetScale: 1.45f)
    {
        ChangedEvent = _fragileNotif,
    };

    public AmountChanceOption FragileChance { get; } =
        new("Fragile Chance", 50f, 0, 100f, 10f, "#", "#", MiraNumberSuffixes.Percent,
            color: FragileModifier.FragileColor, asset: DivaniAssets.FragileIcon,
            assetName: "DivaniMod.Modifier.Universal.Fragile", assetScale: 1.45f)
        {
            ChangedEvent = _fragileNotif,
            Visible = () => OptionGroupSingleton<UniversalModifierOptions>.Instance.FragileAmount.Value > 0
        };

    public AmountChanceOption MementoAmount { get; } = new(
        "Memento Amount", 0f, 0f, 5f, 1f,
        color: MementoModifier.MementoColor, asset: DivaniAssets.MementoIcon,
        assetName: "DivaniMod.Modifier.Universal.Memento", assetScale: 1.45f)
    {
        ChangedEvent = _mementoNotif,
    };

    public AmountChanceOption MementoChance { get; } =
        new("Memento Chance", 50f, 0, 100f, 10f, "#", "#", MiraNumberSuffixes.Percent,
            color: MementoModifier.MementoColor, asset: DivaniAssets.MementoIcon,
            assetName: "DivaniMod.Modifier.Universal.Memento", assetScale: 1.45f)
        {
            ChangedEvent = _mementoNotif,
            Visible = () => OptionGroupSingleton<UniversalModifierOptions>.Instance.MementoAmount.Value > 0
        };

    public AmountChanceOption MisvoteAmount { get; } = new(
        "Misvote Amount", 1f, 0f, 5f, 1f,
        color: MisvoteModifier.MisvoteColor, asset: DivaniAssets.MisvoteIcon,
        assetName: "DivaniMod.Modifier.Universal.Misvote", assetScale: 1.45f)
    {
        ChangedEvent = _misvoteNotif,
    };

    public AmountChanceOption MisvoteChance { get; } =
        new("Misvote Chance", 50f, 0, 100f, 10f, "#", "#", MiraNumberSuffixes.Percent,
            color: MisvoteModifier.MisvoteColor, asset: DivaniAssets.MisvoteIcon,
            assetName: "DivaniMod.Modifier.Universal.Misvote", assetScale: 1.45f)
        {
            ChangedEvent = _misvoteNotif,
            Visible = () => OptionGroupSingleton<UniversalModifierOptions>.Instance.MisvoteAmount.Value > 0
        };

    public AmountChanceOption ShuffleAmount { get; } = new(
        "Shuffle Amount", 1f, 0f, 5f, 1f,
        color: ShuffleModifier.ShuffleColor, asset: DivaniAssets.ShuffleIcon,
        assetName: "DivaniMod.Modifier.Universal.Shuffle", assetScale: 1.45f)
    {
        ChangedEvent = _shuffleNotif,
    };

    public AmountChanceOption ShuffleChance { get; } =
        new("Shuffle Chance", 50f, 0, 100f, 10f, "#", "#", MiraNumberSuffixes.Percent,
            color: ShuffleModifier.ShuffleColor, asset: DivaniAssets.ShuffleIcon,
            assetName: "DivaniMod.Modifier.Universal.Shuffle", assetScale: 1.45f)
        {
            ChangedEvent = _shuffleNotif,
            Visible = () => OptionGroupSingleton<UniversalModifierOptions>.Instance.ShuffleAmount.Value > 0
        };

    public AmountChanceOption TacticalInsertionAmount { get; } = new(
        "Tactical Insertion Amount", 0f, 0f, 5f, 1f,
        color: TacticalInsertionModifier.TacticalColor, asset: DivaniAssets.TacticalInsertionIcon,
        assetName: "DivaniMod.Modifier.Universal.Tactical", assetScale: 1.45f)
    {
        ChangedEvent = _tactNotif,
    };

    public AmountChanceOption TacticalInsertionChance { get; } =
        new("Tactical Insertion Chance", 50f, 0, 100f, 10f, "#", "#", MiraNumberSuffixes.Percent,
            color: TacticalInsertionModifier.TacticalColor, asset: DivaniAssets.TacticalInsertionIcon,
            assetName: "DivaniMod.Modifier.Universal.Tactical", assetScale: 1.45f)
        {
            ChangedEvent = _tactNotif,
            Visible = () => OptionGroupSingleton<UniversalModifierOptions>.Instance.TacticalInsertionAmount.Value > 0
        };

    public AmountChanceOption UavAmount { get; } = new(
        "UAV Amount", 0f, 0f, 5f, 1f,
        color: UAVModifier.UavColor, asset: DivaniAssets.UavIcon,
        assetName: "DivaniMod.Modifier.Universal.UAV", assetScale: 1.45f)
    {
        ChangedEvent = _uavNotif,
    };

    public AmountChanceOption UavChance { get; } =
        new("UAV Chance", 50f, 0, 100f, 10f, "#", "#", MiraNumberSuffixes.Percent,
            color: UAVModifier.UavColor, asset: DivaniAssets.UavIcon,
            assetName: "DivaniMod.Modifier.Universal.UAV", assetScale: 1.45f)
        {
            ChangedEvent = _uavNotif,
            Visible = () => OptionGroupSingleton<UniversalModifierOptions>.Instance.UavAmount.Value > 0
        };
    
    private static Action<float> _armoredNotif = x =>
    {
        var optAmount = OptionGroupSingleton<UniversalModifierOptions>.Instance.ArmoredAmount;
        var opt = OptionGroupSingleton<UniversalModifierOptions>.Instance.ArmoredChance;
        RunNotif(opt, optAmount, "Armored");
    };
    private static Action<float> _fragileNotif = x =>
    {
        var optAmount = OptionGroupSingleton<UniversalModifierOptions>.Instance.FragileAmount;
        var opt = OptionGroupSingleton<UniversalModifierOptions>.Instance.FragileChance;
        RunNotif(opt, optAmount, "Fragile");
    };
    private static Action<float> _mementoNotif = x =>
    {
        var optAmount = OptionGroupSingleton<UniversalModifierOptions>.Instance.MementoAmount;
        var opt = OptionGroupSingleton<UniversalModifierOptions>.Instance.MementoChance;
        RunNotif(opt, optAmount, "Memento");
    };
    private static Action<float> _misvoteNotif = x =>
    {
        var optAmount = OptionGroupSingleton<UniversalModifierOptions>.Instance.MisvoteAmount;
        var opt = OptionGroupSingleton<UniversalModifierOptions>.Instance.MisvoteChance;
        RunNotif(opt, optAmount, "Misvote");
    };
    private static Action<float> _shuffleNotif = x =>
    {
        var optAmount = OptionGroupSingleton<UniversalModifierOptions>.Instance.ShuffleAmount;
        var opt = OptionGroupSingleton<UniversalModifierOptions>.Instance.ShuffleChance;
        RunNotif(opt, optAmount, "Shuffle");
    };
    private static Action<float> _tactNotif = x =>
    {
        var optAmount = OptionGroupSingleton<UniversalModifierOptions>.Instance.TacticalInsertionAmount;
        var opt = OptionGroupSingleton<UniversalModifierOptions>.Instance.TacticalInsertionChance;
        RunNotif(opt, optAmount, "Tactical Insertion");
    };
    private static Action<float> _uavNotif = x =>
    {
        var optAmount = OptionGroupSingleton<UniversalModifierOptions>.Instance.UavAmount;
        var opt = OptionGroupSingleton<UniversalModifierOptions>.Instance.UavChance;
        RunNotif(opt, optAmount, "UAV");
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
