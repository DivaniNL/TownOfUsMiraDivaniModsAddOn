using AmongUs.GameOptions;
using DivaniMods.Assets;
using MiraAPI.Modifiers;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using TownOfUs.Roles;
using UnityEngine;

namespace DivaniMods.Modifiers.Crewmate.CrewmatePower;

public sealed class MentorTaughtModifier(ushort originalRoleId, ushort lessonRoleId) : BaseModifier
{
    public static readonly Color MentorColor = new Color32(150, 150, 200, 255);
    public override string ModifierName => MiraLocaleManager.Get("DivaniMods.Modifier.MentorTaught", "Taught");
    public override bool HideOnUi => true;
    public Color ModifierColor => MentorColor;
    public override LoadableAsset<Sprite>? ModifierIcon => DivaniAssets.MentorTaughtIcon;
    public ushort OriginalRoleId { get; set; } = originalRoleId;
    public ushort LessonRoleId { get; set; } = lessonRoleId;

    public override string GetDescription()
    {
        var roleObj = RoleManager.Instance.GetRole((RoleTypes)LessonRoleId) as ITownOfUsRole;
        var roleName = roleObj?.RoleName ?? MiraLocaleManager.Get("DivaniMods.Role.Mentor.Fallback.NewRole", "a new role");
        var roleColor = roleObj != null ? ColorUtility.ToHtmlStringRGB(roleObj.RoleColor) : "9999FF";
        return MiraLocaleManager.Get("DivaniMods.Modifier.MentorTaught.Description", "You have been taught by the Mentor- you are the [role] for one round!")
            .Replace("[role]", $"<color=#{roleColor}>{roleName}</color>");
    }

    public override void OnActivate()
    {
        base.OnActivate();

        if (Player == null || !Player.AmOwner)
        {
            return;
        }

        var roleObj = RoleManager.Instance.GetRole((RoleTypes)LessonRoleId) as ITownOfUsRole;
        var lessonRoleName = roleObj?.RoleName ?? MiraLocaleManager.Get("DivaniMods.Role.Mentor.Fallback.NewRole", "a new role");
        var lessonRoleHex = roleObj != null ? ColorUtility.ToHtmlStringRGB(roleObj.RoleColor) : "9999FF";
        var mentorName = $"<color=#{ColorUtility.ToHtmlStringRGB(MentorColor)}>{MiraLocaleManager.Get("DivaniMods.Role.Mentor", "Mentor")}</color>";

        var message = MiraLocaleManager.Get("DivaniMods.Modifier.MentorTaught.Notification", "The [mentor] has taught you a new role! You are now the [role].")
            .Replace("[mentor]", mentorName)
            .Replace("[role]", $"<color=#{lessonRoleHex}>{lessonRoleName}</color>");

        Helpers.CreateAndShowNotification(
            $"<b>{message}</b>",
            Color.white, spr: DivaniAssets.MentorIcon.LoadAsset());
    }

    public override void OnDeath(DeathReason reason)
    {
        ModifierComponent?.RemoveModifier(this);
    }
}
