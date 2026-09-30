using System.Collections;
using DivaniMods.Assets;
using PowerTools;
using Reactor.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DivaniMods.Modules.Monster;

public sealed class MonsterDevourAnimation
{
    private static readonly HashSet<byte> HidingVictims = [];

    private static readonly string[] ColorProps = ["_BackColor", "_BodyColor", "_VisorColor", "_OutlineColor", "_AddColor"];

    private static bool IsClear(Vector2 from, Vector2 to) => !PhysicsHelpers.AnythingBetween(from, to, Constants.ShipAndObjectsMask, false);

    public static bool ShouldBeHidden(byte victimId) => HidingVictims.Contains(victimId);

    public static void ClearHidingVictims() => HidingVictims.Clear();

    public static void RemoveHidingVictim(byte victimId) => HidingVictims.Remove(victimId);

    public static void Play(PlayerControl monster, PlayerControl victim, Vector3? victimPosBeforeSnap = null, float hideVictimAtFraction = 0.5f, Action? onComplete = null)
    {
        if (monster == null)
        {
            onComplete?.Invoke();
            return;
        }

        var animClipAsset = DivaniAssets.MonsterDevourAnim;
        var clip = animClipAsset?.LoadAsset();
        if (clip == null)
        {
            if (victim != null)
            {
                victim.Visible = false;
                HidingVictims.Add(victim.PlayerId);
            }
            onComplete?.Invoke();
            return;
        }

        var victimPos = victimPosBeforeSnap ?? (victim != null ? victim.transform.position : monster.transform.position);
        var monsterPos = monster.transform.position;
        var delta = (Vector2)(victimPos - monsterPos);

        const float horizontalDeadzone = 0.2f;
        var wasFlipped = monster.MyPhysics != null && monster.MyPhysics.FlipX;
        var targetOnLeft = Mathf.Abs(delta.x) < horizontalDeadzone ? wasFlipped : delta.x < 0f;

        const float sideDistance = 0.9f;
        const float clearance = 0.3f;
        var victim2D = (Vector2)victimPos;
        var snapPos = victim2D;
        var preferredDir = targetOnLeft ? 1f : -1f;
        if (IsClear(victim2D, victim2D + new Vector2(preferredDir * (sideDistance + clearance), 0f)))
        {
            snapPos = victim2D + new Vector2(preferredDir * sideDistance, 0f);
        }
        else if (IsClear(victim2D, victim2D + new Vector2(-preferredDir * (sideDistance + clearance), 0f)))
        {
            snapPos = victim2D + new Vector2(-preferredDir * sideDistance, 0f);
            targetOnLeft = !targetOnLeft;
        }

        if (victim != null)
        {
            victim.moveable = false;
            victim.MyPhysics.body.velocity = Vector2.zero;
            victim.NetTransform.SnapTo(victimPos);
        }

        monster.moveable = false;
        monster.MyPhysics.body.velocity = Vector2.zero;
        monster.NetTransform.SnapTo(snapPos);

        var cosmetics = monster.cosmetics;
        if (cosmetics != null && cosmetics.gameObject != null)
        {
            cosmetics.gameObject.SetActive(false);
        }

        var go = new GameObject("MonsterDevourAnim");
        go.transform.SetParent(monster.transform, false);

        const float animScale = 1.3f;
        go.transform.localScale = new Vector3(targetOnLeft ? animScale : -animScale, animScale, animScale);

        const float xOffset = 0.35f;
        const float yOffset = 0.14f;
        var posX = targetOnLeft ? -xOffset : xOffset;
        var posY = yOffset;
        go.transform.localPosition = new Vector3(posX, posY, -0.1f);

        var bodyRenderer = monster.cosmetics?.currentBodySprite?.BodySprite;
        var bodyMat = bodyRenderer?.material;

        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.material = new Material(HatManager.Instance.PlayerMaterial);
        monster.SetPlayerMaterialColors(renderer);

        if (bodyRenderer != null)
        {
            renderer.sortingLayerID = bodyRenderer.sortingLayerID;
        }

        foreach (var name in ColorProps)
        {
            var id = Shader.PropertyToID(name);
            if (bodyMat != null && bodyMat.HasProperty(id) && renderer.material.HasProperty(id))
            {
                renderer.material.SetColor(id, bodyMat.GetColor(id));
            }
        }

        var visorProp = Shader.PropertyToID("_VisorColor");
        var visor = bodyMat != null && bodyMat.HasProperty(visorProp) ? bodyMat.GetColor(visorProp) : (Color)Palette.VisorColor;
        visor.a = 1f;
        renderer.material.SetColor(visorProp, visor);

        renderer.sortingOrder = 500;

        if (monster.MyPhysics != null)
        {
            monster.MyPhysics.FlipX = targetOnLeft;
        }

        var anim = go.AddComponent<SpriteAnim>();

        Coroutines.Start(CoPlayAnim(go, anim, clip, Mathf.Clamp01(hideVictimAtFraction), monster, victim, cosmetics, onComplete));
    }

    public static void Play(PlayerControl monster, PlayerControl victim, float hideVictimAtFraction, Action? onComplete)
    {
        Play(monster, victim, null, hideVictimAtFraction, onComplete);
    }

    private static IEnumerator CoPlayAnim(
        GameObject go,
        SpriteAnim anim,
        AnimationClip clip,
        float hideVictimFraction,
        PlayerControl monster,
        PlayerControl? victim,
        CosmeticsLayer? monsterCosmetics,
        Action? onComplete)
    {
        anim.Play(clip);

        var duration = Mathf.Max(clip.length, 0.1f);
        var hideTime = duration * hideVictimFraction;
        var elapsed = 0f;
        var victimHidden = false;

        while (elapsed < duration)
        {
            if (go == null)
            {
                if (monster != null) monster.moveable = true;
                if (victim != null) victim.moveable = true;
                yield break;
            }

            if (!victimHidden && victim != null && elapsed >= hideTime)
            {
                victim.Visible = false;
                HidingVictims.Add(victim.PlayerId);
                victimHidden = true;
            }

            yield return null;
            elapsed += Time.deltaTime;
        }

        if (victim != null)
        {
            victim.Visible = false;
            HidingVictims.Add(victim.PlayerId);
        }

        if (monsterCosmetics != null && monsterCosmetics.gameObject != null)
        {
            monsterCosmetics.gameObject.SetActive(true);
        }

        if (go != null)
        {
            Object.Destroy(go);
        }

        if (monster != null)
        {
            monster.moveable = true;
        }

        if (victim != null)
        {
            victim.moveable = true;
        }

        onComplete?.Invoke();
    }
}