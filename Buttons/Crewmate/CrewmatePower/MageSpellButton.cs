using System.Collections.Generic;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using DivaniMods.Assets;
using DivaniMods.Modifiers.Crewmate.CrewmatePower;
using DivaniMods.Options;
using DivaniMods.Roles.Crewmate.CrewmatePower;
using TownOfUs;
using TownOfUs.Buttons;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Utilities;
using UnityEngine;

namespace DivaniMods.Buttons.Crewmate.CrewmatePower;

public sealed class MageSpellButton : TownOfUsRoleButton<MageRole, PlayerControl>
{
    public override string Name => "Shock Shield";
    public string CurrentName = "Shock Shield";
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => MageRole.MageColor;
    public override float Cooldown =>
        System.Math.Clamp(OptionGroupSingleton<MageOptions>.Instance.SpellCooldown.Value + MapCooldown, 5f, 120f);
    public override float EffectDuration => CurrentSpell switch
    {
        MageSpell.ShockShield => OptionGroupSingleton<MageOptions>.Instance.ShockShieldDuration.Value,
        MageSpell.Illusion => OptionGroupSingleton<MageOptions>.Instance.IllusionDuration.Value,
        _ => 0f,
    };
    private string ActiveLabel => CurrentSpell switch
    {
        MageSpell.ShockShield => "Shielding",
        MageSpell.Illusion => "Cloaking",
        _ => CurrentName,
    };
    public override float Distance => 1.5f;
    public override LoadableAsset<Sprite> Sprite => SpellSprites[(int)CurrentSpell];

    public MageSpell CurrentSpell = MageSpell.ShockShield;

    // -2 means the value isn't set, -1 means it is infinite.
    public int ShockShieldUsesLeft { get; set; } = -2;
    public int EnergizeUsesLeft { get; set; } = -2;
    public int IllusionUsesLeft { get; set; } = -2;

    public static List<LoadableAsset<Sprite>> SpellSprites { get; } = new()
    {
        DivaniAssets.MageShockShieldButton,
        DivaniAssets.MageEnergizeButton,
        DivaniAssets.MageIllusionButton,
    };

    public static List<string> SpellNames { get; } = new()
    {
        "Shock Shield",
        "Energize",
        "Illusion",
    };

    public int CurrentSpellUses()
    {
        return CurrentSpell switch
        {
            MageSpell.ShockShield => ShockShieldUsesLeft,
            MageSpell.Energize => EnergizeUsesLeft,
            MageSpell.Illusion => IllusionUsesLeft,
            _ => -1,
        };
    }

    public bool CurrentSpellLimited => CurrentSpellUses() != -1 && CurrentSpellUses() != -2;
    public string CurrentSpellText => CurrentSpellUses().ToString(TownOfUsPlugin.Culture);
    public bool CanUseSpell => !CurrentSpellLimited || CurrentSpellUses() > 0;

    public override void CreateButton(Transform parent)
    {
        base.CreateButton(parent);
        CurrentSpell = MageSpell.ShockShield;
        OverrideSprite(SpellSprites[(int)CurrentSpell].LoadAsset());
        OverrideName(SpellNames[(int)CurrentSpell]);
    }

    public override bool CanUse()
    {
        return base.CanUse() && CanUseSpell && !EffectActive;
    }

    public override void ClickHandler()
    {
        if (!CanClick() || PlayerControl.LocalPlayer.HasModifier<GlitchHackedModifier>() ||
            PlayerControl.LocalPlayer.HasModifier<DisabledModifier>())
        {
            return;
        }

        if (CurrentSpellLimited)
        {
            switch (CurrentSpell)
            {
                case MageSpell.ShockShield:
                    ShockShieldUsesLeft--;
                    break;
                case MageSpell.Energize:
                    EnergizeUsesLeft--;
                    break;
                case MageSpell.Illusion:
                    IllusionUsesLeft--;
                    break;
            }
        }

        Button!.OverrideText(CurrentSpellLimited ? (CurrentName + " - " + CurrentSpellText) : CurrentName);

        OnClick();

        if (HasEffect)
        {
            EffectActive = true;
            Timer = EffectDuration;
            Button!.OverrideText(ActiveLabel);
        }
        else
        {
            Timer = Cooldown;
        }
    }

    public override void OnEffectEnd()
    {
        Button?.OverrideText(CurrentSpellLimited ? (CurrentName + " - " + CurrentSpellText) : CurrentName);
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            return;
        }

        switch (CurrentSpell)
        {
            case MageSpell.ShockShield:
                Target.RpcAddModifier<ShockShieldModifier>(PlayerControl.LocalPlayer);
                break;
            case MageSpell.Energize:
                MageRole.RpcEnergize(PlayerControl.LocalPlayer, Target);
                break;
            case MageSpell.Illusion:
                Target.RpcAddModifier<IllusionModifier>(PlayerControl.LocalPlayer);
                MiraAPI.Utilities.Helpers.CreateAndShowNotification(
                    $"<b><color=#1586a2>You cast Illusion on {Target.Data.PlayerName}!</color></b>",
                    Color.white,
                    new Vector3(0f, 1f, -20f),
                    spr: DivaniAssets.MageIcon.LoadAsset());
                break;
        }
    }

    public void CycleSpell()
    {
        var next = (MageSpell)((int)CurrentSpell + 1);
        CurrentSpell = System.Enum.IsDefined(next) ? next : MageSpell.ShockShield;
        OverrideSprite(SpellSprites[(int)CurrentSpell].LoadAsset());
        OverrideName(SpellNames[(int)CurrentSpell]);
    }

    public override void OverrideName(string name)
    {
        CurrentName = name;
        Button?.OverrideText(CurrentSpellLimited ? (CurrentName + " - " + CurrentSpellText) : CurrentName);
    }

    public override PlayerControl? GetTarget()
    {
        return CurrentSpell switch
        {
            MageSpell.ShockShield => PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance, false,
                x => !x.HasModifier<ShockShieldModifier>()),
            MageSpell.Illusion => PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance, false,
                x => !x.HasModifier<IllusionModifier>()),
            _ => PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance, false),
        };
    }
}
