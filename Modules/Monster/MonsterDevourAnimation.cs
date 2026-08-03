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

    public static bool ShouldBeHidden(byte victimId) => HidingVictims.Contains(victimId);

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
        var targetOnLeft = victimPos.x < monsterPos.x;

        var cosmetics = monster.cosmetics;
        if (cosmetics != null && cosmetics.gameObject != null)
        {
            cosmetics.gameObject.SetActive(false);
        }

        var go = new GameObject("MonsterDevourAnim");
        go.transform.SetParent(monster.transform, false);

        const float animScale = 2.0f;
        var scaleX = targetOnLeft ? animScale : -animScale;
        go.transform.localScale = new Vector3(scaleX, animScale, animScale);

        const float xOffset = 0.6f;
        var posX = targetOnLeft ? -xOffset : xOffset;
        go.transform.localPosition = new Vector3(posX, 0.1f, -0.1f);

        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.material = HatManager.Instance.PlayerMaterial;
        monster.SetPlayerMaterialColors(renderer);

        var visorProp = Shader.PropertyToID("_VisorColor");
        var glintProp = Shader.PropertyToID("_VisorGlint");

        var bodyMat = monster.cosmetics?.currentBodySprite?.BodySprite?.material;
        if (bodyMat != null)
        {
            if (bodyMat.HasProperty(visorProp))
            {
                renderer.material.SetColor(visorProp, bodyMat.GetColor(visorProp));
            }
            if (bodyMat.HasProperty(glintProp))
            {
                renderer.material.SetColor(glintProp, bodyMat.GetColor(glintProp));
            }
        }
        else
        {
            renderer.material.SetColor(visorProp, Palette.VisorColor);
        }

        renderer.sortingOrder = 500;

        if (monster.MyPhysics != null)
        {
            monster.MyPhysics.FlipX = targetOnLeft;
        }

        var anim = go.AddComponent<SpriteAnim>();

        Coroutines.Start(CoPlayAnim(go, anim, clip, Mathf.Clamp01(hideVictimAtFraction), victim, cosmetics, onComplete));
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
            if (go == null) yield break;

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

        onComplete?.Invoke();
    }
}
