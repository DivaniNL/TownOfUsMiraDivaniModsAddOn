using MiraAPI.Modifiers;
using MiraAPI.Translation;
using MiraAPI.Utilities.Assets;
using DivaniMods.Assets;
using UnityEngine;

namespace DivaniMods.Modifiers.Crewmate.CrewmatePower;

public sealed class TasklisttwoModifier : BaseModifier
{
    public override string ModifierName => MiraLocaleManager.Get("DivaniMods.Modifier.TaskListTwo", "Task List Two"); // Not on UI but maybe in Freeplay
    public override LoadableAsset<Sprite>? ModifierIcon => DivaniAssets.WorkhorseIcon;
    public override bool HideOnUi => true;
}
