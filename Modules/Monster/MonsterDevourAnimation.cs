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

    // 1 = the clip's own speed. Lower is slower (0.85 is ~15% slower).
    private const float AnimSpeed = 0.85f;

    // Walk-up before the devour: the monster walks to its spot beside the victim instead of teleporting.
    private const float WalkSpeed = 3f;
    private const float MaxWalkDistance = 4f;

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

        try
        {
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

            // The victim is frozen on the spot for the whole walk-up and devour.
            if (victim != null)
            {
                victim.moveable = false;
                victim.MyPhysics.body.velocity = Vector2.zero;
                victim.NetTransform.SnapTo(victimPos);
            }

            monster.moveable = false;
            monster.MyPhysics.body.velocity = Vector2.zero;

            // Walk if there is a clear, short path; otherwise fall back to the old instant snap.
            var monster2D = (Vector2)monsterPos;
            var walk = Vector2.Distance(monster2D, snapPos) <= MaxWalkDistance && IsClear(monster2D, snapPos);

            Coroutines.Start(CoWalkThenPlay(monster, victim, snapPos, walk, targetOnLeft, clip, Mathf.Clamp01(hideVictimAtFraction), onComplete));
        }
        catch (Exception e)
        {
            Debug.LogError($"[Monster] Devour setup failed, skipping the animation: {e}");
            Abort(monster, victim, null, onComplete);
        }
    }

    public static void Play(PlayerControl monster, PlayerControl victim, float hideVictimAtFraction, Action? onComplete)
    {
        Play(monster, victim, null, hideVictimAtFraction, onComplete);
    }

    // Used whenever the animation can't run: nobody is left frozen, and the victim is still devoured.
    private static void Abort(PlayerControl? monster, PlayerControl? victim, CosmeticsLayer? monsterCosmetics, Action? onComplete)
    {
        try
        {
            if (monsterCosmetics != null && monsterCosmetics.gameObject != null)
            {
                monsterCosmetics.gameObject.SetActive(true);
            }

            if (monster != null)
            {
                monster.MyPhysics.body.velocity = Vector2.zero;
                monster.moveable = true;
            }

            if (victim != null)
            {
                victim.Visible = false;
                HidingVictims.Add(victim.PlayerId);
                victim.moveable = true;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[Monster] Devour abort cleanup failed: {e}");
        }

        onComplete?.Invoke();
    }

    // One step of the walk. Returns false if something went wrong, so the caller can bail out to the snap.
    private static bool StepWalk(PlayerControl monster, Vector2 position, Vector2 direction)
    {
        try
        {
            var z = monster.transform.position.z;
            monster.transform.position = new Vector3(position.x, position.y, z);

            var body = monster.MyPhysics.body;
            body.position = position;

            // Non-zero velocity is what makes the game play the run animation and face the walking direction.
            body.velocity = direction * WalkSpeed;
            if (Mathf.Abs(direction.x) > 0.01f)
            {
                monster.MyPhysics.FlipX = direction.x < 0f;
            }

            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[Monster] Devour walk step failed: {e}");
            return false;
        }
    }

    private static IEnumerator CoWalkThenPlay(
        PlayerControl monster,
        PlayerControl? victim,
        Vector2 snapPos,
        bool walk,
        bool targetOnLeft,
        AnimationClip clip,
        float hideVictimFraction,
        Action? onComplete)
    {
        if (walk && monster != null)
        {
            // Every client walks the monster along the same straight line, so it doesn't depend on
            // movement sync (or on the monster being able to move) to look right.
            var start = (Vector2)monster.transform.position;
            var distance = Vector2.Distance(start, snapPos);
            var direction = distance > 0.001f ? (snapPos - start) / distance : Vector2.zero;
            var duration = Mathf.Max(distance / WalkSpeed, 0.01f);
            var elapsed = 0f;

            while (elapsed < duration)
            {
                if (monster == null)
                {
                    break;
                }

                elapsed += Time.deltaTime;
                var position = Vector2.Lerp(start, snapPos, Mathf.Clamp01(elapsed / duration));
                if (!StepWalk(monster, position, direction))
                {
                    break;
                }

                yield return null;
            }
        }

        if (monster == null)
        {
            Abort(null, victim, null, onComplete);
            yield break;
        }

        // Land exactly on the spot (a no-op after a clean walk, the old behaviour when walking isn't possible).
        try
        {
            monster.MyPhysics.body.velocity = Vector2.zero;
            monster.NetTransform.SnapTo(snapPos);
        }
        catch (Exception e)
        {
            Debug.LogError($"[Monster] Devour snap failed: {e}");
        }

        if (!TryStartDevour(monster, victim, targetOnLeft, clip, hideVictimFraction, onComplete))
        {
            Abort(monster, victim, monster.cosmetics, onComplete);
        }
    }

    // Builds the devour sprite and starts the clip. No yields in here, so it can be wrapped in a try/catch.
    private static bool TryStartDevour(
        PlayerControl monster,
        PlayerControl? victim,
        bool targetOnLeft,
        AnimationClip clip,
        float hideVictimFraction,
        Action? onComplete)
    {
        try
        {
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
            anim.Speed = AnimSpeed;

            Coroutines.Start(CoPlayAnim(go, anim, clip, hideVictimFraction, monster, victim, cosmetics, onComplete));
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[Monster] Could not start the devour animation: {e}");
            return false;
        }
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

        // Slowed clips take proportionally longer, so the hide point and the end both follow the speed.
        var duration = Mathf.Max(clip.length / AnimSpeed, 0.1f);
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
