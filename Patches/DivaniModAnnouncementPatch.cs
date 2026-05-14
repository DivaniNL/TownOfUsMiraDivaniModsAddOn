using System.Collections.Immutable;
using System.Globalization;
using System.Reflection;
using System.Text.Json;
using AmongUs.Data;
using AmongUs.Data.Player;
using Assets.InnerNet;
using DivaniMods.Assets;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;

namespace DivaniMods.Patches;

/// <summary>
/// In-game news: merge embedded <c>modNews-en_US.json</c> into the announcements list and show a
/// badge on announcement panels, following the same pattern as
/// <see href="https://github.com/Mehzxzz/TownOfExtra/blob/master/TownOfExtra/Patches/AnnouncementPatch.cs">TownOfExtra</see>
/// and Town Of Us <c>AnnouncementPatch</c>.
/// </summary>
public sealed class DivaniModNewsEntry(int number, string title, string subTitle, string shortTitle, string text, string date)
{
    public int Number { get; } = number;
    public string Title { get; } = title;
    public string SubTitle { get; } = subTitle;
    public string ShortTitle { get; } = shortTitle;
    public string Text { get; } = text;
    public string Date { get; } = date;

    public Announcement ToAnnouncement()
    {
        return new Announcement
        {
            Date = Date,
            Number = Number,
            ShortTitle = ShortTitle,
            SubTitle = SubTitle,
            Title = Title,
            Text = Text,
            Language = (uint)DataManager.Settings.Language.CurrentLanguage,
            Id = "DivaniMods",
        };
    }
}

[HarmonyPatch]
public static class DivaniModAnnouncementPatch
{
    private const string EmbeddedNewsResource = "DivaniMods.Resources.Announcements.modNews-en_US.json";

    /// <summary>TownOfExtra-style band: mod news <c>Number</c> values used for the corner badge.</summary>
    private const int ModNewsNumberMin = 10000;
    private const int ModNewsNumberMax = 100000;

    private static bool _parsed;
    public static ImmutableList<DivaniModNewsEntry> AllModNews { get; private set; } = ImmutableList<DivaniModNewsEntry>.Empty;

    /// <summary>Load once from the DLL embedded resource (same idea as TownOfExtra <c>LoadFromResources</c>).</summary>
    public static void EnsureLoaded()
    {
        if (_parsed)
        {
            return;
        }

        _parsed = true;

        try
        {
            var asm = typeof(DivaniPlugin).Assembly;
            using var stream = asm.GetManifestResourceStream(EmbeddedNewsResource);
            if (stream == null)
            {
                DivaniPlugin.Instance.Log.LogWarning(
                    $"Divani mod news: embedded resource not found ({EmbeddedNewsResource}).");
                return;
            }

            using var reader = new StreamReader(stream);
            ParseJson(reader.ReadToEnd());
            DivaniPlugin.Instance.Log.LogInfo($"Divani mod news: loaded {AllModNews.Count} entr(y/ies).");
        }
        catch (Exception ex)
        {
            DivaniPlugin.Instance.Log.LogWarning($"Divani mod news: failed to load — {ex.Message}");
        }
    }

    private static void ParseJson(string json)
    {
        using var doc = JsonDocument.Parse(json);
        foreach (var item in doc.RootElement.GetProperty("News").EnumerateArray())
        {
            var date = item.GetProperty("Date").GetString() ?? "Unknown Date";
            var numberStr = item.GetProperty("Number").GetString();
            var number = numberStr != null ? int.Parse(numberStr, CultureInfo.InvariantCulture) : 0;
            var shortTitle = item.GetProperty("ShortTitle").GetString() ?? "";
            var subTitle = item.GetProperty("SubTitle").GetString() ?? "";
            var title = item.GetProperty("Title").GetString() ?? "";
            // Join without spaces: fragments already use \n / \n\n; a space here would inject " \n" and break list lines.
            var text = string.Join(
                string.Empty,
                item.GetProperty("Text").EnumerateArray().Select(static e => e.GetString() ?? string.Empty));

            AllModNews = AllModNews.Add(new DivaniModNewsEntry(number, title, subTitle, shortTitle, text, date));
        }
    }

    /// <summary>
    /// Run after Town Of Us (priority 0) so <paramref name="aRange"/> already includes TOU entries; we merge Divani news
    /// and de-duplicate by <c>Number</c> like TownOfExtra.
    /// </summary>
    [HarmonyPatch(typeof(PlayerAnnouncementData), nameof(PlayerAnnouncementData.SetAnnouncements))]
    [HarmonyPrefix]
    [HarmonyPriority(100)]
    public static void SetAnnouncements_Prefix(ref Il2CppReferenceArray<Announcement> aRange)
    {
        EnsureLoaded();
        if (AllModNews.IsEmpty)
        {
            return;
        }

        var aArray = aRange.ToArray();
        var combined = AllModNews.Select(static n => n.ToAnnouncement()).ToList();
        combined.AddRange(aArray.Where(a => AllModNews.All(x => x.Number != a.Number)));
        combined.Sort(static (a1, a2) => DateTime.Compare(
            DateTime.Parse(a2.Date, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
            DateTime.Parse(a1.Date, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind)));

        var newArray = new Announcement[combined.Count];
        for (var i = 0; i < combined.Count; i++)
        {
            newArray[i] = combined[i];
        }

        aRange = newArray;
    }

    /// <summary>TownOfExtra-style logo on the announcement card for Divani news IDs.</summary>
    [HarmonyPatch(typeof(AnnouncementPanel), nameof(AnnouncementPanel.SetUp))]
    [HarmonyPostfix]
    public static void AnnouncementPanel_SetUp_Postfix(
        AnnouncementPanel __instance,
        [HarmonyArgument(0)] Announcement announcement)
    {
        if (announcement.Number < ModNewsNumberMin || announcement.Number >= ModNewsNumberMax)
        {
            return;
        }

        var obj = new GameObject("DivaniModNewsLabel");
        obj.transform.SetParent(__instance.transform);
        obj.transform.localPosition = new Vector3(-0.8f, 0.13f, 0.5f);
        // Slightly under TOU’s 0.9; combined with ModNewsLogo PPU (~220) sizes between “no PPU” and “very high PPU”.
        obj.transform.localScale = new Vector3(0.78f, 0.78f, 0.78f);
        var renderer = obj.AddComponent<SpriteRenderer>();
        renderer.sprite = DivaniAssets.ModNewsLogo.LoadAsset();
        renderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
    }
}
