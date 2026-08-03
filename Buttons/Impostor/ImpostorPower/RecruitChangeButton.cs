using System.Linq;
using AmongUs.GameOptions;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Roles;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities.Extensions;
using DivaniMods.Options;
using DivaniMods.Roles.Impostor.ImpostorPower;
using TownOfUs.Assets;
using TownOfUs.Buttons;
using TownOfUs.Extensions;
using TownOfUs.Modules;
using TownOfUs.Modules.Components;
using TownOfUs.Roles;
using TownOfUs.Roles.Impostor;
using TownOfUs.Utilities;
using UnityEngine;

namespace DivaniMods.Buttons.Impostor.ImpostorPower;

public sealed class RecruitChangeButton : TownOfUsRoleButton<RecruitRole>
{
    public override string Name => "Change Role";
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => Palette.ImpostorRed;
    public override float Cooldown => 0.01f;
    public override ButtonLocation Location => ButtonLocation.BottomLeft;
    public override LoadableAsset<Sprite> Sprite => TouImpAssets.TraitorSelect;

    public override bool ZeroIsInfinite { get; set; } = true;

    public override void ClickHandler()
    {
        if (!CanClick() || Minigame.Instance || PlayerControl.LocalPlayer.HasDied())
        {
            return;
        }

        OnClick();
    }

    protected override void OnClick()
    {
        if (Role.ChosenRoles.Count == 0)
        {
            var excluded = MiscUtils.AllRegisteredRoles
                .Where(x => x is ISpawnChange { NoSpawn: true } || x.Role is RoleTypes.Impostor || x.IsDead ||
                            x is ITownOfUsRole { RoleAlignment: RoleAlignment.ImpostorPower })
                .Select(x => x.Role).ToList();
            var impRoles = MiscUtils.GetRolesToAssign(ModdedRoleTeams.Impostor, x => !excluded.Contains(x.Role))
                .Select(x => x.RoleType).ToList();

            var roleList = MiscUtils.GetPotentialRoles()
                .Where(role => role is not ITraitorIgnore ignore || !ignore.IsIgnored)
                .Where(role => impRoles.Contains((ushort)role.Role))
                .Where(role => role is not TraitorRole and not RecruitRole)
                .ToList();

            if (TutorialManager.InstanceExists)
            {
                impRoles = MiscUtils.GetRegisteredRoles(ModdedRoleTeams.Impostor)
                    .Where(x => !excluded.Contains(x.Role))
                    .Select(x => (ushort)x.Role).ToList();
                roleList = MiscUtils.AllRegisteredRoles
                    .Where(role => role is not ITraitorIgnore ignore || !ignore.IsIgnored)
                    .Where(role => impRoles.Contains((ushort)role.Role))
                    .Where(role => role is not TraitorRole and not RecruitRole)
                    .ToList();
            }

            if (OptionGroupSingleton<RecruiterOptions>.Instance.RecruitRemoveExistingRoles)
            {
                foreach (var player in PlayerControl.AllPlayerControls)
                {
                    if (player.IsImpostor() && !player.AmOwner)
                    {
                        var role = player.GetRoleWhenAlive();
                        if (role)
                        {
                            impRoles.Remove((ushort)role!.Role);
                        }
                    }
                }
            }

            roleList.Shuffle();
            roleList.Shuffle();
            var random = roleList.Random();
            roleList.Shuffle();

            for (var i = 0; i < 3; i++)
            {
                var selected = roleList.Random();
                if (selected == null)
                {
                    continue;
                }

                Role.ChosenRoles.Add(selected);
                roleList.Remove(selected);
            }

            Role.RandomRole = random;
        }

        if (!Minigame.Instance)
        {
            var recruitMenu = TraitorSelectionMinigame.Create();
            recruitMenu.Open(
                Role.ChosenRoles,
                role =>
                {
                    Role.SelectedRole = role;
                    Role.UpdateRole();
                    recruitMenu.Close();
                },
                Role.RandomRole?.Role
            );
        }
    }
}
