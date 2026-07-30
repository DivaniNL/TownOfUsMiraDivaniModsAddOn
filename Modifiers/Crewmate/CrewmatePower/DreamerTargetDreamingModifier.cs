using AmongUs.GameOptions;
using DivaniMods.Assets;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using TownOfUs.Roles;
using UnityEngine;

namespace DivaniMods.Modifiers.Crewmate.CrewmatePower;

public sealed class DreamerTargetDreamingModifier(ushort originalRoleId, ushort dreamRoleId) : BaseModifier
{
    public static readonly Color DreamerColor = new Color32(51, 51, 153, 255);
    public override string ModifierName => "Dreaming";
    public override bool HideOnUi => true;
    public Color ModifierColor => DreamerColor;
    public override LoadableAsset<Sprite>? ModifierIcon => DivaniAssets.DreamerIcon;
    public ushort OriginalRoleId { get; set; } = originalRoleId;
    public ushort DreamRoleId { get; set; } = dreamRoleId;

    public override string GetDescription()
    {
        var roleObj = RoleManager.Instance.GetRole((RoleTypes)DreamRoleId) as ITownOfUsRole;
        var roleName = roleObj?.RoleName;
        var roleColor = roleObj != null ? ColorUtility.ToHtmlStringRGB(roleObj.RoleColor) : "9999FF";
        return $"You are in a dream state- your true role is <color=#{roleColor}>{roleName}</color>!";
    }

    public override void OnActivate()
    {
        base.OnActivate();

        if (Player == null || !Player.AmOwner)
        {
            return;
        }

        var dreamRoleName = (RoleManager.Instance.GetRole((RoleTypes)DreamRoleId) as ITownOfUsRole)?.RoleName ?? "a new role";

        Helpers.CreateAndShowNotification(
            $"<b>The Dreamer has <color=#804D19>reimagined</color> your role! You are now the {dreamRoleName}.</b>",
            Color.white, spr: DivaniAssets.DreamerIcon.LoadAsset());
    }

    public override void OnDeath(DeathReason reason)
    {
        ModifierComponent?.RemoveModifier(this);
    }
}
