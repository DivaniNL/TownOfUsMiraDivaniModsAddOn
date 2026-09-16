using DivaniMods.Assets;
using DivaniMods.Options;
using DivaniMods.Roles.Neutral.NeutralKilling;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using Reactor.Utilities;
using System.Collections;
using TownOfUs.Modules;
using TownOfUs.Modules.ControlSystem;
using TownOfUs.Networking;
using TownOfUs.Roles.Impostor;
using TownOfUs.Utilities;
using UnityEngine;

namespace DivaniMods.Modules.Monster;

public static class MonsterState
{
    private static readonly Dictionary<byte, List<byte>> stomachs = [];
    private static readonly Dictionary<byte, byte> eatenBy = [];
    private static readonly HashSet<byte> pendingDigestSort = [];
    private static IEnumerator? spectateRoutine;

    public static bool IsEaten(byte playerId) => eatenBy.ContainsKey(playerId);

    public static byte? MonsterOf(byte victimId) =>
        eatenBy.TryGetValue(victimId, out var id) ? id : null;

    public static int CountHeldBy(byte MonsterId) =>
        stomachs.TryGetValue(MonsterId, out var list) ? list.Count : 0;

    public static List<byte> GetHeld(byte MonsterId) =>
        stomachs.TryGetValue(MonsterId, out var list) ? list.ToList() : [];

    public static float CooldownFor(byte MonsterId)
    {
        var options = OptionGroupSingleton<MonsterOptions>.Instance;
        var cooldown = options.DevourCooldown + options.DevourCooldownIncreasePerDevour * CountHeldBy(MonsterId);
        return Mathf.Clamp(cooldown, 5f, 120f);
    }

    public static int MaxPerRound()
    {
        var configured = (int)OptionGroupSingleton<MonsterOptions>.Instance.MaxDevouredPerRound;
        return configured <= 0 ? int.MaxValue : configured;
    }

    public static bool HasRoomToEat(byte MonsterId) => CountHeldBy(MonsterId) < MaxPerRound();

    public static void Eat(byte MonsterId, byte victimId)
    {
        var victim = MiscUtils.PlayerById(victimId);
        var Monster = MiscUtils.PlayerById(MonsterId);
        if (victim == null || victim.HasDied()) return;

        if (!stomachs.TryGetValue(MonsterId, out var list))
            stomachs[MonsterId] = list = [];
        list.Add(victimId);
        eatenBy[victimId] = MonsterId;

        BreakControllingEffects(victim);

        victim.moveable = false;
        var collider = victim.GetComponent<Collider2D>();
        if (collider != null) collider.enabled = false;

        if (Monster != null)
        {
            var victimPos = victim.transform.position;
            var localPlayer = PlayerControl.LocalPlayer;

            var options = OptionGroupSingleton<MonsterOptions>.Instance;
            var isVisibleToEveryone = options.DevourAnimVisibleToEveryone;

            var isMonsterOrTarget = localPlayer != null &&
                (localPlayer.PlayerId == Monster.PlayerId || localPlayer.PlayerId == victim.PlayerId);

            if (isMonsterOrTarget)
            {
                var swallowSound = DivaniAssets.MonsterDevourSound?.LoadAsset();
                if (swallowSound != null)
                {
                    SoundManager.Instance.PlaySound(swallowSound, false);
                }
            }

            var canPlayForLocal = isMonsterOrTarget || (isVisibleToEveryone && LocalCanSee(victimPos));

            if (canPlayForLocal)
            {
                try
                {
                    MonsterDevourAnimation.Play(Monster, victim, victimPos, 0.5f, null);
                }
                catch
                {
                    victim.Visible = false;
                    SnapTo(victim, Monster.GetTruePosition());
                }
            }
            else
            {
                victim.Visible = false;
                SnapTo(victim, Monster.GetTruePosition());
            }
        }
        else
        {
            victim.Visible = false;
        }

        if (victim.AmOwner)
        {
            Notify(victim, "You have been eaten by the Monster!");
            BeginSpectating(MonsterId);
        }
    }

    public static bool IsDigesting { get; private set; }

    public static List<byte> PendingDigestSortIds() => pendingDigestSort.ToList();

    public static void ClearPendingDigestSort(byte victimId) => pendingDigestSort.Remove(victimId);

    public static void DigestAll(byte MonsterId)
    {
        if (!stomachs.TryGetValue(MonsterId, out var victims)) return;
        var Monster = MiscUtils.PlayerById(MonsterId);
        var localPlayer = PlayerControl.LocalPlayer;
        var shouldPlaySound = false;

        IsDigesting = true;
        try
        {
            foreach (var victimId in victims.ToList())
            {
                var victim = MiscUtils.PlayerById(victimId);
                if (victim != null && !victim.HasDied())
                {
                    if (localPlayer != null &&
                        (localPlayer.PlayerId == MonsterId || localPlayer.PlayerId == victimId))
                    {
                        shouldPlaySound = true;
                    }

                    victim.Visible = true;
                    victim.moveable = true;
                    var collider = victim.GetComponent<Collider2D>();
                    if (collider != null) collider.enabled = true;

                    if (Monster != null)
                        Monster.RpcSpecialMurder(victim,true,true,true,false,false,false,false,true,"Monster");
                    else
                        victim.Die(DeathReason.Kill, false);

                    pendingDigestSort.Add(victimId);

                    if (victim.AmOwner) EndSpectating();
                }

                eatenBy.Remove(victimId);
            }
        }
        finally
        {
            IsDigesting = false;
        }

        if (shouldPlaySound)
        {
            var swallowSound = DivaniAssets.MonsterDevourSound?.LoadAsset();
            if (swallowSound != null)
            {
                SoundManager.Instance.PlaySound(swallowSound, false);
            }
        }

        stomachs.Remove(MonsterId);
    }

    public static void ReleaseAll(byte MonsterId, Vector2 atPosition)
    {
        if (!stomachs.TryGetValue(MonsterId, out var victims)) return;

        foreach (var victimId in victims.ToList())
        {
            var victim = MiscUtils.PlayerById(victimId);
            if (victim != null && !victim.HasDied())
            {
                victim.Visible = true;
                victim.moveable = true;
                var collider = victim.GetComponent<Collider2D>();
                if (collider != null) collider.enabled = true;

                SnapTo(victim, atPosition);

                if (victim.AmOwner)
                {
                    EndSpectating();
                    Notify(victim, "You have been released from the Monster!");
                }
            }

            eatenBy.Remove(victimId);
        }

        stomachs.Remove(MonsterId);
    }

    public static void ForgetMonster(byte MonsterId)
    {
        if (stomachs.TryGetValue(MonsterId, out var victims))
        {
            foreach (var victimId in victims)
            {
                var victim = MiscUtils.PlayerById(victimId);
                if (victim != null)
                {
                    victim.Visible = true;
                    victim.moveable = true;
                    var collider = victim.GetComponent<Collider2D>();
                    if (collider != null) collider.enabled = true;
                    if (victim.AmOwner) EndSpectating();
                }
                eatenBy.Remove(victimId);
            }
        }

        stomachs.Remove(MonsterId);
    }

    public static void ResetAll()
    {
        EndSpectating();

        foreach (var victimId in eatenBy.Keys.ToList())
        {
            var victim = MiscUtils.PlayerById(victimId);
            if (victim == null) continue;

            victim.Visible = true;
            victim.moveable = true;
            var collider = victim.GetComponent<Collider2D>();
            if (collider != null) collider.enabled = true;
        }

        stomachs.Clear();
        eatenBy.Clear();
        pendingDigestSort.Clear();
    }

    private static void BreakControllingEffects(PlayerControl victim)
    {
        try
        {
            if (ParasiteControlState.IsControlled(victim.PlayerId, out var parasiteId))
            {
                var parasite = MiscUtils.PlayerById(parasiteId);
                if (parasite?.Data?.Role is ParasiteRole)
                    ParasiteRole.RpcParasiteEndControl(parasite, victim);
            }
        }
        catch { }

        try
        {
            if (PuppeteerControlState.IsControlled(victim.PlayerId, out var puppeteerId))
            {
                var puppeteer = MiscUtils.PlayerById(puppeteerId);
                if (puppeteer?.Data?.Role is PuppeteerRole)
                    PuppeteerRole.RpcPuppeteerEndControl(puppeteer, victim);
            }
        }
        catch { }

        try
        {
            if (victim.Data?.Role is ParasiteRole parasiteRole && parasiteRole.Controlled != null)
                ParasiteRole.RpcParasiteEndControl(victim, parasiteRole.Controlled);
        }
        catch { }

        try
        {
            if (victim.Data?.Role is PuppeteerRole puppeteerRole && puppeteerRole.Controlled != null)
                PuppeteerRole.RpcPuppeteerEndControl(victim, puppeteerRole.Controlled);
        }
        catch { }
    }

    private static void BeginSpectating(byte MonsterId)
    {
        EndSpectating();
        spectateRoutine = CoSpectate(MonsterId);
        Coroutines.Start(spectateRoutine);
    }

    private static void EndSpectating()
    {
        if (spectateRoutine != null)
        {
            Coroutines.Stop(spectateRoutine);
            spectateRoutine = null;
        }

        var local = PlayerControl.LocalPlayer;
        if (local != null && Camera.main != null)
            Camera.main.GetComponent<FollowerCamera>()?.SetTarget(local);
    }

    private static IEnumerator CoSpectate(byte MonsterId)
    {
        var local = PlayerControl.LocalPlayer;
        if (local == null) yield break;

        while (true)
        {
            if (AmongUsClient.Instance == null || !AmongUsClient.Instance.IsGameStarted || !IsEaten(local.PlayerId))
                yield break;

            var Monster = MiscUtils.PlayerById(MonsterId);
            if (Monster != null && !Monster.HasDied() && Camera.main != null)
            {
                var follower = Camera.main.GetComponent<FollowerCamera>();
                if (follower != null && follower.Target != Monster)
                    follower.SetTarget(Monster);
            }

            yield return null;
        }
    }

    private static bool LocalCanSee(Vector2 pos)
    {
        var local = PlayerControl.LocalPlayer;
        if (local == null) return false;

        var localPos = local.GetTruePosition();
        var viewDistance = local.lightSource != null ? local.lightSource.ViewDistance : 5f;

        var offset = pos - localPos;
        var dist = offset.magnitude;
        if (dist > viewDistance) return false;

        var dirNorm = dist > 0f ? offset / dist : Vector2.zero;
        return !PhysicsHelpers.AnyNonTriggersBetween(localPos, dirNorm, dist, Constants.ShadowMask);
    }

    private static void SnapTo(PlayerControl player, Vector2 pos)
    {
        player.transform.position = new Vector3(pos.x, pos.y, player.transform.position.z);
        if (player.MyPhysics?.body != null)
        {
            player.MyPhysics.body.position = pos;
            player.MyPhysics.body.velocity = Vector2.zero;
        }
    }

    private static void Notify(PlayerControl player, string message)
    {
        var hex = ColorUtility.ToHtmlStringRGB(MonsterRole.MonsterColor);
        var notification = Helpers.CreateAndShowNotification(
            $"<b><color=#{hex}>{message}</color></b>",
            Color.white,
            new Vector3(0f, 1f, -20f),
            spr: DivaniAssets.MonsterIcon.LoadAsset());
        notification.AdjustNotification();
    }
}