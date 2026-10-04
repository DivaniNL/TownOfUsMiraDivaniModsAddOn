using AmongUs.GameOptions;
using DivaniMods.Assets;
using MiraAPI.Modifiers;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using TownOfUs.Roles;
using UnityEngine;

namespace DivaniMods.Modifiers.Crewmate.CrewmatePower;

public sealed class MentorTaughtModifier(ushort originalRoleId, ushort lessonRoleId) : BaseModifier, ICachedRole
{
    public static readonly Color MentorColor = new Color32(150, 150, 200, 255);
    public override string ModifierName => MiraLocaleManager.Get("DivaniMods.Modifier.MentorTaught", "Taught");
    public override bool HideOnUi => true;
    public Color ModifierColor => MentorColor;
    public override LoadableAsset<Sprite>? ModifierIcon => DivaniAssets.MentorTaughtIcon;
    public ushort OriginalRoleId { get; set; } = originalRoleId;
    public ushort LessonRoleId { get; set; } = lessonRoleId;

    public bool ShowCurrentRoleFirst => true;
    public bool Visible => Player.AmOwner || PlayerControl.LocalPlayer.HasDied() || FairyRole.FairySeesRoleVisibilityFlag(Player);
    public CacheRoleGuess GuessMode => CacheRoleGuess.ActiveRole; // placeholder ig? If you wanted to implement an option if a retrained player can be guessed as their prev role ig you could mess with this
    public RoleBehaviour CachedRole => RoleManager.Instance.GetRole((RoleTypes)OriginalRoleId);
    public string CachedRoleName => $"{MentorColor.ToTextColor()}{MiraLocaleManager.Get("DivaniMods.Modifier.MentorTaught", "Taught")}</color>";

    public override void OnActivate()
    {
        base.OnActivate();

        if (Player == null || !Player.AmOwner)
        {
            return;
        }

        var lessonRoleName = (RoleManager.Instance.GetRole((RoleTypes)LessonRoleId) as ITownOfUsRole)?.RoleName;

        Helpers.CreateAndShowNotification(
            $"<b>{MiraLocaleManager.Get($"DivaniMods.Modifier.MentorTaught.Notification").Replace("<role>", lessonRoleName)}</b>",
            Color.white, spr: DivaniAssets.MentorIcon.LoadAsset());
        
        Coroutines.Start(MiscUtils.CoFlash(MentorColor));
    }

    public override void OnDeactivate()
    {
        base.OnDeactivate();

        if (Player == null || !Player.AmOwner)
        {
            return;
        }

        Helpers.CreateAndShowNotification(
            $"<b>{MiraLocaleManager.Get($"DivaniMods.Modifier.MentorUntaught.Notification")}</b>",
            Color.white, spr: DivaniAssets.MentorIcon.LoadAsset());
        
        Coroutines.Start(MiscUtils.CoFlash(MentorColor));
    }

    public override void OnDeath(DeathReason reason)
    {
        ModifierComponent?.RemoveModifier(this);
    }
}
