using AmongUs.GameOptions;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Translation;
using MiraAPI.Roles;
using DivaniMods.Options;
using DivaniMods.Roles.Impostor.ImpostorPower;
using TownOfUs.Extensions;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;

namespace DivaniMods.Modifiers.Impostor;

public sealed class RecruiterCacheModifier : BaseModifier, ICachedRole
{
    public override string ModifierName => MiraLocaleManager.Get("DivaniMods.Modifier.RecruiterCache", "Recruiter"); //Freeplay
    public override bool HideOnUi => true;
    public bool ShowCurrentRoleFirst => true;

    public bool Visible => Player.AmOwner || PlayerControl.LocalPlayer.HasDied() ||
                           PlayerControl.LocalPlayer.IsImpostorAligned() ||
                           FairyRole.FairySeesRoleVisibilityFlag(Player);

    public CacheRoleGuess GuessMode =>
        (CacheRoleGuess)OptionGroupSingleton<RecruiterOptions>.Instance.RecruitGuess.Value;

    public RoleBehaviour CachedRole => RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<RecruiterRole>());
}
