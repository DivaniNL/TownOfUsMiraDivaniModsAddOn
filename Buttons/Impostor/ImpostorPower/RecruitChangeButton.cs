using System.Linq;
using AmongUs.GameOptions;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Roles;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities.Extensions;
using DivaniMods.Assets;
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
        var blocked = GetUnavailableRoles();
        Role.ChosenRoles.RemoveAll(role => blocked.Contains((ushort)role.Role));

        if (Role.ChosenRoles.Count == 0)
        {
            BuildChoices(blocked);
        }

        if (Role.ChosenRoles.Count == 0)
        {
            ShowNoRolesNotification();
            return;
        }

        if (Role.RandomRole == null || blocked.Contains((ushort)Role.RandomRole.Role))
        {
            Role.RandomRole = Role.ChosenRoles.Random();
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

    private void BuildChoices(List<ushort> blocked)
    {
        var excluded = MiscUtils.AllRegisteredRoles
            .Where(x => x is ISpawnChange { NoSpawn: true } || x.Role is RoleTypes.Impostor || x.IsDead ||
                        x is ITownOfUsRole { RoleAlignment: RoleAlignment.ImpostorPower })
            .Select(x => x.Role).ToList();
        var impRoles = MiscUtils.GetRolesToAssign(ModdedRoleTeams.Impostor, x => !excluded.Contains(x.Role))
            .Select(x => x.RoleType).ToList();
        impRoles.RemoveAll(blocked.Contains);

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
            impRoles.RemoveAll(blocked.Contains);
            roleList = MiscUtils.AllRegisteredRoles
                .Where(role => role is not ITraitorIgnore ignore || !ignore.IsIgnored)
                .Where(role => impRoles.Contains((ushort)role.Role))
                .Where(role => role is not TraitorRole and not RecruitRole)
                .ToList();
        }

        if (roleList.Count == 0)
        {
            return;
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

    private static List<ushort> GetUnavailableRoles()
    {
        var taken = new Dictionary<ushort, int>();
        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player == null || player.Data == null || player.AmOwner)
            {
                continue;
            }

            var role = player.Data.IsDead ? player.GetRoleWhenAlive() : player.Data.Role;
            if (!role || !role.IsImpostor)
            {
                continue;
            }

            var roleId = (ushort)role.Role;
            taken[roleId] = taken.TryGetValue(roleId, out var count) ? count + 1 : 1;
        }

        var removeExisting = OptionGroupSingleton<RecruiterOptions>.Instance.RemoveExistingRoles;
        var blocked = new List<ushort>();

        foreach (var entry in taken)
        {
            if (removeExisting)
            {
                blocked.Add(entry.Key);
                continue;
            }

            if (RoleManager.Instance.GetRole((RoleTypes)entry.Key) is ICustomRole custom &&
                custom.Configuration.MaxRoleCount > 0 && entry.Value >= custom.Configuration.MaxRoleCount)
            {
                blocked.Add(entry.Key);
            }
        }

        return blocked;
    }

    private static void ShowNoRolesNotification()
    {
        var notif = MiraAPI.Utilities.Helpers.CreateAndShowNotification(
            "<b>No Impostor roles are available to change into.</b>",
            Color.white,
            new Vector3(0f, 1f, -20f),
            spr: DivaniAssets.RecruitIcon.LoadAsset());
        notif.AdjustNotification();
    }
}
