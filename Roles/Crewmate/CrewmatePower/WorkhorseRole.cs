using Il2CppInterop.Runtime.Attributes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using DivaniMods.Assets;
using DivaniMods.Modifiers.Crewmate.CrewmatePower;
using DivaniMods.Options;
using TownOfUs;
using TownOfUs.Extensions;
using TownOfUs.Modules.Wiki;
using TownOfUs.Roles;
using TownOfUs.Utilities;
using UnityEngine;

namespace DivaniMods.Roles.Crewmate.CrewmatePower;

public sealed class WorkhorseRole(IntPtr cppPtr)
    : CrewmateRole(cppPtr), ITouCrewRole, IWikiDiscoverable, IDoomable
{
    public static readonly Color WorkhorseColor = new Color32(0x92, 0xD4, 0xDA, 255);

    public bool IsPowerCrew => OptionGroupSingleton<WorkhorseOptions>.Instance.ContinuesGame;

    public string RoleName => "Workhorse";
    public string RoleDescription => "Task. Overwork. Win.";
    public string RoleLongDescription => "Gain an extra set of tasks after completing your original list.\n" +
        "Finsh these extra tasks to result in a task win.";
    public Color RoleColor => WorkhorseColor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmatePower;

    public DoomableType DoomHintType => DoomableType.Trickster;

    [HideFromIl2Cpp] public List<uint> SecondListTaskIds { get; } = [];
    public bool Revealed { get; private set; }

    private Dictionary<byte, ArrowBehaviour>? _evilArrows;
    [HideFromIl2Cpp] public ArrowBehaviour? RevealArrow { get; private set; }

    public string GetAdvancedDescription() => RoleLongDescription + MiscUtils.AppendOptionsText(GetType());

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = MiraAPI.Utilities.Assets.TmpSpriteUtils.CreateSpriteAsset(DivaniAssets.WorkhorseIcon.LoadAsset(), "DivaniMod.Role.Crewmate.Workhorse", 1.45f),
        Icon = DivaniAssets.WorkhorseIcon,
        IntroSound = DivaniAssets.WorkhorseIntroSound,
        MaxRoleCount = 1,
    };

    private void FixedUpdate()
    {
        if (!Player || Player.Data.Role is not WorkhorseRole)
        {
            return;
        }

        if (RevealArrow != null && RevealArrow.target != RevealArrow.transform.parent.position)
        {
            RevealArrow.target = Player.transform.position;
        }

        if (_evilArrows is { Count: > 0 } && Player.AmOwner)
        {
            _evilArrows.ToList().ForEach(arrow => arrow.Value.target = arrow.Value.transform.parent.position);
        }
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);

        ClearArrows();
    }

    public static bool IsEvilTarget(PlayerControl player)
    {
        if (player == null || player.Data == null || player.Data.Role == null)
        {
            return false;
        }

        return player.IsImpostor() || player.Is(RoleAlignment.NeutralKilling);
    }

    public void OnSecondListGranted()
    {
        if (Player.AmOwner)
        {
            Coroutines.Start(MiscUtils.CoFlash(WorkhorseColor, alpha: 0.5f));
            ShowNotification("Your work is not done. A second task list awaits!");
        }
        else if (OptionGroupSingleton<WorkhorseOptions>.Instance.NotifyEvilsOnFirstList &&
                 IsEvilTarget(PlayerControl.LocalPlayer))
        {
            Coroutines.Start(MiscUtils.CoFlash(WorkhorseColor, alpha: 0.5f));
            ShowNotification("The Workhorse has finished their first task list!");
        }
    }

    public void CheckRevealProgress()
    {
        if (Revealed || SecondListTaskIds.Count == 0 || !Player || Player.HasDied())
        {
            return;
        }

        var remaining = SecondListTaskIds.Count(id => Player.Data?.FindTaskById(id)?.Complete != true);
        if (remaining > (int)OptionGroupSingleton<WorkhorseOptions>.Instance.ExtraTasksLeftWhenRevealed.Value)
        {
            return;
        }

        Revealed = true;

        if (Player.AmOwner)
        {
            CreateEvilArrows();
        }
        else if (IsEvilTarget(PlayerControl.LocalPlayer))
        {
            CreateRevealingArrow();
        }
    }

    private void CreateRevealingArrow()
    {
        if (RevealArrow != null)
        {
            return;
        }

        Player.AddModifier<WorkhorseRevealModifier>();
        PlayerNameColor.Set(Player);
        Coroutines.Start(MiscUtils.CoFlash(WorkhorseColor, alpha: 0.5f));

        RevealArrow = MiscUtils.CreateArrow(Player.transform, WorkhorseColor);

        ShowNotification("The Workhorse is almost done! You must stop him, NOW!");
    }

    private void CreateEvilArrows()
    {
        if (_evilArrows != null)
        {
            return;
        }

        _evilArrows = new Dictionary<byte, ArrowBehaviour>();
        Coroutines.Start(MiscUtils.CoFlash(WorkhorseColor, alpha: 0.5f));

        foreach (var evil in Helpers.GetAlivePlayers().Where(IsEvilTarget))
        {
            var color = evil.IsImpostor() ? TownOfUsColors.Impostor : TownOfUsColors.Neutral;
            _evilArrows.Add(evil.PlayerId, MiscUtils.CreateArrow(evil.transform, color));
            PlayerNameColor.Set(evil);
            evil.AddModifier<WorkhorseEvilRevealModifier>();
        }

        ShowNotification("The evils know who you are now, but you can see them now too!");
    }

    public void RemoveArrowForPlayer(byte playerId)
    {
        if (_evilArrows != null && _evilArrows.TryGetValue(playerId, out var arrow))
        {
            arrow.gameObject.Destroy();
            _evilArrows.Remove(playerId);
        }
    }

    public void ClearArrows()
    {
        if (_evilArrows is { Count: > 0 })
        {
            _evilArrows.ToList().ForEach(arrow => arrow.Value.gameObject.Destroy());
            _evilArrows.Clear();
        }

        _evilArrows = null;

        if (RevealArrow != null)
        {
            RevealArrow.gameObject.Destroy();
            RevealArrow = null;
        }

        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player == null)
            {
                continue;
            }

            foreach (var mod in player.GetModifiers<WorkhorseRevealModifier>().ToList())
            {
                player.GetModifierComponent()?.RemoveModifier(mod);
            }

            foreach (var mod in player.GetModifiers<WorkhorseEvilRevealModifier>().ToList())
            {
                player.GetModifierComponent()?.RemoveModifier(mod);
            }
        }
    }

    private static void ShowNotification(string text)
    {
        var notif = MiraAPI.Utilities.Helpers.CreateAndShowNotification(
            $"<b>{WorkhorseColor.ToTextColor()}{text}</color></b>",
            Color.white,
            new Vector3(0f, 1f, -20f),
            spr: DivaniAssets.WorkhorseIcon.LoadAsset());
        notif.AdjustNotification();
    }

    public static List<byte> GenerateSecondListTaskIds(PlayerControl player)
    {
        var ids = new List<byte>();
        var ship = ShipStatus.Instance;
        if (ship == null || player == null)
        {
            return ids;
        }

        var options = OptionGroupSingleton<WorkhorseOptions>.Instance;

        var usedTypes = player.myTasks.ToArray()
            .Select(x => x.TryCast<NormalPlayerTask>())
            .Where(x => x != null)
            .Select(x => x!.TaskType)
            .ToHashSet();

        ids.AddRange(PickTasks(ship.CommonTasks, (int)options.ExtraCommonTasks.Value, usedTypes));
        ids.AddRange(PickTasks(ship.ShortTasks, (int)options.ExtraShortTasks.Value, usedTypes));
        ids.AddRange(PickTasks(ship.LongTasks, (int)options.ExtraLongTasks.Value, usedTypes));

        return ids;
    }

    private static List<byte> PickTasks(Il2CppReferenceArray<NormalPlayerTask> pool, int count,
        HashSet<TaskTypes> usedTypes)
    {
        var picked = new List<byte>();
        if (pool == null || count <= 0)
        {
            return picked;
        }

        foreach (var task in pool.ToArray().OrderBy(_ => UnityEngine.Random.value))
        {
            if (picked.Count >= count)
            {
                break;
            }

            if (task == null || !usedTypes.Add(task.TaskType))
            {
                continue;
            }

            picked.Add((byte)task.Index);
        }

        return picked;
    }
}
