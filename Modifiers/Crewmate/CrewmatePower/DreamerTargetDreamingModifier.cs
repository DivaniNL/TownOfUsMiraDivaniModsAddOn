using AmongUs.GameOptions;
using DivaniMods.Assets;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using TownOfUs.Roles;
using UnityEngine;

namespace DivaniMods.Modifiers.Crewmate.CrewmatePower;

public sealed class DreamerTargetDreamingModifier(ushort originalRole, ushort dreamRole) : BaseModifier
{
    public override string ModifierName => "Dreaming";
    public override bool HideOnUi => true;

    public ushort OriginalRole { get; set; } = originalRole;
    public ushort DreamRole { get; set; } = dreamRole;

    public override void OnActivate()
    {
        base.OnActivate();

        if (Player == null || !Player.AmOwner)
        {
            return;
        }

        var dreamRoleName = (RoleManager.Instance.GetRole((RoleTypes)DreamRole) as ITownOfUsRole)?.RoleName ?? "a new role";

        Helpers.CreateAndShowNotification(
            $"<b>The Dreamer has <color=#804D19>reimagined</color> your role! You are now the {dreamRoleName}.</b>",
            Color.white, spr: DivaniAssets.DreamerIcon.LoadAsset());
    }

    public override void OnDeath(DeathReason reason)
    {
        ModifierComponent?.RemoveModifier(this);
    }
}
