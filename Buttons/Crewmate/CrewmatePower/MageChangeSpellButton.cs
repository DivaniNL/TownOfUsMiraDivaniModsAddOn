using MiraAPI.Hud;
using MiraAPI.Utilities.Assets;
using DivaniMods.Assets;
using DivaniMods.Roles.Crewmate.CrewmatePower;
using TownOfUs.Buttons;
using UnityEngine;

namespace DivaniMods.Buttons.Crewmate.CrewmatePower;

public sealed class MageChangeSpellButton : TownOfUsRoleButton<MageRole>
{
    public override string Name => "Change Spell";
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => MageRole.MageColor;
    public override float Cooldown => 0.0001f;
    public override LoadableAsset<Sprite> Sprite => DivaniAssets.MageChangeSpellButton;

    public static MageSpellButton SpellButton => CustomButtonSingleton<MageSpellButton>.Instance;

    public override bool CanUse()
    {
        return base.CanUse() && !SpellButton.EffectActive;
    }

    protected override void OnClick()
    {
        SpellButton.CycleSpell();
    }
}
