using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using DivaniMods.Options;
using TownOfUs.Assets;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Modules;
using TownOfUs.Utilities;
using TownOfUs.Utilities.Appearances;
using UnityEngine;

namespace DivaniMods.Modifiers;

/// <summary>
/// Temporary trail on the killer after slaying a crewmate with Bloody. Uses Investigator-style spacing;
/// footprints are impostor red for all players.
/// </summary>
public sealed class BloodyKillerFootstepsModifier : BaseModifier
{
    public Dictionary<GameObject, SpriteRenderer>? CurrentSteps;
    public bool PrintOnVents;
    public float PrintSize;
    public float PrintDuration;
    public float PrintInterval;
    public bool CheckDistance;
    private Vector3 _lastPos;
    private float _footstepInterval;
    private float _effectEndsAt;

    public override string ModifierName => "Bloody Footsteps";
    public override bool HideOnUi => true;

    public override void OnActivate()
    {
        CurrentSteps = [];
        var opts = OptionGroupSingleton<BloodyOptions>.Instance;
        PrintSize = opts.FootprintSize;
        PrintInterval = opts.FootprintInterval;
        CheckDistance = (BloodyPrintMode)opts.FootprintMode.Value is BloodyPrintMode.Distance;
        PrintDuration = opts.SingleFootprintFadeSeconds.Value;
        PrintOnVents = opts.ShowFootprintVent;
        _effectEndsAt = Time.time + opts.KillerTrailDurationSeconds.Value;
        _lastPos = Player.transform.position;
        _footstepInterval = 0f;
    }

    public override void OnDeactivate()
    {
        if (CurrentSteps == null)
        {
            return;
        }

        foreach (var step in CurrentSteps.ToList())
        {
            Coroutines.Start(FootstepFadeout(step.Key, step.Value));
        }

        CurrentSteps.Clear();
    }

    public override void FixedUpdate()
    {
        if (CurrentSteps == null || Player == null || Player.Data == null || Player.Data.IsDead)
        {
            return;
        }

        if (MeetingHud.Instance != null || Time.time >= _effectEndsAt)
        {
            ModifierComponent?.RemoveModifier(this);
            return;
        }

        var cantContinue = PlayerControl.LocalPlayer.GetModifiers<HypnotisedModifier>().Any(x => x.HysteriaActive) ||
                           !Player.IsVisibleToOthers();

        var footprintColor = Palette.ImpostorRed;

        if (CheckDistance)
        {
            if (cantContinue ||
                Vector3.Distance(_lastPos, Player.transform.position) < PrintInterval)
            {
                return;
            }

            if (!PrintOnVents && ShipStatus.Instance?.AllVents
                    .Any(vent => Vector2.Distance(vent.gameObject.transform.position, Player.GetTruePosition()) < 1f) ==
                true)
            {
                return;
            }

            var angle = Mathf.Atan2(Player.MyPhysics.Velocity.y, Player.MyPhysics.Velocity.x) * Mathf.Rad2Deg;

            var footstep = new GameObject("BloodyFootstep")
            {
                transform =
                {
                    parent = ShipStatus.Instance?.transform,
                    position = new Vector3(Player.transform.position.x, Player.transform.position.y, 2.5708f),
                    rotation = Quaternion.AngleAxis(angle - 90, Vector3.forward)
                }
            };

            if (ModCompatibility.IsSubmerged())
            {
                footstep.AddSubmergedComponent("ElevatorMover");
            }

            var sprite = footstep.AddComponent<SpriteRenderer>();
            sprite.sprite = TouAssets.FootprintSprite.LoadAsset();
            sprite.color = footprintColor;
            footstep.layer = LayerMask.NameToLayer("Players");

            footstep.transform.localScale *= new Vector2(1.2f, 1f) * (PrintSize / 10);

            CurrentSteps!.Add(footstep, sprite);
            _lastPos = Player.transform.position;
            Coroutines.Start(FootstepDisappear(footstep, sprite, PrintDuration));
        }
        else
        {
            if (cantContinue || _footstepInterval < PrintInterval)
            {
                _footstepInterval += Time.fixedDeltaTime;
                return;
            }

            if (!PrintOnVents && ShipStatus.Instance?.AllVents
                    .Any(vent => Vector2.Distance(vent.gameObject.transform.position, Player.GetTruePosition()) < 1f) ==
                true)
            {
                return;
            }

            var angle = Mathf.Atan2(Player.MyPhysics.Velocity.y, Player.MyPhysics.Velocity.x) * Mathf.Rad2Deg;

            var footstep = new GameObject("BloodyFootstep")
            {
                transform =
                {
                    parent = ShipStatus.Instance?.transform,
                    position = Player.transform.position,
                    rotation = Quaternion.AngleAxis(angle - 90, Vector3.forward)
                }
            };

            if (ModCompatibility.IsSubmerged())
            {
                footstep.AddSubmergedComponent("ElevatorMover");
            }

            var sprite = footstep.AddComponent<SpriteRenderer>();
            sprite.sprite = TouAssets.FootprintSprite.LoadAsset();
            sprite.color = footprintColor;
            footstep.layer = LayerMask.NameToLayer("Players");

            footstep.transform.localScale *= new Vector2(1.2f, 1f) * (PrintSize / 10);

            CurrentSteps!.Add(footstep, sprite);
            Coroutines.Start(FootstepDisappear(footstep, sprite, PrintDuration));

            _footstepInterval = 0;
        }
    }

    public override void OnDeath(DeathReason reason)
    {
        ModifierComponent?.RemoveModifier(this);
    }

    private static IEnumerator FootstepFadeout(GameObject obj, SpriteRenderer rend)
    {
        yield return MiscUtils.FadeOut(rend, 0.0001f, 0.05f);
        obj.DestroyImmediate();
    }

    private static IEnumerator FootstepDisappear(GameObject obj, SpriteRenderer rend, float duration)
    {
        yield return new WaitForSeconds(duration);
        yield return FootstepFadeout(obj, rend);
    }
}
