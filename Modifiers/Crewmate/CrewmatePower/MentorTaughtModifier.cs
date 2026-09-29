using AmongUs.GameOptions;
using DivaniMods.Assets;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using TownOfUs.Roles;
using UnityEngine;

namespace DivaniMods.Modifiers.Crewmate.CrewmatePower;

public sealed class MentorTaughtModifier(ushort originalRoleId, ushort lessonRoleId) : BaseModifier
{
    public static readonly Color MentorColor = new Color32(150, 150, 200, 255);
    public override string ModifierName => "Taught";
    public override bool HideOnUi => true;
    public Color ModifierColor => MentorColor;
    public override LoadableAsset<Sprite>? ModifierIcon => DivaniAssets.MentorTaughtIcon;
    public ushort OriginalRoleId { get; set; } = originalRoleId;
    public ushort LessonRoleId { get; set; } = lessonRoleId;

    public override string GetDescription()
    {
        var roleObj = RoleManager.Instance.GetRole((RoleTypes)LessonRoleId) as ITownOfUsRole;
        var roleName = roleObj?.RoleName;
        var roleColor = roleObj != null ? ColorUtility.ToHtmlStringRGB(roleObj.RoleColor) : "9999FF";
        return $"You have been taught by the Mentor- you are the <color=#{roleColor}>{roleName}</color> for one round!";
    }

    public override void OnActivate()
    {
        base.OnActivate();

        if (Player == null || !Player.AmOwner)
        {
            return;
        }

        var lessonRoleName = (RoleManager.Instance.GetRole((RoleTypes)LessonRoleId) as ITownOfUsRole)?.RoleName ?? "a new role";

        Helpers.CreateAndShowNotification(
            $"<b>The Mentor has <color=#{ColorUtility.ToHtmlStringRGB(MentorColor)}>taught</color> you a new role! You are now the {lessonRoleName}.</b>",
            Color.white, spr: DivaniAssets.MentorIcon.LoadAsset());
    }

    public override void OnDeath(DeathReason reason)
    {
        ModifierComponent?.RemoveModifier(this);
    }
}
