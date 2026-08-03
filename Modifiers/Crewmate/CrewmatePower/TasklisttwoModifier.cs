using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using DivaniMods.Assets;
using UnityEngine;

namespace DivaniMods.Modifiers.Crewmate.CrewmatePower;

public sealed class TasklisttwoModifier : BaseModifier
{
    public override string ModifierName => "Task List Two";
    public override LoadableAsset<Sprite>? ModifierIcon => DivaniAssets.WorkhorseIcon;
    public override bool HideOnUi => true;
}
