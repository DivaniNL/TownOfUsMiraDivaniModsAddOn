using Il2CppInterop.Runtime.Attributes;
using Il2CppSystem.Text;
using Reactor.Utilities.Attributes;
using DivaniMods.Roles.Neutral.NeutralEvil;
using DivaniMods.Utilities;
using UnityEngine;

namespace DivaniMods.Patches;

[RegisterInIl2Cpp]
public sealed class DemolitionistSabotageTask(nint cppPtr) : SabotageTask(cppPtr)
{
    public override int TaskStep => _isComplete ? 1 : 0;
    public override bool IsComplete => _isComplete;
    private bool _isComplete;

    public override bool ValidConsole(Console console) => false;

    public override void Initialize()
    {
    }

    public override void AppendTaskText(Il2CppSystem.Text.StringBuilder sb)
    {
        if (!DemolitionistSabotageState.IsActive)
        {
            return;
        }

        var color = (DemolitionistSabotageState.FlashPulseIndex & 1) == 0
            ? DemolitionistRole.DemolitionistColor
            : DemolitionistSabotageState.SecondaryColor;
        var location = DemolitionistSabotageState.PlantedLocationName;
        var seconds = Mathf.CeilToInt(DemolitionistSabotageState.TimeRemaining);
        var hex = ColorUtility.ToHtmlStringRGB(color);

        sb.AppendLine(
            $"<color=#{hex}>Demolitionist Sabotage active\nLocation: {location}\n{seconds}s</color>");
    }

    public override void Complete()
    {
        if (_isComplete)
        {
            return;
        }

        _isComplete = true;

        if (Owner != null)
        {
            Owner.RemoveTask(this);
        }

        if (gameObject != null)
        {
            UnityEngine.Object.Destroy(gameObject);
        }
    }
}
