using AmongUs.GameOptions;
using System.Collections;
using System.Linq;
using System.Reflection;
using DivaniMods.Assets;
using DivaniMods.Modifiers.Crewmate.CrewmatePower;
using DivaniMods.Options;
using Il2CppInterop.Runtime.Attributes;
using Il2CppSystem.Runtime.InteropServices;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using TownOfUs;
using TownOfUs.Assets;
using TownOfUs.Extensions;
using TownOfUs.Modules;
using TownOfUs.Modules.Components;
using TownOfUs.Modules.Wiki;
using TownOfUs.Roles;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using TownOfUs.Modifiers;
using System.Text;
using Reactor.Utilities.Extensions;
using TownOfUs.Modifiers.Crewmate;

namespace DivaniMods.Roles.Crewmate.CrewmatePower;

public sealed class MentorRole(IntPtr cppPtr)
    : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    private MeetingMenu? meetingMenu;
    private static readonly Vector3 TeachButtonPosition = new(-0.35f, 0f, -3f);
    private AmbassadorSelectionMinigame? teachMenu;
    private int meetingCount;
    private int tasksCompletedAtLastLesson = -1;
    private readonly HashSet<byte> taughtPlayers = new();
    public static readonly Color MentorColor = new Color32(150, 150, 200, 255);
    public string RoleName => MiraLocaleManager.Get("DivaniMods.Role.Mentor", "Mentor");
    public string LocaleKey => "Mentor";
    public string RoleDescription => MiraLocaleManager.Get("DivaniMods.Role.Mentor.Description", "Teach the Crewmates!");
    public string RoleLongDescription => MiraLocaleManager.Get("DivaniMods.Role.Mentor.LongDescription", "Teach players to learn skills of roles for one round.\nYour lesson fails if it targets a Non-Crewmate.");
    public Color RoleColor => MentorColor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmatePower;

    public override bool IsAffectedByComms => false;

    public DoomableType DoomHintType => DoomableType.Perception;

    public string GetAdvancedDescription() => RoleLongDescription + MiscUtils.AppendOptionsText(GetType());

    public byte StudentId { get; set; } = byte.MaxValue;
    public ushort LessonRoleId { get; set; } = default;

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = DivaniAssets.MentorIcon,
        IconTmp = MiraAPI.Utilities.Assets.TmpSpriteUtils.CreateSpriteAsset(DivaniAssets.MentorIcon.LoadAsset(), "DivaniMod.Role.Crewmate.Mentor", 1.45f),
        IntroSound = DivaniAssets.MentorIntroSound,
        MaxRoleCount = 1
    };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
       var stringB = ITownOfUsRole.SetNewTabText(this);

        stringB.AppendLine(MiraLocaleManager.Get("DivaniMods.Role.Mentor.Tab.CurrentLessons", "Current Lessons:"));

        if (ShowChosenLesson())
        {
            var targetName = GameData.Instance.GetPlayerById(StudentId)?.Object?.Data?.PlayerName;
            var roleObj = RoleManager.Instance.GetRole((RoleTypes)LessonRoleId) as ITownOfUsRole;
            var roleName = roleObj?.RoleName;
            var roleColor = roleObj != null ? ColorUtility.ToHtmlStringRGB(roleObj.RoleColor) : "9999FF";
            var line = MiraLocaleManager.Get("DivaniMods.Role.Mentor.Tab.Student", "Student: [player]. Role Taught: [role]")
                .Replace("[player]", targetName ?? string.Empty)
                .Replace("[role]", $"<color=#{roleColor}>{roleName}</color>");
            stringB.AppendLine($"<b>{line}</b>");
            return stringB;
        }

        return stringB;
    }

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);

        meetingCount = 0;
        tasksCompletedAtLastLesson = -1;
        taughtPlayers.Clear();
        ClearLesson();

        if (Player.AmOwner)
        {
            StudentId = byte.MaxValue;

            meetingMenu = new MeetingMenu(
                this,
                OpenLessonMenu,
                MiraLocaleManager.Get("DivaniMods.Role.Mentor.Ability.Teach", "Teach"),
                MeetingAbilityType.Click,
                DivaniAssets.MentorTeachButton,
                exemption: IsExempt,
                position: TeachButtonPosition);
        }
    }

    public override void OnMeetingStart()
    {
        RoleBehaviourStubs.OnMeetingStart(this);

        meetingCount++;
        ClearLesson();

        var meeting = MeetingHud.Instance;
        if (Player.AmOwner && meeting != null && !Player.HasDied())
        {
            meetingMenu?.GenButtons(meeting, true);
        }
    }

    public override void OnVotingComplete()
    {
        RoleBehaviourStubs.OnVotingComplete(this);

        if (Player.AmOwner)
        {
            meetingMenu?.HideButtons();

            if (teachMenu != null)
            {
                teachMenu.Close();
                teachMenu = null;
            }
        }
    }

    [HideFromIl2Cpp]
    private int CompletedTasks()
    {
        var tasks = Player?.Data?.Tasks;
        return tasks == null ? 0 : tasks.ToArray().Count(t => t.Complete);
    }

    [HideFromIl2Cpp]
    private bool CanTeachNow()
    {
        var options = OptionGroupSingleton<MentorOptions>.Instance;

        if (!options.CanTeachRoundOne.Value && meetingCount <= 1)
        {
            return false;
        }

        var tasksNeeded = (int)options.TasksNeededToTeachAgain.Value;
        if (tasksNeeded > 0 && tasksCompletedAtLastLesson >= 0 && CompletedTasks() - tasksCompletedAtLastLesson < tasksNeeded)
        {
            return false;
        }

        return true;
    }

    [HideFromIl2Cpp]
    public void RecordLesson(byte targetId)
    {
        taughtPlayers.Add(targetId);
        tasksCompletedAtLastLesson = CompletedTasks();
    }

    [HideFromIl2Cpp]
    public bool IsExempt(PlayerVoteArea voteArea)
    {
        if (voteArea == null || voteArea.PlayerId == Player.PlayerId || !CanTeachNow())
        {
            return true;
        }

        if (!OptionGroupSingleton<MentorOptions>.Instance.CanTeachOnSamePlayerAgain.Value && taughtPlayers.Contains(voteArea.PlayerId))
        {
            return true;
        }

        var target = GameData.Instance.GetPlayerById(voteArea.PlayerId)?.Object;

        if (target == null || target.HasDied() || target.HasModifier<MentorTaughtModifier>() || target.HasModifier<MentorInsomniaModifier>() || target.HasModifier<BaseRevealModifier>())
        {
            return true;
        }

        return false;
    }

    [HideFromIl2Cpp]
    public void OpenLessonMenu(PlayerVoteArea voteArea, MeetingHud meeting)
    {
        if (meeting.state == MeetingHud.MeetingStates.Discussion || IsExempt(voteArea))
        {
            return;
        }

        if (Minigame.Instance)
        {
            return;
        }

        var student = GameData.Instance.GetPlayerById(voteArea.PlayerId)?.Object;
        if (student == null)
        {
            return;
        }

        var roles = MiscUtils.GetPotentialRoles().Where(IsRoleValid).ToList();
        if (roles.Count == 0)
        {
            return;
        }

        var targetId = student.PlayerId;
        var menu = AmbassadorSelectionMinigame.Create();
        teachMenu = menu;
        ApplyMenuBackground(menu);
        menu.Open(roles, role =>
        {
            if (role != null)
            {
                OnRoleSelected(role, targetId);
            }
        });

        Coroutines.Start(CoStyleLessonMenu(menu, roles.Count));
    }

    [HideFromIl2Cpp]
    private static IEnumerator CoStyleLessonMenu(AmbassadorSelectionMinigame menu, int roleCount)
    {
        while (menu != null && (menu.RolesHolder == null || menu.RolesHolder.childCount < roleCount + 1))
        {
            yield return null;
        }

        if (menu == null)
        {
            yield break;
        }

        menu.StatusText.text = MiraLocaleManager.Get("DivaniMods.Role.Mentor.Menu.ChooseRole", "Choose a role");
        menu.RoleName.text = MiraLocaleManager.Get("DivaniMods.Role.Mentor", "Mentor");
        menu.RoleTeam.text = MiraLocaleManager.Get("DivaniMods.Role.Mentor.Menu.HoverOverRole", "Hover over a role");
        menu.RoleIcon.sprite = DivaniAssets.MentorIcon.LoadAsset();

        ApplyMenuTheme(menu);

        var randomCard = menu.RolesHolder.GetChild(menu.RolesHolder.childCount - 1);
        var actualCard = randomCard.GetChild(0);
        var crewColor = (Color)Palette.CrewmateBlue;
        var crewIcon = TouRoleIcons.RandomCrew.LoadAsset();
        var randomTeam = MiraLocaleManager.Get("DivaniMods.Role.Mentor.Menu.RandomCrewmate", "Random Crewmate");
        var randomName = MiraLocaleManager.Get("DivaniMods.Role.Mentor.Menu.Random", "Random");

        var cardName = actualCard.GetChild(0).GetComponent<TextMeshPro>();
        var cardIcon = actualCard.GetChild(1).GetComponent<SpriteRenderer>();
        var cardTeam = actualCard.GetChild(2).GetComponent<TextMeshPro>();
        var rollover = actualCard.GetComponent<ButtonRolloverHandler>();
        var button = actualCard.GetComponent<PassiveButton>();

        cardName.color = crewColor;
        cardTeam.text = randomTeam;
        cardTeam.color = crewColor;
        cardIcon.sprite = crewIcon;
        rollover.OverColor = crewColor;

        button.OnMouseOver.AddListener((UnityAction)(() =>
        {
            menu.RoleName.text = randomName;
            menu.RoleTeam.text = randomTeam;
            menu.RoleIcon.sprite = crewIcon;
        }));
    }

    // ---- Mentor theming for the Ambassador role-selection minigame ----------------------------------

    // Dark indigo version of the Mentor colour (the vanilla menu fades in a dark red, 24,0,0,215).
    private static readonly Color MenuBackground = new Color32(14, 14, 38, 215);

    // New saturation = source * SatScale + SatFloor, so grey art picks up a lavender tint and the
    // saturated red ring/swirl art becomes a rich indigo instead of a washed-out grey.
    private const float MenuSatScale = 0.65f;
    private const float MenuSatFloor = 0.20f;

    private static readonly FieldInfo? MenuBackgroundField =
        typeof(AmbassadorSelectionMinigame).GetField("_bgColor", BindingFlags.Instance | BindingFlags.NonPublic);

    private static readonly Dictionary<int, Sprite> RecolouredMenuSprites = new();

    /// <summary>Must run before Open(): the full-screen fade colour is read when the menu begins.</summary>
    [HideFromIl2Cpp]
    private static void ApplyMenuBackground(AmbassadorSelectionMinigame menu)
    {
        try
        {
            MenuBackgroundField?.SetValue(menu, MenuBackground);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[Mentor] Could not recolour the menu background: {e.Message}");
        }
    }

    /// <summary>Runs once the menu's cards exist: title text, ring, swirl and hover underlay.</summary>
    [HideFromIl2Cpp]
    private static void ApplyMenuTheme(AmbassadorSelectionMinigame menu)
    {
        menu.StatusText.color = MentorColor;
        menu.RoleTeam.color = MentorColor;

        RecolourMenuRenderers(menu.RedRing);
        RecolourMenuRenderers(menu.WarpRing);

        // Hover underlay on every role card (the same sprite on each, so it is only recoloured once).
        for (var i = 0; i < menu.RolesHolder.childCount; i++)
        {
            var card = menu.RolesHolder.GetChild(i).GetChild(0);
            if (card.childCount > 3)
            {
                RecolourMenuRenderers(card.GetChild(3).gameObject);
            }
        }
    }

    [HideFromIl2Cpp]
    private static void RecolourMenuRenderers(GameObject? root)
    {
        if (root == null)
        {
            return;
        }

        foreach (var spriteRenderer in root.GetComponentsInChildren<SpriteRenderer>(true))
        {
            var recoloured = RecolourMenuSprite(spriteRenderer.sprite);
            if (recoloured != null)
            {
                spriteRenderer.sprite = recoloured;
            }

            // The tint is baked into the new sprite; a multiplied colour would darken it again.
            spriteRenderer.color = Color.white;
        }
    }

    /// <summary>Hue-shifts a sprite to the Mentor colour, keeping its shading and alpha.</summary>
    [HideFromIl2Cpp]
    private static Sprite? RecolourMenuSprite(Sprite? source)
    {
        if (source == null || source.texture == null)
        {
            return source;
        }

        var key = source.GetInstanceID();
        if (RecolouredMenuSprites.TryGetValue(key, out var cached) && cached != null)
        {
            return cached;
        }

        Color.RGBToHSV(MentorColor, out var hue, out _, out _);

        // Use the sprite's FULL rect, not textureRect: these sprites have tight meshes, so textureRect is only
        // the trimmed bounds. Rebuilding from it while keeping the original pivot is what shifted the wheel.
        var rect = source.rect;
        var width = Mathf.RoundToInt(rect.width);
        var height = Mathf.RoundToInt(rect.height);

        // Bundle textures are not CPU-readable, so read them back through a render texture.
        var temp = RenderTexture.GetTemporary(source.texture.width, source.texture.height, 0, RenderTextureFormat.ARGB32);
        Graphics.Blit(source.texture, temp);
        var previous = RenderTexture.active;
        RenderTexture.active = temp;

        var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.ReadPixels(new Rect(rect.x, rect.y, width, height), 0, 0);

        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(temp);

        // Plain managed maths: calling Color.RGBToHSV/HSVToRGB per pixel crosses into native code every
        // time, which made big ring textures take seconds to recolour. The hue is constant, so only the
        // source saturation and value are needed.
        var h6 = (hue - Mathf.Floor(hue)) * 6f;
        var sector = (int)h6;
        var frac = h6 - sector;

        var pixels = texture.GetPixels32();
        for (var i = 0; i < pixels.Length; i++)
        {
            var pixel = pixels[i];
            if (pixel.a == 0)
            {
                continue;
            }

            var max = Mathf.Max(pixel.r, Mathf.Max(pixel.g, pixel.b));
            var min = Mathf.Min(pixel.r, Mathf.Min(pixel.g, pixel.b));
            var v = max / 255f;
            var s = max == 0 ? 0f : (max - min) / (float)max;
            s = Mathf.Clamp01(s * MenuSatScale + MenuSatFloor);

            var p = v * (1f - s);
            var q = v * (1f - s * frac);
            var t = v * (1f - s * (1f - frac));
            float r, g, b;
            switch (sector)
            {
                case 0: r = v; g = t; b = p; break;
                case 1: r = q; g = v; b = p; break;
                case 2: r = p; g = v; b = t; break;
                case 3: r = p; g = q; b = v; break;
                case 4: r = t; g = p; b = v; break;
                default: r = v; g = p; b = q; break;
            }

            pixels[i] = new Color32(
                (byte)(r * 255f + 0.5f),
                (byte)(g * 255f + 0.5f),
                (byte)(b * 255f + 0.5f),
                pixel.a);
        }

        texture.SetPixels32(pixels);
        texture.Apply();
        texture.filterMode = source.texture.filterMode;
        texture.hideFlags = HideFlags.HideAndDontSave;

        var pivot = new Vector2(source.pivot.x / width, source.pivot.y / height);
        var sprite = Sprite.Create(texture, new Rect(0, 0, width, height), pivot, source.pixelsPerUnit, 0, SpriteMeshType.FullRect);
        sprite.name = source.name + "_Mentor";
        sprite.hideFlags = HideFlags.HideAndDontSave;

        RecolouredMenuSprites[key] = sprite;
        return sprite;
    }

    [HideFromIl2Cpp]
    public static bool IsRoleValid(RoleBehaviour role)
    {
        if (role is not ITownOfUsRole { Team: ModdedRoleTeams.Crewmate } touRole || role is MentorRole)
        {
            return false;
        }

        if (role.GetRoleAlignment() is RoleAlignment.CrewmateAfterlife || role.GetRoleAlignment() is RoleAlignment.CrewmateHider)
        {
            return false;
        }

        if (role is MayorRole or PoliticianRole or MonarchRole or TimeLordRole)
        {
            return false;
        }

        var options = OptionGroupSingleton<MentorOptions>.Instance;
        return touRole.RoleAlignment switch
        {
            RoleAlignment.CrewmateKilling => options.CanTeachCrewKilling.Value,
            RoleAlignment.CrewmatePower => options.CanTeachCrewPower.Value,
            RoleAlignment.CrewmateInvestigative => options.CanTeachCrewInves.Value,
            _ => true,
        };
    }

    [HideFromIl2Cpp]
    public void OnRoleSelected(RoleBehaviour role, byte targetId)
    {
        var target = GameData.Instance.GetPlayerById(targetId)?.Object;

        if (target == null)
        {
            return;
        }

        teachMenu?.Close();
        teachMenu = null;
        RpcSetLessonTarget(Player, targetId, RoleId.Get(role.GetType()));

        // One lesson per meeting: hide the Teach buttons once a student has been chosen.
        meetingMenu?.HideButtons();
        ShowTaughtIndicator(targetId);

        var roleObj = role as ITownOfUsRole;

        var notif = Helpers.CreateAndShowNotification(
            $"<b>{MiraLocaleManager.Get("DivaniMods.Role.Mentor.Notification.WillTeach").Replace("<player>", $"{MentorColor.ToTextColor()}{target.Data.PlayerName}</color>").Replace("<role>", $"{roleObj?.RoleColor.ToTextColor()}{roleObj?.RoleName}</color>")}</b>",
            Color.white, new Vector3(0f, 1f, -20f), spr: DivaniAssets.MentorIcon.LoadAsset());

        notif.AdjustNotification();
    }

    [HideFromIl2Cpp]
    private static void ShowTaughtIndicator(byte targetId)
    {
        var meeting = MeetingHud.Instance;
        if (meeting == null)
        {
            return;
        }

        var voteArea = meeting.playerStates.FirstOrDefault(state => state.PlayerId == targetId);
        if (voteArea == null)
        {
            return;
        }

        var indicator = new GameObject("MentorTaughtIndicator");
        indicator.layer = voteArea.gameObject.layer;
        indicator.transform.SetParent(voteArea.transform, false);
        indicator.transform.localPosition = TeachButtonPosition;
        indicator.AddComponent<SpriteRenderer>().sprite = DivaniAssets.MentorTaughtMeeting.LoadAsset();
    }

    [MethodRpc((uint)DivaniRpcCalls.MentorSetLessonTarget)]
    public static void RpcSetLessonTarget(PlayerControl Mentor, byte targetId, ushort roleId)
    {
        if (Mentor?.Data?.Role is not MentorRole mentorRole)
        {
            return;
        }

        mentorRole.StudentId = targetId;
        mentorRole.LessonRoleId = roleId;
        mentorRole.RecordLesson(targetId);
    }

    [MethodRpc((uint)DivaniRpcCalls.MentorLesson)]
    public static void RpcLesson(PlayerControl Mentor, byte targetId, ushort LessonRoleId)
    {
        var canApplyRole = true;
        var target = MiscUtils.PlayerById(targetId);

        if (!IsValidLessonTarget(target, Mentor))
        {
            return;
        }

        if (Mentor?.Data?.Role is not MentorRole mentorRole)
        {
            return;
        }

        var role = RoleManager.Instance.GetRole((RoleTypes)LessonRoleId);
        var options = OptionGroupSingleton<MentorOptions>.Instance;

        if (IsBreakingMaxRoleCount(role, target))
        {
            if (AmongUsClient.Instance.AmHost)
            {
                var randomRole = GetRandomValidRole(target);
                if (randomRole == null)
                {
                    canApplyRole = false;
                }
                RpcSetLessonTarget(Mentor, targetId, (ushort)randomRole.Role);
            }
            LessonRoleId = mentorRole.LessonRoleId;
            role = RoleManager.Instance.GetRole((RoleTypes)LessonRoleId);

            if (Mentor.AmOwner)
            {
                var roleObj = role as ITownOfUsRole;
                var notif = Helpers.CreateAndShowNotification(
                $"<b>{MiraLocaleManager.Get("DivaniMods.Role.Mentor.Notification.Redirected").Replace("<player>", $"{MentorColor.ToTextColor()}{target.Data.PlayerName}</color>").Replace("<role>", $"{roleObj?.RoleColor.ToTextColor()}{roleObj?.RoleName}</color>")}</b>",
                Color.white, new Vector3(0f, 1f, -20f), spr: DivaniAssets.MentorIcon.LoadAsset());

                notif.AdjustNotification();
            }
        }

        if (!target.IsCrewmate())
        {
            if (options.NotifyTargetOfRoleOnAttempt.Value)
            {
                if (target.AmOwner)
                {
                    var roleObj = RoleManager.Instance.GetRole((RoleTypes)mentorRole.LessonRoleId) as ITownOfUsRole;

                    var notif = Helpers.CreateAndShowNotification(
                    $"<b>{MiraLocaleManager.Get("DivaniMods.Role.Mentor.Notification.FailedLessonWithRole").Replace("<role>", $"{roleObj?.RoleColor.ToTextColor()}{roleObj?.RoleName}</color>")}</b>",
                    Color.white, new Vector3(0f, 1f, -20f), spr: DivaniAssets.MentorIcon.LoadAsset());

                    notif.AdjustNotification();
                }
            }
            else if (options.NotifyTargetOnAttempt.Value)
            {
                if (target.AmOwner)
                {
                    var notif = Helpers.CreateAndShowNotification(
                    $"<b>{MiraLocaleManager.Get("DivaniMods.Role.Mentor.Notification.FailedLesson")}</b>",
                    Color.white, new Vector3(0f, 1f, -20f), spr: DivaniAssets.MentorIcon.LoadAsset());

                    notif.AdjustNotification();
                }
            }
            canApplyRole = false;
        }

        if (AmongUsClient.Instance.AmHost)
        {
            target.RpcAddModifier<MentorInsomniaModifier>((int)options.InsomniaRounds.Value);

            if (canApplyRole)
            {
                if (target.HasModifier<ImitatorCacheModifier>())
                {
                    target.RemoveModifier<ImitatorCacheModifier>();
                    target.RpcChangeRole(RoleId.Get<ImitatorRole>(), false);
                }
                var originalRole = (ushort)target.Data.Role.Role;
                target.RpcChangeRole(LessonRoleId, false);
                target.RpcAddModifier<MentorTaughtModifier>(originalRole, LessonRoleId);
            }
        }
    }

    public static bool IsValidLessonTarget(PlayerControl? target, PlayerControl Mentor)
    {
        if (target == null || Mentor == null)
        {
            return false;
        }

        if (target.Data == null || target.Data.Disconnected)
        {
            return false;
        }

        if (target.HasDied() || target.PlayerId == Mentor.PlayerId)
        {
            return false;
        }

        if (target.HasModifier<MentorTaughtModifier>() || target.HasModifier<MentorInsomniaModifier>())
        {
            return false;
        }

        return true;
    }

    public void ClearLesson()
    {
        StudentId = byte.MaxValue;
        LessonRoleId = default;
    }

    [HideFromIl2Cpp]
    public static bool IsBreakingMaxRoleCount(RoleBehaviour role, PlayerControl target)
    {
        if (role is not ICustomRole customRole || customRole.GetCount() is not int cap || cap == 0)
        {
            return false;
        }

        var aliveWithRole = PlayerControl.AllPlayerControls.ToArray()
            .Count(p => p != null && p.Data?.Role != null && p.Data.Role.Role == role.Role && !p.HasDied() && p != target);

        return aliveWithRole >= cap;
    }

    [HideFromIl2Cpp]
    public static RoleBehaviour? GetRandomValidRole(PlayerControl target)
    {
        var pool = MiscUtils.GetPotentialRoles()
            .Where(r => IsRoleValid(r) && !IsBreakingMaxRoleCount(r, target))
            .ToList();

        return pool.Count == 0 ? null : pool[UnityEngine.Random.Range(0, pool.Count)];
    }

    public bool ShowChosenLesson()
    {
        return !(LessonRoleId == default) || !(StudentId == byte.MaxValue);
    }
}
