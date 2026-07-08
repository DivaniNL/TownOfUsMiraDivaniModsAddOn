using System.Collections.Generic;
using MiraAPI.GameOptions;
using DivaniMods.Options;
using DivaniMods.Roles.Crewmate.CrewmateProtective;
using TownOfUs.Assets;
using TownOfUs.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DivaniMods.Buttons.Crewmate.CrewmateProtective;

public static class DomeManager
{
    public sealed class Dome
    {
        public byte OwnerId { get; init; }
        public Vector3 Position { get; init; }
        public GameObject? GameObject { get; set; }
        public float BaseDiameter { get; init; }
        public float SpawnedAt { get; init; }
        public float ExpiresAt { get; set; }
    }

    private static readonly List<Dome> _domes = new();
    public static IReadOnlyList<Dome> Domes => _domes;

    public static void PlaceDome(byte ownerId, Vector3 position)
    {
        if (ShipStatus.Instance == null)
        {
            return;
        }

        var opts = OptionGroupSingleton<DomesmithOptions>.Instance;
        var diameter = opts.DomeSize.Value * ShipStatus.Instance.MaxLightRadius * 2f;

        var pos = position;
        pos.z += 0.001f;

        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = $"DomesmithDome_{ownerId}_{_domes.Count}";
        go.transform.localScale = new Vector3(diameter, diameter, diameter);
        var sphereCollider = go.GetComponent("SphereCollider");
        if (sphereCollider != null)
        {
            Object.Destroy(sphereCollider);
        }

        var renderer = go.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.material = AuAvengersAnims.TrapMaterial.LoadAsset();

            var mat = renderer.material;
            var tint = DomesmithRole.DomesmithColor;
            if (mat.HasProperty("_BackgroundColor"))
            {
                var orig = mat.GetColor("_BackgroundColor");
                mat.SetColor("_BackgroundColor", new Color(tint.r, tint.g, tint.b, orig.a));
            }
            if (mat.HasProperty("_OutlineColor"))
            {
                var orig = mat.GetColor("_OutlineColor");
                mat.SetColor("_OutlineColor", new Color(tint.r, tint.g, tint.b, orig.a));
            }
        }

        go.transform.position = pos;

        var now = Time.time;
        var dome = new Dome
        {
            OwnerId = ownerId,
            Position = pos,
            GameObject = go,
            BaseDiameter = diameter,
            SpawnedAt = now,
            ExpiresAt = now + opts.ActiveSeconds.Value,
        };
        _domes.Add(dome);

        ApplyVisibility(dome);
        PlaceDomeButton.SyncDomeTimerFromManager();
    }

    public static void Tick()
    {
        if (_domes.Count == 0)
        {
            PlaceDomeButton.SyncDomeTimerFromManager();
            return;
        }

        var now = Time.time;
        for (var i = _domes.Count - 1; i >= 0; i--)
        {
            var dome = _domes[i];
            if (now >= dome.ExpiresAt)
            {
                Destroy(dome);
                _domes.RemoveAt(i);
            }
        }

        PlaceDomeButton.SyncDomeTimerFromManager();
    }

    public static float GetLongestRemainingSeconds(byte ownerId)
    {
        var now = Time.time;
        var longest = 0f;
        foreach (var dome in _domes)
        {
            if (dome.OwnerId != ownerId || dome.ExpiresAt >= float.MaxValue)
            {
                continue;
            }

            longest = Mathf.Max(longest, dome.ExpiresAt - now);
        }

        return longest;
    }

    public static Dome? FindContaining(Vector2 position)
    {
        if (_domes.Count == 0 || ShipStatus.Instance == null)
        {
            return null;
        }

        foreach (var dome in _domes)
        {
            if (dome.GameObject == null)
            {
                continue;
            }

            var radius = GetCurrentRadius(dome);
            if (radius <= 0f)
            {
                continue;
            }

            var center = new Vector2(dome.Position.x, dome.Position.y);
            if (Vector2.Distance(center, position) <= radius)
            {
                return dome;
            }
        }

        return null;
    }

    public static float GetCurrentRadius(Dome dome)
    {
        if (dome.GameObject == null || dome.BaseDiameter <= 0f)
        {
            return 0f;
        }

        return dome.BaseDiameter * 0.5f;
    }

    public static void RefreshVisibility()
    {
        foreach (var dome in _domes)
        {
            ApplyVisibility(dome);
        }
    }

    public static void Clear()
    {
        foreach (var dome in _domes)
        {
            Destroy(dome);
        }

        _domes.Clear();
        PlaceDomeButton.SyncDomeTimerFromManager();
    }

    public static void SetVisibleAll(bool visible)
    {
        foreach (var dome in _domes)
        {
            if (dome.GameObject == null)
            {
                continue;
            }

            var renderer = dome.GameObject.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.enabled = visible && LocalShouldSee(dome);
            }
        }
    }

    private static void Destroy(Dome dome)
    {
        if (dome.GameObject != null)
        {
            Object.Destroy(dome.GameObject);
            dome.GameObject = null;
        }
    }

    private static void ApplyVisibility(Dome dome)
    {
        if (dome.GameObject == null)
        {
            return;
        }

        var renderer = dome.GameObject.GetComponent<MeshRenderer>();
        if (renderer == null)
        {
            return;
        }

        renderer.enabled = LocalShouldSee(dome);
    }

    private static bool LocalShouldSee(Dome dome)
    {
        var localPlayer = PlayerControl.LocalPlayer;
        if (localPlayer == null || localPlayer.Data == null)
        {
            return false;
        }

        if (localPlayer.Data.IsDead)
        {
            return true;
        }

        var mode = (DomesmithVisibility)OptionGroupSingleton<DomesmithOptions>.Instance.SeenBy.Value;
        return mode switch
        {
            DomesmithVisibility.Everyone => true,
            DomesmithVisibility.NonImpostor => !localPlayer.Data.Role.IsImpostor(),
            DomesmithVisibility.Crewmates => localPlayer.IsCrewmate() || localPlayer.IsNeutral(),
            _ => localPlayer.Data.Role is DomesmithRole || localPlayer.PlayerId == dome.OwnerId,
        };
    }
}
