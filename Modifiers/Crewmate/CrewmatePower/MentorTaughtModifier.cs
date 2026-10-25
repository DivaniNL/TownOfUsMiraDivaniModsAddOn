using AmongUs.GameOptions;
using DivaniMods.Assets;
using MiraAPI.Modifiers;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using TownOfUs.Extensions;
using TownOfUs.Roles;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;

namespace DivaniMods.Modifiers.Crewmate.CrewmatePower;

public sealed class MentorTaughtModifier(ushort originalRoleId, ushort lessonRoleId, bool lessonSucceeded = true) : BaseModifier, ICachedRole
{
    public static readonly Color MentorColor = new Color32(150, 150, 200, 255);
    public override string ModifierName => MiraLocaleManager.Get("DivaniMods.Modifier.MentorTaught", "Taught");
    public override bool HideOnUi => true;
    public Color ModifierColor => MentorColor;
    public override LoadableAsset<Sprite>? ModifierIcon => DivaniAssets.MentorTaughtIcon;
    public ushort OriginalRoleId { get; set; } = originalRoleId;
    public ushort LessonRoleId { get; set; } = lessonRoleId;
    public bool LessonSucceeded { get; set; } = lessonSucceeded;

    public bool ShowCurrentRoleFirst => true;
    public bool Visible => LessonSucceeded &&
                           (Player.AmOwner || PlayerControl.LocalPlayer.HasDied() ||
                            FairyRole.FairySeesRoleVisibilityFlag(Player));
    public CacheRoleGuess GuessMode => CacheRoleGuess.ActiveRole; // placeholder ig? If you wanted to implement an option if a retrained player can be guessed as their prev role ig you could mess with this
    public RoleBehaviour CachedRole => RoleManager.Instance.GetRole((RoleTypes)OriginalRoleId);
    public string CachedRoleName => $"{MentorColor.ToTextColor()}{MiraLocaleManager.Get("DivaniMods.Modifier.MentorTaught", "Taught")}</color>";

    public override void OnActivate()
    {
        base.OnActivate();

        if (Player == null || !Player.AmOwner || !LessonSucceeded)
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

        if (Player == null || !Player.AmOwner || !LessonSucceeded)
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
