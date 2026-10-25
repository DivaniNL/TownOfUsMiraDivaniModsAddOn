using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Translation;
using DivaniMods.Buttons.Modifiers;
using DivaniMods.Options;
using UnityEngine;

namespace DivaniMods.Modifiers.Game.Universal;

public sealed class UAVActiveModifier : BaseModifier
{
    public override string ModifierName => MiraLocaleManager.Get("DivaniMods.Modifier.UAVActive", "UAV Active"); //For Freeplay
    public override bool HideOnUi => true;

    private float _activeUntil;

    public override void OnActivate()
    {
        _activeUntil = Time.time + OptionGroupSingleton<UAVOptions>.Instance.UavDuration.Value;
    }

    public override void FixedUpdate()
    {
        if (Player == null || !Player.AmOwner || Time.time < _activeUntil)
        {
            return;
        }

        Player.RpcRemoveModifier<UAVActiveModifier>();
        UAVButton.RpcUavEnd(Player);
    }
}
