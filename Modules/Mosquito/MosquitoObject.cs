using System;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using MiraAPI.Hud;
using MiraAPI.Networking;
using Reactor.Utilities.Attributes;
using DivaniMods.Assets;
using DivaniMods.Buttons.Impostor.ImpostorPower;
using DivaniMods.Networking.Impostor.ImpostorPower;
using TownOfUs.Networking;
using TownOfUs.Utilities;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace DivaniMods.Modules.Mosquito;

[RegisterInIl2Cpp]
public sealed class MosquitoObject : MonoBehaviour
{
    public MosquitoObject(IntPtr cppPtr) : base(cppPtr)
    {
    }

    private const float ContactRadius = 0.45f;
    private const float MaxLifetime = 20f;
    private const float BeanBaseSpeed = 2.5f;
    private const float SpeedMultiplier = 1f;

    public byte ShooterId { get; set; } = byte.MaxValue;
    public byte TargetId { get; set; } = byte.MaxValue;
    public Vector2 Destination { get; set; }
    public bool Aimbot { get; set; }

    private SpriteRenderer? _rend;
    private float _speed;
    private float _life;
    private bool _stung;
    private bool _swatted;
    private bool _swatSoundPlayed;

    [HideFromIl2Cpp]
    public void Configure(byte shooterId, byte targetId, Vector2 destination, bool aimbot)
    {
        ShooterId = shooterId;
        TargetId = targetId;
        Destination = destination;
        Aimbot = aimbot;
    }

    private void Start()
    {
        _rend = gameObject.GetComponent<SpriteRenderer>();
        if (_rend == null)
        {
            _rend = gameObject.AddComponent<SpriteRenderer>();
        }

        _rend.sprite = DivaniAssets.MosquitoIcon.LoadAsset();
        transform.localScale = new Vector3(0.5f, 0.5f, 1f);

        var local = PlayerControl.LocalPlayer;
        if (local != null && (local.PlayerId == TargetId || local.PlayerId == ShooterId))
        {
            var hud = HudManager.Instance;
            if (hud != null && hud.ShadowQuad != null)
            {
                _rend.sortingLayerID = hud.ShadowQuad.sortingLayerID;
                _rend.sortingOrder = hud.ShadowQuad.sortingOrder + 50;
            }
            else
            {
                _rend.sortingOrder = 1000;
            }
        }

        SetupClickable();

        var speedMod = 1f;
        if (GameOptionsManager.Instance != null)
        {
            speedMod = GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.PlayerSpeedMod);
        }

        _speed = speedMod * BeanBaseSpeed * SpeedMultiplier;
    }

    private void Update()
    {
        if (!ShipStatus.Instance || MeetingHud.Instance)
        {
            Destroy(gameObject);
            return;
        }

        var target = GetPlayer(TargetId);
        if (target == null || target.Data == null || target.Data.Disconnected || target.Data.IsDead)
        {
            Destroy(gameObject);
            return;
        }

        var owner = PlayerControl.LocalPlayer;
        if (owner != null && owner.PlayerId == ShooterId)
        {
            owner.SetKillTimer(owner.GetKillCooldown());

            var sting = CustomButtonSingleton<MosquitoStingButton>.Instance;
            if (sting != null && sting.Timer < sting.Cooldown)
            {
                sting.Timer = sting.Cooldown;
            }
        }

        var targetPos = target.GetTruePosition();
        var dest = Aimbot ? targetPos : Destination;
        var pos = (Vector2)transform.position;
        var delta = dest - pos;
        var step = _speed * Time.deltaTime;
        var next = delta.magnitude <= step ? dest : pos + (delta.normalized * step);

        transform.position = new Vector3(next.x, next.y, (next.y / 1000f) - 0.5f);

        if (_rend != null && Mathf.Abs(delta.x) > 0.001f)
        {
            _rend.flipX = delta.x > 0f;
        }

        if (!_stung && Vector2.Distance(next, targetPos) <= ContactRadius)
        {
            _stung = true;

            var local = PlayerControl.LocalPlayer;
            if (local != null && local.PlayerId == ShooterId)
            {
                var shooter = GetPlayer(ShooterId);
                if (shooter != null && !shooter.HasDied() && !target.HasDied())
                {
                    shooter.RpcSpecialMurder(
                        target,
                        MeetingCheck.OutsideMeeting,
                        isIndirect: true,
                        teleportMurderer: false,
                        showKillAnim: false,
                        playKillSound: true,
                        causeOfDeath: "Mosquito");
                    MosquitoRpc.ResetStingCooldown(ShooterId);

                    var colorHex = ColorUtility.ToHtmlStringRGB(Palette.ImpostorRed);
                    MiraAPI.Utilities.Helpers.CreateAndShowNotification(
                        $"<b><color=#{colorHex}>Your mosquito stung {target.Data.PlayerName}</color></b>",
                        Color.white,
                        new Vector3(0f, 1f, -20f),
                        spr: DivaniAssets.MosquitoIcon.LoadAsset());
                }
            }

            Destroy(gameObject);
            return;
        }

        if (!Aimbot && delta.magnitude <= 0.05f)
        {
            Destroy(gameObject);
            return;
        }

        _life += Time.deltaTime;
        if (_life > MaxLifetime)
        {
            Destroy(gameObject);
        }
    }

    [HideFromIl2Cpp]
    private void SetupClickable()
    {
        gameObject.layer = LayerMask.NameToLayer("Players");

        var collider = gameObject.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(3f, 3f);

        var passive = gameObject.AddComponent<PassiveButton>();
        passive.OnClick = new Button.ButtonClickedEvent();
        passive.OnClick.AddListener((Action)(() => OnSwatClicked()));
        passive.OnMouseOver = new UnityEvent();
        passive.OnMouseOut = new UnityEvent();
        passive.Colliders = (Il2CppReferenceArray<Collider2D>)new Collider2D[] { collider };
    }

    [HideFromIl2Cpp]
    private void OnSwatClicked()
    {
        if (_stung || _swatted)
        {
            return;
        }

        var local = PlayerControl.LocalPlayer;
        if (local == null || local.Data == null || local.Data.IsDead)
        {
            return;
        }

        _swatted = true;
        MosquitoRpc.RpcSwatMosquito(local, ShooterId);
    }

    [HideFromIl2Cpp]
    public void Swat()
    {
        if (_swatSoundPlayed)
        {
            return;
        }

        _swatSoundPlayed = true;
        PlaySwatSound();
        Destroy(gameObject);
    }

    [HideFromIl2Cpp]
    private void PlaySwatSound()
    {
        if (SoundManager.Instance == null)
        {
            return;
        }

        var clip = DivaniAssets.MosquitoSwatSound.LoadAsset();
        if (clip == null)
        {
            return;
        }

        var local = PlayerControl.LocalPlayer;
        if (local == null || local.Data == null)
        {
            return;
        }

        var distance = Vector2.Distance(local.GetTruePosition(), (Vector2)transform.position);
        var volume = Mathf.Clamp01(1f - (distance / 5f));
        if (volume <= 0f)
        {
            return;
        }

        SoundManager.Instance.PlaySound(clip, false, volume);
    }

    [HideFromIl2Cpp]
    public static void DestroyAll()
    {
        foreach (var mosquito in UnityEngine.Object.FindObjectsOfType<MosquitoObject>())
        {
            if (mosquito != null)
            {
                UnityEngine.Object.Destroy(mosquito.gameObject);
            }
        }
    }

    [HideFromIl2Cpp]
    private static PlayerControl? GetPlayer(byte id)
    {
        return GameData.Instance == null ? null : GameData.Instance.GetPlayerById(id)?.Object;
    }
}
