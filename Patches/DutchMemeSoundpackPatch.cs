using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using MiraAPI.GameOptions;
using DivaniMods.Assets;
using DivaniMods.Options;
using UnityEngine;

namespace DivaniMods.Patches;

public static class DutchMemeSoundpackPatch
{
    private const float SampleGain = 1f;

    private static AudioClip? _boostedOpen;
    private static AudioClip? _boostedClose;
    private static readonly HashSet<int> VanillaOpenClipIds = new();
    private static readonly HashSet<int> VanillaCloseClipIds = new();

    public static void Register(Harmony harmony)
    {
        try
        {
            var begin = AccessTools.Method(typeof(ShipStatus), nameof(ShipStatus.Begin));
            if (begin != null)
            {
                harmony.Patch(begin,
                    postfix: new HarmonyMethod(typeof(DutchMemeSoundpackPatch), nameof(ShipStatusBeginPostfix))
                    {
                        priority = Priority.Last
                    });
            }
            var hudUpdate = AccessTools.Method(typeof(HudManager), nameof(HudManager.Update));
            if (hudUpdate != null)
            {
                harmony.Patch(hudUpdate,
                    postfix: new HarmonyMethod(typeof(DutchMemeSoundpackPatch), nameof(PollDoorsPostfix)));
            }
        }
        catch (Exception ex)
        {
            DivaniPlugin.Instance.Log.LogError($"DutchMemeSoundpack: Register failed: {ex}");
        }
    }

    private static readonly Dictionary<int, bool> DoorOpenState = new();
    private const float PlayCooldown = 0.25f;
    private const float HearRange = 4f;
    private static float _lastOpenPlay;
    private static float _lastClosePlay;

    private sealed class ActiveSound
    {
        public GameObject Go = null!;
        public AudioSource Src = null!;
        public Vector2 Pos;
        public float EndTime;
    }
    private static readonly List<ActiveSound> ActiveSounds = new();

    public static void PollDoorsPostfix()
    {
        if (!OptionGroupSingleton<DivaniOptions>.Instance.UseDutchMemeSoundpack) return;

        var ship = ShipStatus.Instance;
        if (ship == null) return;
        var doors = ship.AllDoors;
        if (doors == null) return;

        var sm = SoundManager.Instance;
        if (sm == null) return;

        var me = PlayerControl.LocalPlayer;
        if (me == null) return;
        var myPos = me.GetTruePosition();

        var now = Time.time;

        for (var i = ActiveSounds.Count - 1; i >= 0; i--)
        {
            var s = ActiveSounds[i];
            if (s.Src == null || s.Go == null || now >= s.EndTime)
            {
                if (s.Go != null) UnityEngine.Object.Destroy(s.Go);
                ActiveSounds.RemoveAt(i);
                continue;
            }
            s.Src.volume = VolumeFor(Vector2.Distance(myPos, s.Pos));
        }
        var nearestOpen = float.MaxValue; Vector2 openPos = default;
        var nearestClose = float.MaxValue; Vector2 closePos = default;

        foreach (var door in doors)
        {
            if (door == null) continue;
            var id = door.GetInstanceID();
            var open = door.IsOpen;

            if (DoorOpenState.TryGetValue(id, out var prev) && prev != open)
            {
                var pos = (Vector2)door.transform.position;
                var dist = Vector2.Distance(myPos, pos);
                if (open) { if (dist < nearestOpen) { nearestOpen = dist; openPos = pos; } }
                else { if (dist < nearestClose) { nearestClose = dist; closePos = pos; } }
            }

            DoorOpenState[id] = open;
        }

        if (nearestOpen <= HearRange && now - _lastOpenPlay >= PlayCooldown)
        {
            var clip = GetBoostedOpen();
            if (clip != null) { PlayAt(openPos, clip); _lastOpenPlay = now; }
        }

        if (nearestClose <= HearRange && now - _lastClosePlay >= PlayCooldown)
        {
            var clip = GetBoostedClose();
            if (clip != null) { PlayAt(closePos, clip); _lastClosePlay = now; }
        }
    }

    private static void PlayAt(Vector2 pos, AudioClip clip)
    {
        var go = new GameObject("DivaniDoorSfx");
        var src = go.AddComponent<AudioSource>();
        src.clip = clip;
        src.spatialBlend = 0f;
        src.dopplerLevel = 0f;
        src.volume = 0f;
        src.Play();
        ActiveSounds.Add(new ActiveSound
        {
            Go = go,
            Src = src,
            Pos = pos,
            EndTime = Time.time + clip.length + 0.25f,
        });
    }

    private static float VolumeFor(float dist) => Mathf.Clamp01(1f - dist / HearRange);

    public static void ShipStatusBeginPostfix(ShipStatus __instance)
    {
        DoorOpenState.Clear();
        foreach (var s in ActiveSounds)
        {
            if (s.Go != null) UnityEngine.Object.Destroy(s.Go);
        }
        ActiveSounds.Clear();
        if (__instance == null) return;
        var doors = __instance.AllDoors;
        if (doors == null) return;

        var useSoundpack = OptionGroupSingleton<DivaniOptions>.Instance.UseDutchMemeSoundpack;
        var openClip = useSoundpack ? GetBoostedOpen() : null;
        var closeClip = useSoundpack ? GetBoostedClose() : null;

        var swapped = 0;
        foreach (var door in doors)
        {
            if (door == null) continue;
            if (SwapOnDoor(door, openClip, closeClip)) swapped++;
        }

        DivaniPlugin.Instance.Log.LogInfo($"DutchMemeSoundpack: Begin swapped {swapped} doors (soundpack={useSoundpack}, recorded open={VanillaOpenClipIds.Count} close={VanillaCloseClipIds.Count})");
    }
    private static bool SwapOnDoor(Component door, AudioClip? replaceOpen, AudioClip? replaceClose)
    {
        if (door == null) return false;
        var didSwap = false;
        var type = door.GetType();

        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
        {
            if (prop.PropertyType != typeof(AudioClip)) continue;
            if (!prop.CanRead) continue;
            try
            {
                var current = prop.GetValue(door) as AudioClip;
                if (current == null) continue;
                var role = ClassifyClipMember(prop.Name);
                if (role == ClipRole.Other) continue;

                RecordVanilla(current, role);
                if (prop.CanWrite)
                {
                    var replacement = role == ClipRole.Open ? replaceOpen : replaceClose;
                    if (replacement != null)
                    {
                        prop.SetValue(door, replacement);
                        didSwap = true;
                    }
                }
            }
            catch
            {
            }
        }

        foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
        {
            if (field.FieldType != typeof(AudioClip)) continue;
            try
            {
                var current = field.GetValue(door) as AudioClip;
                if (current == null) continue;
                var role = ClassifyClipMember(field.Name);
                if (role == ClipRole.Other) continue;

                RecordVanilla(current, role);
                var replacement = role == ClipRole.Open ? replaceOpen : replaceClose;
                if (replacement != null)
                {
                    field.SetValue(door, replacement);
                    didSwap = true;
                }
            }
            catch
            {
            }
        }

        return didSwap;
    }

    private static void RecordVanilla(AudioClip clip, ClipRole role)
    {
        var id = clip.GetInstanceID();
        if (role == ClipRole.Open) VanillaOpenClipIds.Add(id);
        else if (role == ClipRole.Close) VanillaCloseClipIds.Add(id);
    }

    private enum ClipRole { Other, Open, Close }

    private static ClipRole ClassifyClipMember(string name)
    {
        var lower = name.ToLowerInvariant();
        if (lower.Contains("open")) return ClipRole.Open;
        if (lower.Contains("close")) return ClipRole.Close;
        return ClipRole.Other;
    }

    private static AudioClip? GetBoostedOpen()
    {
        if (_boostedOpen != null) return _boostedOpen;
        _boostedOpen = BuildBoosted(DivaniAssets.DutchDoorOpen.LoadAsset(), "DutchDoorOpen_boosted");
        return _boostedOpen;
    }

    private static AudioClip? GetBoostedClose()
    {
        if (_boostedClose != null) return _boostedClose;
        _boostedClose = BuildBoosted(DivaniAssets.DutchDoorClose.LoadAsset(), "DutchDoorClose_boosted");
        return _boostedClose;
    }

    private static AudioClip? BuildBoosted(AudioClip? src, string name)
    {
        if (src == null) return null;
        try
        {
            var sampleCount = src.samples * src.channels;
            if (sampleCount <= 0) return src;

            var buffer = new Il2CppStructArray<float>(sampleCount);
            src.GetData(buffer, 0);

            for (var i = 0; i < sampleCount; i++)
            {
                var v = buffer[i] * SampleGain;
                if (v > 1f) v = 1f;
                else if (v < -1f) v = -1f;
                buffer[i] = v;
            }

            var copy = AudioClip.Create(name, src.samples, src.channels, src.frequency, false);
            copy.SetData(buffer, 0);
            return copy;
        }
        catch (Exception ex)
        {
            DivaniPlugin.Instance.Log.LogWarning($"DutchMemeSoundpack: failed to boost clip '{src.name}': {ex.Message}");
            return src;
        }
    }
}
