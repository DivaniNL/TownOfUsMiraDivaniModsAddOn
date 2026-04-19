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

// All patches here are registered imperatively from DivaniPlugin.Load via
// Register(Harmony). Harmony.PatchAll aborts on the first unresolved target
// method and every subsequent [HarmonyPatch]-decorated type in the assembly
// silently fails to register - isolating the soundpack with AccessTools
// lookups + null-guards keeps those failures from cascading into unrelated
// plugin patches (credits, button visibility, etc.).
public static class DutchMemeSoundpackPatch
{
    // SoundManager clamps AudioSource.volume to [0,1], so boosting the volume
    // arg past 1f does nothing. Real loudness has to be baked into the sample
    // data. 4x peak with hard clip sounds like a limiter-squashed master,
    // which punches through any vanilla SFX still bleeding through.
    private const float SampleGain = 1f;

    private static AudioClip? _boostedOpen;
    private static AudioClip? _boostedClose;

    // Every vanilla open / close AudioClip reference we've ever seen on a
    // door, recorded by InstanceID. Populated on every ShipStatus.Begin AND
    // every OpenableDoor.SetDoorway call (defense in depth) so PlaySoundPrefix
    // can redirect vanilla clips that somehow survive the field swap.
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

            // Patch every concrete SetDoorway override in the loaded assemblies.
            // Abstract OpenableDoor.SetDoorway can't be patched directly, but
            // hitting every override means we catch every door subclass Among
            // Us (or a mod) ships - including ones we don't know about at
            // compile time - and run SwapOnDoor just before the vanilla body
            // plays its AudioClip field. This is what guarantees non-impostor
            // crewmates also hear the replaced clip: their local door
            // transition is swapped at the moment it fires, not only at Begin.
            var patchedOverrides = 0;
            var openableDoorType = AccessTools.TypeByName("OpenableDoor");
            if (openableDoorType != null)
            {
                foreach (var type in GetAllLoadedTypes())
                {
                    if (type == null) continue;
                    if (type.IsAbstract) continue;
                    if (!openableDoorType.IsAssignableFrom(type)) continue;
                    var setDoorway = AccessTools.Method(type, "SetDoorway", new[] { typeof(bool) });
                    if (setDoorway == null) continue;
                    if (setDoorway.DeclaringType != type) continue;
                    try
                    {
                        harmony.Patch(setDoorway,
                            prefix: new HarmonyMethod(typeof(DutchMemeSoundpackPatch), nameof(SetDoorwayPrefix)));
                        patchedOverrides++;
                    }
                    catch (Exception ex)
                    {
                        DivaniPlugin.Instance.Log.LogWarning($"DutchMemeSoundpack: failed to patch {type.FullName}.SetDoorway: {ex.Message}");
                    }
                }
            }
            DivaniPlugin.Instance.Log.LogInfo($"DutchMemeSoundpack: patched {patchedOverrides} SetDoorway overrides");

            // Patch every SoundManager.PlaySound overload that starts with
            // (AudioClip, bool, float). We don't hard-reference any extra-arg
            // types (like AudioMixerGroup) so builds that ship different
            // overloads all work without needing signature-specific shims.
            foreach (var method in typeof(SoundManager).GetMethods(BindingFlags.Public | BindingFlags.Instance))
            {
                if (method.Name != nameof(SoundManager.PlaySound)) continue;
                var parameters = method.GetParameters();
                if (parameters.Length < 3) continue;
                if (parameters[0].ParameterType != typeof(AudioClip)) continue;
                if (parameters[1].ParameterType != typeof(bool)) continue;
                if (parameters[2].ParameterType != typeof(float)) continue;

                try
                {
                    harmony.Patch(method,
                        prefix: new HarmonyMethod(typeof(DutchMemeSoundpackPatch), nameof(PlaySoundPrefix)));
                }
                catch (Exception ex)
                {
                    DivaniPlugin.Instance.Log.LogWarning($"DutchMemeSoundpack: failed to patch PlaySound overload ({parameters.Length} args): {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            DivaniPlugin.Instance.Log.LogError($"DutchMemeSoundpack: Register failed: {ex}");
        }
    }

    public static void ShipStatusBeginPostfix(ShipStatus __instance)
    {
        if (__instance == null) return;
        var doors = __instance.AllDoors;
        if (doors == null) return;

        var useSoundpack = OptionGroupSingleton<SoundpackOptions>.Instance.UseDutchMemeSoundpack;
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

    // Harmony prefix on every concrete OpenableDoor.SetDoorway override. Runs
    // on every client for every door transition, so even if Begin missed a
    // door (e.g. it was added later) we still redirect its clip just before
    // vanilla plays it. `__instance` is the door component.
    public static void SetDoorwayPrefix(Component __instance)
    {
        if (__instance == null) return;
        if (!OptionGroupSingleton<SoundpackOptions>.Instance.UseDutchMemeSoundpack) return;
        var openClip = GetBoostedOpen();
        var closeClip = GetBoostedClose();
        if (openClip == null || closeClip == null) return;
        SwapOnDoor(__instance, openClip, closeClip);
    }

    // Harmony calls prefixes with parameter names matched by name. `ref` on a
    // reference-type parameter of a non-ref original method rewrites the
    // argument the original method actually receives - that's how we make
    // sure only the Dutch clip ever reaches the audio mixer when the toggle
    // is on, regardless of which code path invoked PlaySound.
    public static void PlaySoundPrefix(ref AudioClip clip, bool loop, ref float volume)
    {
        if (clip == null) return;
        if (!OptionGroupSingleton<SoundpackOptions>.Instance.UseDutchMemeSoundpack) return;

        var id = clip.GetInstanceID();
        if (VanillaOpenClipIds.Contains(id))
        {
            var replacement = GetBoostedOpen();
            if (replacement != null) clip = replacement;
        }
        else if (VanillaCloseClipIds.Contains(id))
        {
            var replacement = GetBoostedClose();
            if (replacement != null) clip = replacement;
        }
    }

    // Reflection-based swap + harvest. Runs over every AudioClip property
    // AND field on the door wrapper so we cover:
    //   - Il2Cpp-wrapper types (PlainDoor, AutoOpenDoor, MushroomWallDoor, etc.)
    //     which expose fields as properties.
    //   - Managed TownOfUs subclasses (AutoOpenMushroomDoor) which use real
    //     C# public fields.
    // Any AudioClip member whose name contains "open" or "close" is treated
    // as a door-sound slot: the current value is recorded by InstanceID (so
    // the PlaySound prefix can redirect it later) and the slot is rewritten
    // to our boosted clip when the toggle is on.
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
                // Il2Cpp wrappers occasionally throw on property access - just
                // skip, the fields pass and PlaySound safety net will handle.
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
                // Same as above - best-effort.
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
        // Look for "open"/"close" anywhere in the member name so both
        // PlainDoor.OpenSound and AutoOpenMushroomDoor.openSound / openClip /
        // doorOpen match. We only ever inspect AudioClip-typed members so
        // false positives on e.g. a hypothetical "CloseEyesClip" are harmless.
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

    // Multiplies every sample by SampleGain and hard-clips to [-1,1]. Hard
    // clipping distorts on loud peaks but that's fine for short meme SFX and
    // much more audible than boosting AudioSource.volume (which the engine
    // clamps to 1.0 so a 4x multiplier on that is effectively a no-op).
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

    private static IEnumerable<Type> GetAllLoadedTypes()
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type?[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                // Grab whatever did load - some assemblies advertise types
                // whose refs can't resolve at runtime (common w/ Il2Cpp).
                types = ex.Types;
            }
            catch
            {
                continue;
            }

            foreach (var type in types)
            {
                if (type != null) yield return type;
            }
        }
    }
}
