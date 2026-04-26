using System;
using System.Collections.Generic;
using HarmonyLib;
using MiraAPI.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace DivaniMods.Utilities;

/// <summary>
/// Renders a vertical stack of timed (or static) status lines at the top of
/// the HUD, using the same <see cref="LobbyNotificationMessage"/> prefab the
/// game and Mira use for bottom notifications (e.g. Thief / Sentinel /
/// Disperser via <see cref="Helpers.CreateAndShowNotification"/>) so text and
/// icon scale match 1:1.
/// <para>
/// Add / remove by string id. Order is controlled by the optional
/// <c>priority</c> parameter passed to <see cref="Set"/> - lower values are
/// stacked nearer the top. Callers own their own ids and priorities; this
/// utility never hard-codes consumers.
/// </para>
/// <para>
/// All HUD-touching code is wrapped in defensive try / catch + Unity null
/// checks so a single bad frame (e.g. stale references after returning to the
/// lobby) cannot bubble up out of <c>HudManager.Update</c> and break unrelated
/// Harmony postfixes (which is what was causing custom buttons to stay
/// visible during meetings).
/// </para>
/// </summary>
public static class DivaniTimers
{
    public const int DefaultPriority = 100;

    private const float TopAnchorY = 2.5f;
    private const float ZLayer = -20f;
    /// <summary>Vertical gap between row centers (roughly one notification line).</summary>
    private const float RowStep = 0.42f;

    private static GameObject? _stackRoot;
    private static GameObject? _rowTemplate;
    private static readonly List<Entry> Entries = new();
    private static readonly List<GameObject> RowPool = new();

    private sealed class Entry
    {
        public string Id = "";
        public GameObject? Go;
        public TextMeshPro? Tmp;
        public Sprite? Icon;
        public string TitleRich = "";
        public float? SecondsRemaining;
        public bool IsCountingDown;
        /// <summary>When true, <see cref="Tick"/> decrements <see cref="SecondsRemaining"/>. When false, the caller sets seconds (game state is authoritative).</summary>
        public bool UseLocalTimeDelta;
        /// <summary>Lower values are placed nearer the top of the stack.</summary>
        public int Priority = DefaultPriority;
        /// <summary>Stable tiebreaker so equal-priority rows keep insertion order.</summary>
        public int InsertionOrder;
    }

    private static int _nextInsertionOrder;

    public static IReadOnlyList<string> ActiveIds
    {
        get
        {
            var list = new List<string>(Entries.Count);
            for (var i = 0; i < Entries.Count; i++)
            {
                list.Add(Entries[i].Id);
            }

            return list;
        }
    }

    /// <summary>
    /// Add or update a timer row.
    /// </summary>
    /// <param name="id">Stable identifier the caller owns (e.g. <c>"divani.lockdown"</c>).</param>
    /// <param name="titleRichText">Rich text for the line (e.g. <c>&lt;b&gt;&lt;color=#CC3333&gt;LOCKDOWN&lt;/color&gt;&lt;/b&gt;</c>).</param>
    /// <param name="icon">Optional icon; if null, the icon side stays empty.</param>
    /// <param name="seconds">If <see langword="null"/>, no countdown suffix is shown and the row is static until removed.</param>
    /// <param name="useLocalTimeDelta">If <see langword="true"/>, <see cref="Tick"/> counts down and removes the row at 0. If <see langword="false"/>, only the supplied <paramref name="seconds"/> is displayed; the caller must <see cref="Remove"/> when done.</param>
    /// <param name="priority">Lower values are stacked higher up. Default <see cref="DefaultPriority"/>.</param>
    public static void Set(
        string id,
        string titleRichText,
        Sprite? icon = null,
        float? seconds = null,
        bool useLocalTimeDelta = true,
        int priority = DefaultPriority)
    {
        if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(titleRichText)) return;

        try
        {
            for (var i = 0; i < Entries.Count; i++)
            {
                if (Entries[i].Id == id)
                {
                    var e = Entries[i];
                    e.TitleRich = titleRichText;
                    e.Icon = icon;
                    e.SecondsRemaining = seconds;
                    e.IsCountingDown = seconds.HasValue;
                    e.UseLocalTimeDelta = useLocalTimeDelta;
                    var oldPriority = e.Priority;
                    e.Priority = priority;
                    if (icon != null && IsAlive(e.Go)) ApplyIcon(e.Go!, icon);
                    ApplyText(e);
                    if (oldPriority != priority)
                    {
                        SortEntries();
                        Relayout();
                    }
                    return;
                }
            }

            if (!TryEnsureStack()) return;
            if (!TryCreateRow(out var go, out var tmp)) return;

            var entry = new Entry
            {
                Id = id,
                Go = go,
                Tmp = tmp,
                TitleRich = titleRichText,
                Icon = icon,
                SecondsRemaining = seconds,
                IsCountingDown = seconds.HasValue,
                UseLocalTimeDelta = useLocalTimeDelta,
                Priority = priority,
                InsertionOrder = _nextInsertionOrder++
            };

            if (icon != null) ApplyIcon(go, icon);
            ApplyText(entry);

            Entries.Add(entry);
            go.SetActive(true);

            SortEntries();
            Relayout();
        }
        catch (Exception ex)
        {
            DivaniPlugin.Instance?.Log.LogWarning($"DivaniTimers.Set({id}): {ex.Message}");
        }
    }

    public static void Remove(string id)
    {
        try
        {
            for (var i = 0; i < Entries.Count; i++)
            {
                if (Entries[i].Id != id) continue;
                ReturnToPool(Entries[i].Go);
                Entries.RemoveAt(i);
                Relayout();
                return;
            }
        }
        catch (Exception ex)
        {
            DivaniPlugin.Instance?.Log.LogWarning($"DivaniTimers.Remove({id}): {ex.Message}");
        }
    }

    /// <summary>
    /// Drop every timer and forget all references. Called on game end and on
    /// the next intro so we never reuse a GameObject that belonged to the
    /// previous HudManager (those become destroyed Unity objects when the
    /// scene reloads, and using them throws inside HudManager.Update).
    /// </summary>
    public static void Clear()
    {
        try
        {
            for (var i = 0; i < Entries.Count; i++)
            {
                ReturnToPool(Entries[i].Go);
            }

            Entries.Clear();
            RowPool.Clear();
            _rowTemplate = null;
            _stackRoot = null;
        }
        catch (Exception ex)
        {
            DivaniPlugin.Instance?.Log.LogWarning($"DivaniTimers.Clear: {ex.Message}");
        }
    }

    /// <summary>Per-frame: countdown and hide during meeting/exile.</summary>
    public static void Tick()
    {
        try
        {
            // If the stack root was destroyed (scene change without an explicit
            // Clear() yet), drop every reference so the next Set() rebuilds
            // cleanly against the new HudManager.
            if (_stackRoot != null && !IsAlive(_stackRoot))
            {
                ForgetReferences();
            }

            var inMeeting = MeetingHud.Instance != null || ExileController.Instance != null;
            if (inMeeting)
            {
                if (IsAlive(_stackRoot)) _stackRoot!.SetActive(false);
                return;
            }

            if (!IsAlive(_stackRoot)) return;
            if (Entries.Count == 0)
            {
                _stackRoot!.SetActive(false);
                return;
            }

            _stackRoot!.SetActive(true);

            var anyRemoved = false;
            for (var i = 0; i < Entries.Count;)
            {
                var e = Entries[i];
                if (e.IsCountingDown && e.SecondsRemaining.HasValue && e.UseLocalTimeDelta)
                {
                    e.SecondsRemaining -= Time.deltaTime;
                    if (e.SecondsRemaining <= 0f)
                    {
                        ReturnToPool(e.Go);
                        Entries.RemoveAt(i);
                        anyRemoved = true;
                        continue;
                    }
                }

                ApplyText(e);
                i++;
            }

            if (anyRemoved) Relayout();
        }
        catch (Exception ex)
        {
            // Never let an exception escape into HudManager.Update - that's
            // exactly what was breaking other Harmony patches and leaving
            // custom buttons visible during meetings. Drop everything so the
            // next frame can rebuild from scratch.
            ForgetReferences();
            DivaniPlugin.Instance?.Log.LogWarning($"DivaniTimers.Tick: {ex.Message}");
        }
    }

    // --------------------------------------------------------------------
    // Internals
    // --------------------------------------------------------------------

    private static void ForgetReferences()
    {
        for (var i = 0; i < Entries.Count; i++)
        {
            Entries[i].Go = null;
            Entries[i].Tmp = null;
        }

        Entries.Clear();
        RowPool.Clear();
        _stackRoot = null;
        _rowTemplate = null;
    }

    private static void ReturnToPool(GameObject? go)
    {
        if (!IsAlive(go)) return;
        go!.SetActive(false);
        RowPool.Add(go);
    }

    /// <summary>
    /// Robust Unity null-check: a destroyed Unity object is not "really" null
    /// from the C# side but its overloaded == operator returns true.
    /// </summary>
    private static bool IsAlive(UnityEngine.Object? obj) => obj != null;

    private static bool TryEnsureStack()
    {
        if (!HudManager.InstanceExists || HudManager.Instance == null) return false;

        var hud = HudManager.Instance;
        if (!IsAlive(_stackRoot))
        {
            _stackRoot = new GameObject("DivaniTimerStack");
            _stackRoot.transform.SetParent(hud.transform, false);
            _stackRoot.transform.localPosition = new Vector3(0f, TopAnchorY, ZLayer);
            _stackRoot.transform.localScale = Vector3.one;
            _stackRoot.layer = LayerMask.NameToLayer("UI");
        }

        return true;
    }

    private static bool TryCreateRow(out GameObject go, out TextMeshPro? tmp)
    {
        go = null!;
        tmp = null;
        if (!TryEnsureRowTemplate()) return false;
        if (!IsAlive(_rowTemplate)) return false;

        // Drain destroyed pool entries that survived a scene reload.
        while (RowPool.Count > 0)
        {
            var candidate = RowPool[RowPool.Count - 1];
            RowPool.RemoveAt(RowPool.Count - 1);
            if (IsAlive(candidate))
            {
                go = candidate;
                break;
            }
        }

        if (!IsAlive(go))
        {
            go = Object.Instantiate(_rowTemplate!, _stackRoot?.transform, false);
            go.name = "DivaniTimerRow";
            StopAutoDispose(go);
        }

        if (IsAlive(_stackRoot))
        {
            go.transform.SetParent(_stackRoot!.transform, false);
        }
        go.SetActive(true);

        tmp = go.GetComponentInChildren<TextMeshPro>(true);
        return true;
    }

    /// <summary>Clone source for one notification line - same prefab Mira uses for CreateAndShowNotification.</summary>
    private static bool TryEnsureRowTemplate()
    {
        if (IsAlive(_rowTemplate)) return true;
        if (!HudManager.InstanceExists || HudManager.Instance == null) return false;

        var notifier = HudManager.Instance.Notifier;
        if (notifier == null) return false;

        var origin = notifier.notificationMessageOrigin;
        if (origin == null) return false;

        _rowTemplate = origin.gameObject;
        return IsAlive(_rowTemplate);
    }

    /// <summary>
    /// Disable animator + the prefab's own self-destroying MonoBehaviour on the
    /// clone so the row stays on-screen instead of sliding off and despawning.
    /// </summary>
    private static void StopAutoDispose(GameObject go)
    {
        foreach (var a in go.GetComponentsInChildren<Animator>(true))
        {
            if (a != null) a.enabled = false;
        }

        // The cloned prefab carries a LobbyNotificationMessage component whose
        // own Update slides the row off-screen and disposes it. We only want
        // the visuals, not the lifecycle, so disable any MonoBehaviour on the
        // clone whose name suggests notification logic.
        foreach (var mb in go.GetComponents<MonoBehaviour>())
        {
            if (mb == null) continue;
            var typeName = mb.GetType().Name;
            if (typeName.IndexOf("Notification", StringComparison.OrdinalIgnoreCase) >= 0 ||
                typeName.IndexOf("Popper", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                mb.enabled = false;
            }
        }
    }

    private static void ApplyIcon(GameObject go, Sprite icon)
    {
        var t = go.transform.Find("Icon");
        if (t == null) t = go.transform.Find("icon");
        if (t != null)
        {
            var sr = t.GetComponent<SpriteRenderer>();
            if (sr != null) sr.sprite = icon;
            var img = t.GetComponent<Image>();
            if (img != null) img.sprite = icon;
            return;
        }

        foreach (var img in go.GetComponentsInChildren<Image>(true))
        {
            if (img == null) continue;
            if (img.gameObject.name.IndexOf("Icon", StringComparison.OrdinalIgnoreCase) < 0) continue;
            img.sprite = icon;
            return;
        }

        foreach (var r in go.GetComponentsInChildren<SpriteRenderer>(true))
        {
            if (r == null || r.gameObject == go) continue;
            if (r.gameObject.name.IndexOf("Icon", StringComparison.OrdinalIgnoreCase) < 0) continue;
            r.sprite = icon;
            return;
        }
    }

    private static void ApplyText(Entry e)
    {
        if (!IsAlive(e.Tmp)) return;
        if (e.IsCountingDown && e.SecondsRemaining.HasValue)
        {
            var s = Mathf.Max(0, Mathf.CeilToInt(e.SecondsRemaining.Value));
            e.Tmp!.text = $"{e.TitleRich}  <color=#FFAA33>{s}s</color>";
        }
        else
        {
            e.Tmp!.text = e.TitleRich;
        }
    }

    private static void SortEntries()
    {
        Entries.Sort((a, b) =>
        {
            var byPriority = a.Priority.CompareTo(b.Priority);
            return byPriority != 0 ? byPriority : a.InsertionOrder.CompareTo(b.InsertionOrder);
        });
    }

    private static void Relayout()
    {
        for (var i = 0; i < Entries.Count; i++)
        {
            var go = Entries[i].Go;
            if (!IsAlive(go)) continue;
            go!.transform.localPosition = new Vector3(0f, -i * RowStep, 0f);
        }
    }

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    public static class DivaniTimersHudUpdate
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            // Tick already swallows all exceptions internally.
            Tick();
        }
    }

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
    public static class DivaniTimersGameEnd
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Clear();
        }
    }

    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.CoBegin))]
    public static class DivaniTimersIntro
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Clear();
        }
    }
}
